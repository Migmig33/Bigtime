using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Context;
using WebApplication1.Models.Tables;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        public JsonResult Login(LoginModelDTO data)
        {
            try
            {
                using (var connect = new DB_Context())
                {
                    // Check admin table first
                    var admin = connect.admin.Where(x =>
                        x.email == data.email &&
                        x.password == data.password
                    ).FirstOrDefault();

                    if (admin != null)
                    {
                        string code = new Random().Next(100000, 999999).ToString();
                        Session["VerificationCode"] = code;
                        Session["VerifyEmail"] = data.email;
                        Session["VerifyExpiry"] = DateTime.Now.AddMinutes(5);
                        Session["UserRole"] = 1; // Admin
                        EmailServices.SendEmailVerification(data.email, code);
                        TempData["Email"] = data.email;
                        return Json(new { success = true, role = 1, message = "Verification code sent to " + data.email }, JsonRequestBehavior.AllowGet);
                    }

                    // Check tenant table
                    var tenant = connect.tenant.Where(x =>
                        x.email == data.email &&
                        x.passwordHash == data.password
                    ).FirstOrDefault();

                    if (tenant != null)
                    {
                        string code = new Random().Next(100000, 999999).ToString();
                        Session["VerificationCode"] = code;
                        Session["VerifyEmail"] = data.email;
                        Session["VerifyExpiry"] = DateTime.Now.AddMinutes(5);
                        Session["UserRole"] = 2; // Tenant
                        EmailServices.SendEmailVerification(data.email, code);
                        TempData["Email"] = data.email;
                        return Json(new { success = true, role = 2, message = "Verification code sent to " + data.email }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = false, message = "User not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ErrorHandling(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult VerifyCode(verifycode data)
        {
            try
            {
                string savedCode = Session["VerificationCode"] as string;
                DateTime? expiry = Session["VerifyExpiry"] as DateTime?;

                if (savedCode == null || expiry == null)
                    return Json(new { success = false, message = "Session Expired" }, JsonRequestBehavior.AllowGet);

                if (DateTime.Now > expiry)
                    return Json(new { success = false, message = "Code Has Expired" }, JsonRequestBehavior.AllowGet);

                if (data.code != savedCode)
                    return Json(new { success = false, message = "Invalid Code" }, JsonRequestBehavior.AllowGet);

                Session["IsAuthenticated"] = true;

                // ✅ Set LoggedInTid so TenantPortal can identify the user
                int userRole = Convert.ToInt32(Session["UserRole"]);
                if (userRole == 2)
                {
                    string email = Session["VerifyEmail"] as string;
                    using (var connect = new DB_Context())
                    {
                        var tenant = connect.tenant.FirstOrDefault(x => x.email == email);

                        // If tenant is found, use their Tid. Otherwise, default to 1.
                        if (tenant != null)
                        {
                            Session["LoggedInTid"] = tenant.Tid;
                        }
                        else
                        {
                            Session["LoggedInTid"] = 1;
                        }
                    }
                }

                return Json(new { success = true, role = userRole }, JsonRequestBehavior.AllowGet);
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