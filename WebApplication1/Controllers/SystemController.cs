using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication1;
using WebApplication1.Models.Context;
using WebApplication1.Models.Tables;
using WebApplication1.Services;
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
        [HttpPost]
        public JsonResult SaveTenant(tenant tenantData, List<co_occupant> coOccupants, List<IdFileDto> idFiles, int? id)
        {
            System.Diagnostics.Debug.WriteLine("Saving Tenant: " + tenantData.name);
            try
            {
                using (var connect = new DB_Context())
                {
                    // Use the passed 'id' to check if we are updating or inserting
                    var existingTenant = connect.tenant.Where(x => x.Tid == id).FirstOrDefault();
                    if (existingTenant == null)
                    {
                        // --- 1. INSERT NEW TENANT ---

                        // Auto-generate a tenant number if it's empty
                        string newTenantNumber = tenantData.tenantNumber;
                        if (string.IsNullOrEmpty(newTenantNumber))
                        {
                            newTenantNumber = "T-" + DateTime.Now.ToString("yyyyMMddHHmm");
                        }

                        // MAP TO A FRESH DATABASE ENTITY
                        // This allows us to intercept the string dates and turn them into SQL DateTimes
                        var newDbTenant = new tenant
                        {
                            tenantNumber = newTenantNumber,
                            name = tenantData.name,
                            email = tenantData.email,
                            phone = tenantData.phone,
                            address = tenantData.address,
                            occupation = tenantData.occupation,
                            occupancyTypeId = tenantData.occupancyTypeId,
                            passwordHash = tenantData.passwordHash,
                            unitId = tenantData.unitId,
                            status = "Active",
                            isTerminated = 0,

                            leaseStart = tenantData.leaseStart,
                            leaseEnd = tenantData.leaseEnd
                        };

                        connect.tenant.Add(newDbTenant);
                        connect.SaveChanges(); // Saves to DB and generates the new Tid inside newDbTenant

                        // --- 2. UPDATE UNIT STATUS ---
                        if (newDbTenant.unitId > 0)
                        {
                            var assignedUnit = connect.unit.Where(u => u.Uid == newDbTenant.unitId).FirstOrDefault();
                            if (assignedUnit != null)
                            {
                                assignedUnit.status = "active";
                                connect.SaveChanges();
                            }
                        }

                        // --- 3. SAVE CO-OCCUPANTS ---
                        if (newDbTenant.occupancyTypeId == 2 && coOccupants != null && coOccupants.Any())
                        {
                            foreach (var occ in coOccupants)
                            {
                                // IMPORTANT: Use newDbTenant.Tid so it links to the newly created tenant
                                occ.Tid = newDbTenant.Tid;
                                connect.co_occupant.Add(occ);
                            }
                            connect.SaveChanges();
                        }

                        // --- 4. SAVE ID DOCUMENTS ---
                        if (idFiles != null && idFiles.Any())
                        {
                            foreach (var file in idFiles)
                            {
                                var doc = new tenant_document
                                {
                                    // IMPORTANT: Use newDbTenant.Tid here too!
                                    Tid = newDbTenant.Tid,
                                    fileName = file.name,
                                    fileUrl = file.url,
                                    fileType = "ID Document"
                                };
                                connect.tenant_document.Add(doc);
                            }
                            connect.SaveChanges();
                        }
                    }
                    else
                    {
                        // --- 5. UPDATE EXISTING TENANT ---
                        existingTenant.unitId = tenantData.unitId;
                        existingTenant.name = tenantData.name;
                        existingTenant.email = tenantData.email;
                        existingTenant.phone = tenantData.phone;
                        existingTenant.leaseStart = tenantData.leaseStart;
                        existingTenant.leaseEnd = tenantData.leaseEnd;
                        existingTenant.address = tenantData.address;
                        existingTenant.occupation = tenantData.occupation;
                        existingTenant.occupancyTypeId = tenantData.occupancyTypeId;

                        if (!string.IsNullOrEmpty(tenantData.passwordHash))
                        {
                            existingTenant.passwordHash = tenantData.passwordHash;
                        }

                        // --- 6. PROCESS CO-OCCUPANT DELETIONS (The Trash Can) ---
                        // If the frontend sent an array of IDs to delete, wipe them out
                        if (tenantData.deletedCoOccupants != null && tenantData.deletedCoOccupants.Any())
                        {
                            var recordsToDelete = connect.co_occupant
                                .Where(c => tenantData.deletedCoOccupants.Contains(c.id))
                                .ToList();

                            connect.co_occupant.RemoveRange(recordsToDelete);
                        }

                        // --- 7. PROCESS CO-OCCUPANT ADDITIONS & UPDATES ---
                        if (tenantData.occupancyTypeId == 2 && coOccupants != null)
                        {
                            foreach (var occ in coOccupants)
                            {
                                if (occ.id == 0) // It's a brand new row added during the edit
                                {
                                    occ.Tid = existingTenant.Tid;
                                    connect.co_occupant.Add(occ);
                                }
                                else // It already exists, just update the text fields
                                {
                                    var existingOcc = connect.co_occupant.Where(c => c.id == occ.id).FirstOrDefault();
                                    if (existingOcc != null)
                                    {
                                        existingOcc.name = occ.name;
                                        existingOcc.phone = occ.phone;
                                        existingOcc.address = occ.address;
                                    }
                                }
                            }
                        }
                        // Safety Catch: If they changed from "With Others" back to "Solo", delete all of their co-occupants
                        else if (tenantData.occupancyTypeId == 1)
                        {
                            var allExistingCoOccupants = connect.co_occupant.Where(c => c.Tid == existingTenant.Tid).ToList();
                            if (allExistingCoOccupants.Any())
                            {
                                connect.co_occupant.RemoveRange(allExistingCoOccupants);
                            }
                        }

                        // Save all changes (Tenant updates, Co-occupant deletes, additions, and edits) at once!
                        connect.SaveChanges();
                    }
                }
                return Json(new { success = true, message = "Tenant Saved Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Replace ErrorHandling(ex) with however you globally handle exceptions
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult ConfirmBooking(int id)
        {
            using (var connect = new DB_Context())
            {
                var booking = connect.booking.FirstOrDefault(b => b.id == id);
                if (booking != null)
                {
                    booking.status = "Confirmed";
                    connect.SaveChanges();

                    var unit = connect.unit.FirstOrDefault(u => u.Uid == booking.Uid);
                    var unitName = unit?.unitName ?? "the unit";

                    EmailServices.SendBookingConfirmed(
                        booking.guestEmail,
                        booking.guestName,
                        unitName,
                        booking.bookingDatetime
                    );

                    return Json(new { success = true, message = "Booking confirmed!" });
                }
                return Json(new { success = false, message = "Booking not found." });
            }
        }

        [HttpPost]
        public JsonResult DeclineBooking(int id, string reason)
        {
            using (var connect = new DB_Context())
            {
                var booking = connect.booking.FirstOrDefault(b => b.id == id);
                if (booking != null)
                {
                    booking.status = "Declined";
                    booking.cancelReason = reason;
                    connect.SaveChanges();

                    var unit = connect.unit.FirstOrDefault(u => u.Uid == booking.Uid);
                    var unitName = unit?.unitName ?? "the unit";

                    EmailServices.SendBookingDeclined(
                        booking.guestEmail,
                        booking.guestName,
                        unitName,
                        booking.bookingDatetime,
                        reason
                    );

                    return Json(new { success = true, message = "Booking declined." });
                }
                return Json(new { success = false, message = "Booking not found." });
            }
        }

        [HttpPost]
        public JsonResult ToggleBusyDate(string dateStr)
        {
            try
            {
                using (var connect = new DB_Context()) // Use your actual DbContext
                {
                    // Convert the string "2026-05-12" into a proper C# DateTime
                    if (DateTime.TryParse(dateStr, out DateTime parsedDate))
                    {
                        // Check if this exact date is already in the database
                        var existingDate = connect.busy_schedule.FirstOrDefault(b => b.busyDate == parsedDate);

                        if (existingDate != null)
                        {
                            // It exists! That means the admin clicked an already-busy day to free it up.
                            connect.busy_schedule.Remove(existingDate);
                            connect.SaveChanges();

                            return Json(new { success = true, action = "removed" });
                        }
                        else
                        {
                            // It doesn't exist! The admin wants to block this day off.
                            var newBusy = new busy_schedule
                            {
                                busyDate = parsedDate
                            };

                            connect.busy_schedule.Add(newBusy);
                            connect.SaveChanges();

                            return Json(new { success = true, action = "added" });
                        }
                    }

                    return Json(new { success = false, message = "Invalid date format." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RemoveBusyDate(string dateStr)
        {
            try
            {
                using (var connect = new DB_Context()) // Use your actual DB Context
                {
                    // CASE 1: Clear EVERYTHING
                    if (dateStr == "ALL")
                    {
                        var allDates = connect.busy_schedule.ToList();
                        connect.busy_schedule.RemoveRange(allDates);
                        connect.SaveChanges();

                        return Json(new { success = true, message = "All busy dates cleared." });
                    }

                    // CASE 2: Delete just a SINGLE date
                    else if (DateTime.TryParse(dateStr, out DateTime parsedDate))
                    {
                        var targetDate = connect.busy_schedule.FirstOrDefault(b => b.busyDate == parsedDate);

                        if (targetDate != null)
                        {
                            connect.busy_schedule.Remove(targetDate);
                            connect.SaveChanges();
                        }

                        return Json(new { success = true, message = "Busy date removed." });
                    }

                    return Json(new { success = false, message = "Invalid command or date format." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult SaveMaintenanceRequest(maintenance_request requestData, int? id)
        {
            try
            {
                using (var connect = new DB_Context())
                {
                    // 1. Intelligently find the correct Tenant ID based on the chosen Unit
                    var activeTenant = connect.tenant.FirstOrDefault(t =>
                        t.unitId == requestData.Uid && t.status != "inactive");

                    int tid = activeTenant != null ? activeTenant.Tid : 0;

                    // 2. CHECK IF INSERT OR UPDATE
                    if (requestData.id == 0)
                    {
                        var newRequest = new maintenance_request
                        {
                            Uid = requestData.Uid,
                            Tid = requestData.Tid, // <-- Direct assignment!
                            category = requestData.category,
                            priority = requestData.priority,
                            description = requestData.description,
                            status = "Pending",
                            reportedDate = DateTime.Now
                        };

                        connect.maintenance_request.Add(newRequest);
                        connect.SaveChanges();

                        return Json(new { success = true, newId = newRequest.id, message = "Added successfully" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // --- B. UPDATE EXISTING REQUEST ---
                        var existingRequest = connect.maintenance_request.FirstOrDefault(m => m.id == requestData.id);

                        if (existingRequest != null)
                        {
                            // Update the fields
                            existingRequest.Uid = requestData.Uid;
                            existingRequest.Tid = tid; // Re-evaluate tenant in case they changed the unit
                            existingRequest.category = requestData.category;
                            existingRequest.priority = requestData.priority;
                            existingRequest.description = requestData.description;

                            // Note: We deliberately do NOT update 'status' or 'reportedDate' here 
                            // so we don't accidentally overwrite them during a basic edit.

                            connect.SaveChanges();

                            return Json(new { success = true, newId = existingRequest.id, message = "Updated successfully" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Request not found" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
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
                           .ToList();

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
                            images = unitImages
                                .Where(i => i.Uid == u.Uid)
                                .OrderBy(i => i.displayOrder)
                                .Select(i => new {
                                    i.imageUrl
                                })
                                .ToList()
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
        public JsonResult GetAllTenant()
        {
            try
            {
                using (var connect = new DB_Context())
                {
                    var today = DateTime.Today;
                    var expiringThreshold = today.AddDays(45);

                    var tenants = connect.tenant.ToList();
                    var units = connect.unit.ToList();
                    var coOccupants = connect.co_occupant.ToList();
                    var tenantDocuments = connect.tenant_document.ToList();

                    // 1. The RAW query (Runs in SQL Server)
                    var raw = (from t in tenants
                               // Left join the unit to get the unit name safely
                               join u in units on t.unitId equals u.Uid into unitGroup
                               from u in unitGroup.DefaultIfEmpty()
                               select new
                               {
                                   t.Tid,
                                   t.tenantNumber,
                                   t.name,
                                   t.email,
                                   t.phone,
                                   t.address,
                                   t.occupation,
                                   t.status,
                                   t.leaseStart,
                                   t.leaseEnd,
                                   t.isTerminated,
                                   t.unitId,
                                   t.occupancyTypeId,
                                   t.passwordHash,
                                   unitName = u != null ? u.unitName : null,
                                   maxOccupants = u != null ? u.maxOccupants : 0,
                                   // Map your co-occupant model exactly
                                   coOccupantsList = coOccupants.Where(c => c.Tid == t.Tid).Select(c => new {
                                       c.id,
                                       c.Tid,
                                       c.name,
                                       c.phone,
                                       c.address
                                   }).ToList(),

                               
                                   additionalOccupantsCount = coOccupants.Count(c => c.Tid == t.Tid),
                                   idFilesCount = tenantDocuments.Count(d => d.Tid == t.Tid)
                               }).ToList();

                    var data = raw.Select(t =>
                    {
                        // Occupancy logic
                        var occupancyType = t.additionalOccupantsCount > 0 ? "with_others" : "solo";

                        // Days left logic
                        var daysLeft = (int?)(t.leaseEnd.Date - today).TotalDays;

                        // Live status logic
                        string liveStatus;
                        if (t.isTerminated == 1)
                            liveStatus = "Terminated";
                        else if (t.leaseEnd < today)
                            liveStatus = "Expired";
                        else if (t.leaseEnd <= expiringThreshold)
                            liveStatus = "Expiring";
                        else
                            liveStatus = "Active";

                        // Contract type logic
                        var leaseDurationMonths = ((t.leaseEnd.Year - t.leaseStart.Year) * 12) + t.leaseEnd.Month - t.leaseStart.Month;
                        string contractType;
                        if (leaseDurationMonths <= 3)
                            contractType = "Short-term";
                        else if (leaseDurationMonths <= 6)
                            contractType = "Mid-term";
                        else
                            contractType = "Long-term";

                        return new
                        {
                            Tid = t.Tid,
                            tenantNumber = t.tenantNumber,
                            name = t.name,
                            email = t.email,
                            phone = t.phone,
                            address = t.address,
                            occupation = t.occupation,
                            status = t.status,
                            coOccupants = t.coOccupantsList,
                            maxOccupants = t.maxOccupants,

                            // FORMAT DATES HERE: This fixes the Angular [ngModel:datefmt] error!
                            leaseStart = t.leaseStart.ToString("yyyy-MM-dd"),
                            leaseEnd = t.leaseEnd.ToString("yyyy-MM-dd"),

                            isTerminated = t.isTerminated == 1,
                            unit = t.unitName,
                            unitId = t.unitId,
                            passwordHash = t.passwordHash,
                            occupancyType = occupancyType,
                            occupancyTypeId = t.occupancyTypeId,
                            additionalOccupantsCount = t.additionalOccupantsCount,
                            daysLeft = daysLeft,
                            liveStatus = liveStatus,
                            contractType = contractType,
                            latestPaymentStatus = "None", // extend when payment table is available
                            idFilesCount = t.idFilesCount
                        };
                    }).ToList();

                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllBooking()
        {
            try
            {
                using (var connect = new DB_Context()) // Use your actual DbContext name
                {
                    // 1. Fetch Bookings and join with Units to get the UnitName
                    var raw = (from b in connect.booking
                                        join u in connect.unit on b.Uid equals u.Uid into unitJoin
                                        from u in unitJoin.DefaultIfEmpty()
                                        select new
                                        {
                                            id = b.id,
                                            name = b.guestName,       // Maps to {{ b.name }} in HTML
                                            email = b.guestEmail,     // Maps to {{ b.email }} in HTML
                                            phone = b.guestPhone,     // Maps to {{ b.phone }} in HTML
                                            datetime = b.bookingDatetime, // Maps to {{ b.datetime }}
                                            status = b.status,        // Maps to {{ b.status }}
                                            unitName = u != null ? u.unitName : "Unknown Unit"
                                        })
                                        .OrderByDescending(b => b.datetime)
                                        .ToList();
                    var bookingsData = raw.Select(x => new
                    {
                        id = x.id,
                        name = x.name,
                        email = x.email,
                        phone = x.phone,

                        // CHANGE THIS LINE: Add the 'T'
                        datetime = x.datetime.ToString("yyyy-MM-ddTHH:mm:ss"),

                        status = x.status,
                        unitName = x.unitName
                    });
                    // 2. Fetch Busy Dates
                    // NOTE: If you have a busy_schedule table, query it here. 
                    // If not, we just pass an empty list for now to prevent frontend errors.
                    var busyDatesList = connect.busy_schedule
                        .ToList() // This executes the query to grab them from SQL
                        .Select(b => b.busyDate.ToString("yyyy-MM-dd")) // Formats them for the calendar
                        .ToList();

                    return Json(new
                    {
                        success = true,
                        bookings = bookingsData,
                        busyDates = busyDatesList // Now contains your real database dates!
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllMaintenance()
        {
            try
            {
                using (var connect = new DB_Context())
                {
                    // 1. Fetch all tables into memory (matching your GetAllUnit pattern)
                    var maintenanceRequests = connect.maintenance_request.ToList();
                    var units = connect.unit.ToList();
                    var tenants = connect.tenant.ToList();

                    // 2. Map the maintenance requests
                    var requestsResult = maintenanceRequests.Select(m =>
                    {
                        // Find related unit and tenant
                        var unit = units.FirstOrDefault(u => u.Uid == m.Uid);
                        var tenant = tenants.FirstOrDefault(t => t.Tid == m.Tid);

                        return new
                        {
                            id = m.id,
                            category = m.category,
                            status = m.status,
                            description = m.description,

                            // Fallbacks in case the unit or tenant was deleted
                            unitName = unit != null ? unit.unitName : "Unknown Unit",
                            tenantName = tenant != null ? tenant.name : "Unknown Tenant",

                            // Format dates for the UI
                            reportedDate = m.reportedDate.ToString("MMM dd, yyyy"),
                            resolvedDate = m.resolvedDate == DateTime.MinValue ? null : m.resolvedDate.ToString("MMM dd, yyyy"),

                            // UI placeholders for fields that aren't in your DB model yet
                            photoUrl = (string)null,
                            scheduledDate = (string)null,
                            scheduledTime = (string)null
                        };
                    }).ToList();

                    // 3. Map the units for the "Add Request" dropdown
                    var unitsResult = units.Select(u => new
                    {
                        u.Uid,
                        u.unitName
                    }).ToList();

                    var tenantsResult = tenants.Select(t => new {
                        Tid = t.Tid,
                        name = t.name
                    }).ToList();

                    return Json(new
                    {
                        success = true,
                        requests = requestsResult,
                        units = unitsResult,
                        tenants = tenantsResult // <-- Send this to Angular!
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
            }
        }


        //Delete
        public JsonResult DeleteUnit(unit data)
        {
            System.Diagnostics.Debug.WriteLine("Data is " + data.Uid);
            try
            {
                using(var connect = new DB_Context())
                {
                    var deleteUnit = connect.unit.Where(u => u.Uid == data.Uid).FirstOrDefault();
                    var deleteUnitImage = connect.unit_image.Where(u => u.Uid == deleteUnit.Uid).ToList();
                    connect.unit.Remove(deleteUnit);
                    connect.unit_image.RemoveRange(deleteUnitImage);
                    connect.SaveChanges();
                }
                return Json(new { success = true, message = "Unit Successfully Deleted" });

            }catch(Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) });   
            }
        }
        public JsonResult DeleteTenant(tenant data)
        {
            System.Diagnostics.Debug.WriteLine("Data is " + data.Tid);
            try
            {
                using (var connect = new DB_Context())
                {
                    var deleteTenant = connect.tenant.Where(u => u.Tid == data.Tid).FirstOrDefault();
                    var deleteTenantDocu = connect.tenant_document.Where(u => u.Tid == deleteTenant.Tid).ToList();
                    connect.tenant.Remove(deleteTenant);
                    connect.tenant_document.RemoveRange(deleteTenantDocu);
                    connect.SaveChanges();
                }
                return Json(new { success = true, message = "Tenant Successfully Deleted" });

            }
            catch (Exception ex)
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