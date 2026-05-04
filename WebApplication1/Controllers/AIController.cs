using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models.Context;
namespace WebApplication1.Controllers
{
    public class AIController : Controller
    {
        [HttpPost]
        public async Task<ActionResult> Chat(string message, string conversationHistory)
        {
            try
            {
                var apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
                var client = new HttpClient();
                using(var connect = new DB_Context())
                {
                    var units = connect.unit.ToList();
                    var tenants = connect.tenant.Where(t => t.status != "inactive").ToList();
                    var bookings = connect.booking.ToList();
                    var payments = connect.payment.ToList();
                    var maintenance = connect.maintenance_request.ToList();



                    // ===== BUILD CONTEXT FROM DB =====
                    var dbContext = $@"
                                        === GREEN RESIDENCES LIVE DATA ===

                                        UNITS ({units.Count} total):
                                        {string.Join("\n", units.Select(u => $"- {u.unitName} | Floor {u.floor} | {u.beds} | {u.sqm}sqm | ₱{u.price:N0}/mo | Status: {u.status} | Max Occupants: {u.maxOccupants}"))}

                                        TENANTS ({tenants.Count} active):
                                        {string.Join("\n", tenants.Select(t => $"- {t.name} | Unit: {t.unitId} | Lease: {t.leaseStart:MMM dd, yyyy} to {t.leaseEnd:MMM dd, yyyy} | Status: {t.status}"))}

                                        BOOKINGS ({bookings.Count} total):
                                        {string.Join("\n", bookings.Select(b => $"- {b.guestName} | {b.bookingDatetime:MMM dd, yyyy} | Status: {b.status}"))}

                                        PAYMENTS:
                                        - Total Paid:    ₱{payments.Where(p => p.status == "Paid").Sum(p => p.amount):N0}
                                        - Total Overdue: ₱{payments.Where(p => p.status == "Overdue").Sum(p => p.amount):N0}
                                        - Total Unpaid:  ₱{payments.Where(p => p.status == "Unpaid").Sum(p => p.amount):N0}

                                        MAINTENANCE REQUESTS:
                                        - Pending:     {maintenance.Count(m => m.status == "Pending")}
                                        - In Progress: {maintenance.Count(m => m.status == "In Progress")}
                                        - Resolved:    {maintenance.Count(m => m.status == "Resolved")}
        ";


                    // ===== FETCH DB DATA =====


                    // ===== PARSE HISTORY =====
                    var history = string.IsNullOrEmpty(conversationHistory)
                        ? new List<object>()
                        : Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(conversationHistory);

                    history.Add(new
                    {
                        role = "user",
                        parts = new[] { new { text = message } }
                    });

                    var requestBody = new
                    {
                        systemInstruction = new
                        {
                            parts = new[] { new { text = $@"You are a helpful assistant for Green Residences. 
                                                    You help tenants and guests with inquiries about units, bookings, payments, and maintenance requests. 
                                                    Be friendly and concise.
                                                    IMPORTANT: Do not use any markdown formatting. 
                                                    No asterisks, no bullet points, no bold, no headers, no symbols. 
                                                    Plain text only.
                                                    Answer questions based on the live data below. If the data does not contain the answer, say so politely.

                                                    {dbContext}" } }
                        },
                        contents = history,
                        generationConfig = new
                        {
                            maxOutputTokens = 1024,
                            temperature = 0.7
                        }
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";
                    var response = await client.PostAsync(url, content);
                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Gemini API Error ({response.StatusCode}): {result}"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    dynamic parsed = Newtonsoft.Json.JsonConvert.DeserializeObject(result);

                    if (parsed.candidates == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Gemini returned no candidates: {result}"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    string reply = parsed.candidates[0].content.parts[0].text;

                    history.Add(new
                    {
                        role = "model",
                        parts = new[] { new { text = reply } }
                    });

                    return Json(new
                    {
                        success = true,
                        reply = reply,
                        history = history
                    }, JsonRequestBehavior.AllowGet);
                }
              
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ErrorHandling(ex)
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public string ErrorHandling(Exception ex)
        {
            var errorMessage = $@"
            ===== ERROR DETAILS =====
            Message        : {ex.Message}
            Type           : {ex.GetType().FullName}
            Source         : {ex.Source}
            Stack Trace    : {ex.StackTrace}
            Inner Exception: {(ex.InnerException != null ? ex.InnerException.Message : "None")}
            Timestamp      : {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            =========================";

            return errorMessage;
        }
    }
}