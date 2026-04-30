using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models.Context;
using WebApplication1;
using WebApplication1.Models.Tables;
using System.Web.Helpers;
using Microsoft.Ajax.Utilities;
namespace WebApplication1.Controllers
{
    public class SystemController : Controller
    {
        // GET: System
        public ActionResult RentersHome()
        {
            return View("Renters/Index");
        }
        public ActionResult RentersBrowse()
        {
            return View("Renters/Browse");
        }
        public ActionResult RentersBookings()
        {
            return View("Renters/MyBookings");
        }
        public ActionResult AdminDashboard()
        {
            return View("Admin/Dashboard");
        }
        public ActionResult AdminUnits()
        {
            return View("Admin/Units");
        }
        public ActionResult AdminTenants()
        {
            return View("Admin/Tenants");
        }
        public ActionResult AdminBookings()
        {
            return View("Admin/Bookings");
        }
        public ActionResult AdminMaintenance()
        {
            return View("Admin/Maintenance");
        }
        public ActionResult AdminPayments()
        {
            return View("Admin/Payments");
        }
        public ActionResult AdminLogin()
        {
            return View();
        }
        // POST API
        public JsonResult SaveUnit(unit data, List<string> imageUrls, int? id)
        {
            System.Diagnostics.Debug.WriteLine("Data is: " + data.unitName);
            try
            {
                using(var connect = new DB_Context())
                {
                    var existingUnit = connect.unit.Where(x => x.Uid == 0).FirstOrDefault();
                    if(existingUnit == null)
                    {
                        //Insert if null
                        var unitData = new unit
                        {
                            unitName = data.unitName,
                            price = data.price,
                            beds = data.beds,
                            sqm = data.sqm,
                            floor = data.floor,
                            description = data.description,
                            videoUrl = data.videoUrl,
                            colorCode = data.colorCode,
                            status = data.status,
                            address = data.address,
                            maxOccupants = data.maxOccupants
                        };
                        connect.unit.Add(unitData);
                        connect.SaveChanges();

                        if(imageUrls != null && imageUrls.Any())
                        {
                            foreach(var url in imageUrls)
                            {
                                var unitImage = new unit_image
                                {
                                    Uid = unitData.Uid,
                                    imageUrl = url
                                };
                                connect.unit_image.Add(unitImage);
                            }
                            connect.SaveChanges();
                        }
                        
                    }
                    else
                    {
                        existingUnit.unitName = data.unitName;
                        existingUnit.price = data.price;
                        existingUnit.beds = data.beds;
                        existingUnit.sqm = data.sqm;
                        existingUnit.floor = data.floor;
                        existingUnit.description = data.description;
                        existingUnit.videoUrl = data.videoUrl;
                        existingUnit.colorCode = data.colorCode;
                        existingUnit.status = data.status;
                        existingUnit.address = data.address;
                        existingUnit.maxOccupants = data.maxOccupants;
                        connect.SaveChanges();
                        

                    }
                }
                return Json(new { success = true, message = "Unit Saved Successfully" },JsonRequestBehavior.AllowGet);
            }catch(Exception ex)
            {
                return Json(new {success = false, message = ErrorHandling(ex)}, JsonRequestBehavior.AllowGet);
            }
        }
        // Get API
        public JsonResult GetDashboardData()
        {
            try
            {
                using (var connect = new DB_Context())
                {
                    var requests = connect.maintenance_request
                         .Where(m => m.status == "Pending")
                         .Select(m => m.status).ToList();
                    var units = connect.unit
                        .Where(m => m.status == "active")
                        .Select(m => m.status).ToList();
                    var bookings = connect.booking
                        .Where(m => m.status == "Pending")
                        .Select(m => m.status).ToList();
                    var payments = connect.payment
                        .Where(m => m.status == "Paid")
                        .AsEnumerable()
                        .Sum(m => (decimal?)m.amount) ?? 0;
                    var raw = (from b in connect.booking
                                          join u in connect.unit on b.Uid equals u.Uid
                                          orderby b.bookingDatetime descending
                                          select new
                                          {
                                              id = b.id,
                                              name = b.guestName,
                                              unitName = u.unitName,
                                              bookingDate = b.bookingDatetime,
                                              status = b.status
                                          }).Take(5).ToList();
                    var recentBookings = raw.Select(b => new
                    {
                        id = b.id,
                        name = b.name,
                        unitName = b.unitName,
                        bookingDate = b.bookingDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        status = b.status
                    }).ToList();
                    var monthlyRevenue = connect.payment
                        .Where(x => x.status == "Paid" && x.paidDate != null)
                        .Select(x => new
                        {
                            Year = x.paidDate.Year,
                            Month = x.paidDate.Month,
                            x.amount
                        })
                        .AsEnumerable()
                        .GroupBy(x => new {x.Year, x.Month})
                        .Select(p => new
                        {
                            Year = p.Key.Year,
                            Month = p.Key.Month,
                            Revenue = p.Sum(x => x.amount)
                        })
                        .OrderBy(x => x.Year)
                        .ThenBy(x => x.Month)
                        .ToList();
                    var allUnits = connect.unit.ToList();
                    var allTenants = connect.tenant.ToList();
                    var allCoOccupants = connect.co_occupant.ToList();

                    var monthlyOccupant = allTenants
                        .Where(t => t.leaseStart != null && t.leaseEnd != null)
                        .Select(t => new
                        {
                            Year = ((DateTime)t.leaseStart).Year,
                            Month = ((DateTime)t.leaseStart).Month
                        })
                        .Distinct()
                        .OrderBy(x => x.Year)
                        .ThenBy(x => x.Month)
                        .Select(period =>
                        {
                            var tenantThisMonth = allTenants
                                .Where(t =>
                                    t.leaseStart != null &&
                                    t.leaseEnd != null &&
                                    (((DateTime)t.leaseStart).Year < period.Year ||
                                        (((DateTime)t.leaseStart).Year == period.Year &&
                                         ((DateTime)t.leaseStart).Month <= period.Month)) &&
                                    (((DateTime)t.leaseEnd).Year > period.Year ||
                                        (((DateTime)t.leaseEnd).Year == period.Year &&
                                         ((DateTime)t.leaseEnd).Month >= period.Month))
                                ).ToList();

                            var tenantIdsThisMonth = tenantThisMonth
                                .Select(t => t.Tid)
                                .ToList();

                            int tenantCount = tenantThisMonth.Count;
                            int coOccupantCount = allCoOccupants
                                .Count(c => tenantIdsThisMonth.Contains(c.Tid));

                            int occupied = tenantCount + coOccupantCount;
                            int totalCapacity = allUnits
                                .Where(u => u.status == "active")
                                .Sum(u => u.maxOccupants);

                            int vacant = Math.Max(0, totalCapacity - occupied);

                            return new
                            {
                                Year = period.Year,
                                Month = period.Month,
                                Occupied = occupied,
                                Vacant = vacant
                            };
                        })
                        .ToList();

                    return Json(new {success = true, activeUnitNum = units, pendingRequestNum = requests, pendingBookNum = bookings, totalPayment = payments, recentBookings = recentBookings, monthlyRevenue = monthlyRevenue, monthlyOccupant = monthlyOccupant,  message = "Get Data Success"}, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllUnit()
        {
            try
            {
                using (var connect = new DB_Context())
                {
                    var today = DateTime.Today;
                    var expiringThresHold = today.AddDays(45);

                    var units = connect.unit.ToList();
                    var tenants = connect.tenant.ToList();
                    var unitImages = connect.unit_image.ToList();

                    var result = units.Select(u =>
                    {
                        var tenant = tenants.FirstOrDefault(t =>
                        t.unitId == u.Uid &&
                        t.status != "inactive"
                        );


                        string occupancy;
                        if (tenant == null)
                        {
                            occupancy = "Vacant";
                        }
                        else if (tenant.leaseEnd <= expiringThresHold)
                        {
                            occupancy = "Expiring";
                        }
                        else
                        {
                            occupancy = "Occupied";
                        }
                        var image = unitImages
                           .Where(i => i.Uid == u.Uid)
                           .OrderBy(i => i.displayOrder)
                           .FirstOrDefault();

                        return new
                        {
                            u.Uid,
                            u.unitName,
                            u.price,
                            u.beds,
                            u.sqm,
                            u.floor,
                            u.description,
                            u.videoUrl,
                            u.colorCode,
                            u.status,
                            u.address,
                            u.maxOccupants,
                            occupancy,
                            imageUrl = image != null ? image.imageUrl : null
                        };
                    }).ToList();
                   
                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
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