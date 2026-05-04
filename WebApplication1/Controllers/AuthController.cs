using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication1.Models.Context;
using WebApplication1.Models.Tables;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        public JsonResult Login(admin data)
        {
            try
            {
                using(var connect = new DB_Context())
                {
                    var admin = connect.admin.Where(x =>
                        x.email == data.email &&
                        x.password == data.password
                        ).FirstOrDefault();
                    if(admin != null)
                    {

                        string code = new Random().Next(100000, 999999).ToString(); // Generate a random 6-digit code

                        Session["VerificationCode"] = code; // Store the code in session for later verification
                        Session["VerifyEmail"] = data.email; // Store the email in session for later use
                        Session["VerifyExpiry"] = DateTime.Now.AddMinutes(5); // Set code expiry time to 5 minutes

                        EmailServices.SendEmailVerification(data.email, code); // Send the code to the user's email

                        TempData["Email"] = data.email;
                        return Json(new { success = true, message = "Verification code sent to " + data.email  }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid email or password." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex) { 
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult VerifyCode(verifycode data)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("data is: " + data.code);
                string savedCode = Session["VerificationCode"] as string;
                DateTime? expiry = Session["VerifyExpiry"] as DateTime?;

                if (savedCode == null || expiry == null)
                {
                    return Json(new { success = false, message = "Session Expired" }, JsonRequestBehavior.AllowGet);

                }
                if (DateTime.Now > expiry)
                {
                    return Json(new { success = false, message = "Code Has Expired" }, JsonRequestBehavior.AllowGet);
                }
                if (data.code != savedCode)
                {
                    return Json(new { success = false, message = "Invalid Code" }, JsonRequestBehavior.AllowGet);
                }
                Session["IsAuthenticated"] = true;
                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) });
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