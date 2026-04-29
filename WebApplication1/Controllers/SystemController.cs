using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
    }
}