﻿using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using RSPP.Helper;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Controllers
{
    public class AdminController : Controller
    {

        

        public RSPPdbContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        GeneralClass generalClass = new GeneralClass();
        HelperController _helpersController;
        WorkFlowHelper _workflowHelper;
        List<UserMaster> staffJsonList = new List<UserMaster>();

        private ILog log = log4net.LogManager.GetLogger(typeof(CompanyController));

        [Obsolete]
        private readonly IHostingEnvironment _hostingEnv;


        [Obsolete]
        public AdminController(RSPPdbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnv)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnv = hostingEnv;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
            _workflowHelper = new WorkFlowHelper(_context);

        }






        public ActionResult Index()
        {
            int totalPermitCount = 0;
            int totalAppWorkedOn = 0;
            int totalCancelled = 0;
            String errorMessage = null;

            ViewBag.LoggedInRole = _helpersController.getSessionRoleName();

            try
            {


                foreach (ApplicationRequestForm appRequest in _context.ApplicationRequestForm.ToList())
                {
                    switch (_context.WorkFlowState.Where(w => w.StateId == appRequest.CurrentStageId).FirstOrDefault().StateType)
                    {
                        case "COMPLETE":
                            totalPermitCount++;
                            break;
                        case "PROGRESS":
                            totalAppWorkedOn++;
                            break;
                        case "REJECTED":
                            totalCancelled++;
                            break;
                        default:
                            break;
                    }
                }
                log.Info("totalPermitCount =>" + totalPermitCount);
                log.Info("totalAppWorkedOn =>" + totalAppWorkedOn);
                log.Info("totalCancelled =>" + totalCancelled);

                List<ApplicationRequestForm> myAppDeskCountList = _helpersController.GetApprovalRequest(out errorMessage);
                log.Info("OnMyDeskCount =>" + myAppDeskCountList.Count);
                ViewBag.OnMyDeskCount = myAppDeskCountList.Count;
                ViewBag.TotalApplicationWorkedOn = (from a in _context.ApplicationRequestForm where a.Status == "Processing" select a).ToList().Count();
                ViewBag.PermitCount = (from a in _context.ApplicationRequestForm where a.Status == "Approved" select a).ToList().Count();
                ViewBag.TotalRejection = (from a in _context.ApplicationRequestForm where a.Status == "Rejected" select a).ToList().Count();
                ViewBag.ErrorMessage = errorMessage;
                var pastthreeweek = _helpersController.GetApplicationForPastThreeWks();
                var pastfivedays = _helpersController.GetApplicationForPastFiveDays();
                ViewBag.Pastfivedays = pastfivedays;
                ViewBag.Pastwksapp = pastthreeweek;
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured on Admin DashBoard, Please try again Later";
            }

            return View();
        }






        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetChart(RatioDash ratio)
        {

            int inprogressOnline = (from a in _context.ApplicationRequestForm where a.Status == "Processing" select a).ToList().Count();
            int issued = (from a in _context.ApplicationRequestForm where a.LicenseReference != null select a).ToList().Count();
            int approved = (from a in _context.ApplicationRequestForm where a.Status == "Approved" select a).ToList().Count();
            int rejected = (from a in _context.ApplicationRequestForm where a.Status == "Rejected" select a).ToList().Count();
            ratio.Approved = approved;
            ratio.OnDesk = (from a in _context.ApplicationRequestForm where a.LastAssignedUser == _helpersController.getSessionEmail() select a).ToList().Count();
            ratio.Processed = issued;
            ratio.Rejected = rejected;
            ratio.OnlineInProgress = inprogressOnline;
            return Json(ratio);
        }







        [HttpPost]
        public ActionResult NewUser(UserMaster user)
        {
            try
            {

                var checkexistence = (from u in _context.UserMaster where u.UserEmail == user.UserEmail select u).FirstOrDefault();
                if (checkexistence != null)
                {
                   return Content("<html><head><script>alert('hghhghugygfyff')</script></head></html>");
                   
                }
                else
                {
                    var splitfullname = Request.Form["staffFullName"];
                    var name = splitfullname.ToString().Split(' ');
                    var firstname = name[0];
                    var lastname = name[1];
                    var role = Request.Form["Userroles"];
                    var userdata = new UserMaster()
                    {
                        UserEmail = user.UserEmail,
                        UserType = "ADMIN",
                        UserRole = role,
                        FirstName = firstname,
                        LastName = lastname,
                        CreatedOn = DateTime.Now,
                        UpdatedBy = _helpersController.getSessionEmail(),
                        UpdatedOn = DateTime.Now,
                        Status = "ACTIVE",
                        LastLogin = DateTime.Now,
                    };
                    _context.UserMaster.Add(userdata);
                    _context.SaveChanges();
                    TempData["success"] = "User Was Successfully Added";
                }

            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }


            return RedirectToAction("StaffMaintenance", "Admin", "");
        }





        [HttpPost]
        public ActionResult EditStaff(UserMaster usr)
        {
            try
            {


                var user = (from u in _context.UserMaster where u.UserEmail == usr.UserEmail select u).FirstOrDefault();
                var staffname = Request.Form["Fullname"];
                var sname = staffname.ToString().Split(' ');
                var firstname = Request.Form["FirstName"];
                var lastname = Request.Form["LastName"];
                var staffrole = Request.Form["Userrole"];
                user.UserRole = staffrole;
                user.FirstName = firstname;
                user.LastName = lastname;
                _context.SaveChanges();
                TempData["success"] = "Staff Update was Successful";

            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }

            return RedirectToAction("StaffMaintenance");
        }





        [HttpPost]
        public ActionResult DeleteUser()
        {
            var email = Request.Form["useremail"];
            try
            {
                var useremail = (from u in _context.UserMaster where u.UserEmail == email select u).ToList();

                if (useremail != null)
                {
                    _context.UserMaster.Remove(useremail.FirstOrDefault());
                    _context.SaveChanges();
                }
                TempData["success"] = email + " was successfully deleted";
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message + " Unable to delete user " + email;
            }
            return RedirectToAction("StaffMaintenance");
        }





        [HttpPost]
        public ActionResult ActivateUser(FormCollection collect)
        {
            try
            {

                var usr = Request.Form["userID"];
                var deactive = Request.Form["Activate"];
                var location = Request.Form["UserLocationActivate"];
                var usermas = (from u in _context.UserMaster where u.UserEmail == usr select u).FirstOrDefault();
                usermas.Status = "ACTIVE";
                usermas.LastComment = deactive;

                _context.SaveChanges();
                TempData["success"] = usr + " was Successfully Activated";

            }

            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            return RedirectToAction("StaffMaintenance", "Admin", "");
        }









        [HttpPost]
        public ActionResult DeactivateUser()
        {
            try
            {

                var usr = Request.Form["userid"];
                var deactive = Request.Form["DeactivateComment"];
                var usermas = (from u in _context.UserMaster where u.UserEmail == usr select u).FirstOrDefault();
                usermas.Status = "PASSIVE";
                usermas.LastComment = deactive;
                _context.SaveChanges();
                TempData["success"] = usr + " was Successfully Deactivated";

            }

            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }


            return RedirectToAction("StaffMaintenance", "Admin", "");
        }







        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetAllUser()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from u in _context.UserMaster
                         where u.UserType != "COMPANY"

                         select new
                         {
                             u.UserEmail,
                             fullname = u.FirstName + " " + u.LastName,
                             u.UserRole,
                             u.UserType,
                             u.SignatureImage,
                             u.Status
                         });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(u => u.fullname.Contains(searchTxt) || u.UserEmail.Contains(searchTxt)
               || u.UserRole.Contains(searchTxt) || u.UserType.Contains(searchTxt) || u.Status.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.UserEmail + " " + sortColumnDir);
            }


            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        public ActionResult AllApplications(string location)
        {
            List<ApplicationRequestForm> apps = null;

            apps = (from a in _context.ApplicationRequestForm select a).ToList();

            return View(apps);
        }



        public ActionResult AllExtraPayment()
        {
            List<PaymentModel> extrapayment = new List<PaymentModel>();
            var paymt = (from p in _context.ExtraPayment
                         join a in _context.ApplicationRequestForm on p.ApplicationId equals a.ApplicationId
                         select new
                         {
                             p.Rrreference,
                             p.Status,
                             p.TransactionDate,
                             p.TxnAmount,
                             a.AgencyName,
                             p.ApplicationId,
                             a.CompanyEmail,
                             p.TransactionId,
                             p.ExtraPaymentBy
                         }).ToList();

            foreach (var item in paymt)
            {
                extrapayment.Add(new PaymentModel()
                {

                    RRReference = item.Rrreference,
                    Status = item.Status,
                    TransactionDate = item.TransactionDate,
                    TxnAmount = item.TxnAmount,
                    ApplicantName = item.AgencyName,
                    ApplicationID = item.ApplicationId,
                    CompanyUserId = item.CompanyEmail,
                    TransactionID = item.TransactionId,
                    ExtraPaymentBy = item.ExtraPaymentBy
                });
            }
            ViewBag.ExtraPaymentList = extrapayment;
            return View();
        }




        public ActionResult AllPayment()
        {
            List<PaymentModel> payment = new List<PaymentModel>();
            var paymt = (from p in _context.PaymentLog
                         join a in _context.ApplicationRequestForm on p.ApplicationId equals a.ApplicationId
                         select new
                         {
                             p.Rrreference,
                             p.Status,
                             p.TransactionDate,
                             p.TxnAmount,
                             a.AgencyName,
                             p.ApplicationId,
                             a.CompanyEmail,
                             p.Arrears,
                             p.TransactionId
                         }).ToList();

            foreach (var item in paymt)
            {
                payment.Add(new PaymentModel()
                {

                    RRReference = item.Rrreference,
                    Status = item.Status,
                    TransactionDate = item.TransactionDate,
                    TxnAmount = item.TxnAmount,
                    ApplicantName = item.AgencyName,
                    ApplicationID = item.ApplicationId,
                    CompanyUserId = item.CompanyEmail,
                    Arrears = item.Arrears,
                    TransactionID = item.TransactionId
                });
            }
            ViewBag.PaymentList = payment;
            return View();
        }





        public ActionResult AllStaffOutofOffice()
        {
            ViewBag.AllStaffOutofOfficeList = _helpersController.GetAllOutofOffice();
            return View();
        }




        [HttpGet]
        public ActionResult ApplicationDetails(string applicationId)
        {
            ApplicationRequestForm br = null;
            br = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
            if (br != null)
            {
                ViewBag.MyAgencyId = br.AgencyId;

                if (br.AgencyId == 1)
                {
                    ViewBag.Governmentagency = _helpersController.Governmentagency(applicationId);

                }
                else if (br.AgencyId == 2)
                {
                    ViewBag.LogisticsServiceProvider = _helpersController.LogisticsServiceProvider(applicationId);

                }
                else if (br.AgencyId == 3)
                {
                    ViewBag.PortOffDockTerminalOperator = _helpersController.PortOffDockTerminalOperator(applicationId);

                }
                else if (br.AgencyId == 4)
                {
                    ViewBag.ShippingAgency = _helpersController.ShippingAgency(applicationId);

                }
                else if (br.AgencyId == 5)
                {
                    ViewBag.OtherPortServiceProvider = _helpersController.OtherPortServiceProvider(applicationId);

                }
            }

            return View(br);
        }





        public ActionResult ApplicationReport()
        {
            int totalapplication = (from a in _context.ApplicationRequestForm select a).ToList().Count();
            TempData["totalapplication"] = totalapplication;
            return View();
        }





        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetApplicationReport()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from p in _context.ApplicationRequestForm

                         select new
                         {
                             p.ApplicationId,
                             p.Status,
                             p.CompanyEmail,
                             p.AgencyName,
                             LicenseIssuedDate = p.LicenseIssuedDate.ToString(),
                             LicenseExpiryDate = p.LicenseExpiryDate.ToString(),
                             expiryDATE = p.LicenseExpiryDate,
                             issuedDATE = p.LicenseIssuedDate
                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "All Company")
                {
                    staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
                }
                else
                {
                    staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt)
                   || a.Status.Contains(searchTxt) || a.AgencyName.Contains(searchTxt)
                   || a.LicenseIssuedDate.Contains(searchTxt) || a.LicenseExpiryDate.Contains(searchTxt));
                }
            }
            string firstdate = Request.Form["mymin"];
            string lastdate = Request.Form["mymax"];
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate);
                var maxdate = Convert.ToDateTime(lastdate);
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && a.issuedDATE >= mindate && a.issuedDATE <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }






        [HttpGet]
        public ActionResult GetApplicationChart(ApplicationRatio Appobj)
        {
            var applicationstatus = (from a in _context.ApplicationRequestForm select a).ToList();
            if (applicationstatus.Count > 0)
            {
                Appobj.Initiated = applicationstatus.Select(a=>a.Status=="ACTIVE").Count();
                Appobj.Approved = applicationstatus.Select(a => a.Status == "Approved").Count(); ;
                Appobj.Processing = applicationstatus.Select(a => a.Status == "Processing").Count();
                Appobj.Rejected = applicationstatus.Select(a => a.Status == "Rejected").Count();
            }

            return Json(Appobj);
        }







        [HttpGet]
        public ActionResult ChangePassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordModel model)
        {
            string responseMessage = null;

            try
            {
                AppResponse appResponse = _helpersController.ChangePassword(_helpersController.getSessionEmail(), model.OldPassword, model.NewPassword);
                log.Info("Response from Elps =>" + appResponse.message);
                if (appResponse.message.Trim() != "SUCCESS")
                {
                    responseMessage = "An Error Message occured during Service Call to Elps Server, Please try again Later";
                }
                else
                {
                    if (((bool)appResponse.value) == true)
                    {
                        {
                            responseMessage = "success";
                            TempData["success"] = _helpersController.getSessionEmail() + " password was successfully changed";
                        }
                    }
                    else
                    {
                        responseMessage = "Password Cannot Change, Kindly ensure your Old Password is correct and try again";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                responseMessage = "A General Error occured during Change Password";
            }

            return Json(new
            {
                Message = responseMessage
            }
             );
        }



        [HttpGet]
        public ActionResult Companies()
        {
            return View(_context.UserMaster.Where(u => u.UserType == "COMPANY").ToList());
        }




        [HttpGet]
        public ActionResult CompanyDocuments(string compId)
        {

            return View(_helpersController.CompanyDocument(compId));
        }





        [HttpGet]
        public ActionResult CompanyPermits(string userId)
        {
            var companypermit = (from a in _context.ApplicationRequestForm where a.CompanyEmail == userId && a.LicenseReference != null select a).ToList();
            return View(companypermit);
        }






        [HttpGet]
        public ActionResult CompanyProfile(string CompanyEmail)
        {

            var companydetails = (from a in _context.UserMaster where a.UserEmail == CompanyEmail select a).FirstOrDefault();

            ViewBag.AllCompanyDocument = _helpersController.CompanyDocument(CompanyEmail);

            return View(companydetails);
        }


        public ActionResult GenerateExtraPayment(FormCollection collection)
        {
            List<ApplicationRequestForm> apps = new List<ApplicationRequestForm>();
            var extra = (from a in _context.ApplicationRequestForm where a.LicenseReference == null select a).ToList();

            foreach (var item in extra)
            {
                apps.Add(new ApplicationRequestForm()
                {
                    ApplicationId = item.ApplicationId,
                    CompanyEmail = item.CompanyEmail,
                    CompanyAddress = item.CompanyAddress,
                    AgencyName = item.AgencyName,
                    AddedDate = item.AddedDate
                });
            }
            ViewBag.ExtraPaymentStage = apps;
            return View();

        }




        [HttpPost]
        public ActionResult AddExtraPayment()
        {
            var appid = Request.Form["myappid"];
            var status = Request.Form["status"];
            try
            {
                ApplicationRequestForm appmaster = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == appid).FirstOrDefault();
                var descrip = Request.Form["sanctiondescription"];
                var amount = Convert.ToDecimal(Request.Form["sanctionamount"]);
                var sanctype = Request.Form["sanctiontype"];
                string genApplicationId = generalClass.GenerateApplicationNo();
                var ExtraAmount = new ExtraPayment()
                {
                    ApplicationId = appid,
                    Description = descrip,
                    TxnAmount = amount,
                    ExtraPaymentAppRef = genApplicationId,
                    Arrears = 0,
                    LastRetryDate = DateTime.Now,
                    RetryCount = 1,
                    Status = status,
                    SanctionType = sanctype,
                    ApplicantId = appmaster.CompanyEmail,
                    ExtraPaymentBy = _helpersController.getSessionEmail()
                };
                _context.ExtraPayment.Add(ExtraAmount);
                int done = _context.SaveChanges();
                if (done > 0)
                {
                    TempData["GeneSuccess"] = "Extra payment was successfully generated for application with the reference number " + appid;
                    var extrapaymentdetails = (from e in _context.ExtraPayment where e.ApplicationId == appid select e).FirstOrDefault();
                    if (extrapaymentdetails != null)
                    {

                        var subject = "Extra Payment Generated";
                        var content = "YOU ARE REQUIRED TO MAKE EXTRA PAYMENT OF " + extrapaymentdetails.TxnAmount + " NAIRA FOR THE APPLICATION WITH REFERENCE NUMBER " + extrapaymentdetails.ApplicationId + ", AND YOUR REMITA REFRENCE NUMBER IS " + extrapaymentdetails.Rrreference;
                        var sendmail = generalClass.SendStaffEmailMessage(extrapaymentdetails.ApplicantId, subject, content);

                    }
                }
                else
                {
                    TempData["message"] = "something went wrong trying to generate extra payment";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            if (status == "Pending")
            {
                return RedirectToAction("ViewApplication", "Admin", new { applicationId = appid });
            }
            else
            {
                return RedirectToAction("GenerateExtraPayment", "Admin");
            }
        }



        public ActionResult GetRelievedStaffOutofOffice()
        {
            ViewBag.ReliverStaffOutofOfficeList = _helpersController.GetReliverStaffOutofOffice(_helpersController.getSessionEmail());
            return View();
        }




        [HttpPost]
        public ActionResult GetStaffStartOutofOffice()
        {
            string status = "";
            try
            {

                var today = DateTime.Now;
                List<OutofOffice> office = (from o in _context.OutofOffice where o.StartDate == today select o).ToList();
                foreach (var item in office)
                {
                    item.Status = "Started";
                }
                _context.SaveChanges();
                status = "done";
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                status = "failed";
            }
            return Json(status);
        }





        [HttpPost]
        public ActionResult EndLeave(OutofOffice office)
        {
            try
            {


                var Outoffice = (from u in _context.OutofOffice where u.Relieved == office.Relieved select u).FirstOrDefault();
                Outoffice.Status = "Finished";

                _context.SaveChanges();
                TempData["success"] = office.Relieved + " Successfully Ended Leave";

            }

            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            return RedirectToAction("OutOfOffice", "Admin", "");
        }











        public ActionResult ALLPermits()
        {
            return View();
        }


        [HttpPost]
        public ActionResult GetAllPermits()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;

            var staff = (from p in _context.ApplicationRequestForm where p.LicenseReference != null
                         
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.CompanyEmail,
                             p.CompanyAddress,
                             p.ApplicationTypeId,
                             p.AgencyName
                         });
               

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.AgencyName.Contains(searchTxt) || a.LicenseReference.Contains(searchTxt)
               || a.CompanyEmail.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }









        [HttpPost]
        public ActionResult ApplicationAcceptDecline(string applicationId, string myaction, string mycomment)//, string DocId
        {
            string response = string.Empty;
            ResponseWrapper responseWrapper;
            log.Info("UserAction => " + myaction);
            log.Info("Applications => " + applicationId);
            log.Info("UserComment => " + mycomment);

            var email = _helpersController.getSessionEmail();


            log.Info("AcceptDecline Parameters =>" + applicationId + "_" + myaction + "_" + mycomment);
            try
            {
                ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();

               


                if (appRequest == default(ApplicationRequestForm))
                {
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application ID with Reference " + applicationId + " Cannot be retrievd from the Database"
                    });
                }

                var approcom = mycomment == "" ? "" : "Comment => " + mycomment + ";";
                log.Info("Continuing with the Approval to Process Application");
                responseWrapper = _workflowHelper.processAction(appRequest.ApplicationId, myaction, email, (mycomment == "=> " || mycomment == "") ? "Application was proccessed by "+ email : mycomment);
                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    log.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    });
                }


            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return Json(new { status = "failure", Message = "An Exception occur during Transaction, Please try again Later" });
            }

            return Json(new { status = "success", Message = responseWrapper.value });
        }









        [HttpGet]

        public ActionResult WorkFlow()
        {
            List<WorkFlowNavigation> workflowInfo = new List<WorkFlowNavigation>();

            var workflowlist = (from a in _context.WorkFlowNavigation select a).ToList();


            foreach (var item in workflowlist)
            {
                workflowInfo.Add(new WorkFlowNavigation()
                {
                    WorkFlowId = item.WorkFlowId,
                    Action = item.Action,
                    ActionRole = item.ActionRole,
                    CurrentStageId = item.CurrentStageId,
                    NextStateId = item.NextStateId,
                    TargetRole = item.TargetRole,
                });
            }

            ViewBag.UserLoginEmail = _helpersController.getSessionRoleName();

            return View(workflowInfo);
        }



        [HttpPost]
        public ActionResult UpdateWorkFlowRecord(int WorkFlowId, string Action, string ActionRole, short CurrentStageID, short NextStateID, string TargetRole)
        {
            var result = "";
            try
            {
                var WorkflowRec = (from w in _context.WorkFlowNavigation where w.WorkFlowId == WorkFlowId select w).FirstOrDefault();

                if (WorkflowRec != null)
                {

                    WorkflowRec.Action = Action;
                    WorkflowRec.ActionRole = ActionRole;
                    WorkflowRec.CurrentStageId = CurrentStageID;
                    WorkflowRec.NextStateId = NextStateID;
                    WorkflowRec.TargetRole = TargetRole;
                    _context.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result);
        }



        [HttpPost]
        public ActionResult AddWorkFlowRecord(string Action, string ActionRole, short CurrentStageID, short NextStateID, string TargetRole)
        {
            var result = "";
            try
            {
                WorkFlowNavigation WorkflowRec = new WorkFlowNavigation();

                if (WorkflowRec != null)
                {

                    WorkflowRec.Action = Action;
                    WorkflowRec.ActionRole = ActionRole;
                    WorkflowRec.CurrentStageId = CurrentStageID;
                    WorkflowRec.NextStateId = NextStateID;
                    WorkflowRec.TargetRole = TargetRole;
                    _context.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result);
        }

















        [HttpGet]
        public ActionResult GiveValueList()
        {
            return View();
        }






        [HttpPost]
        public ActionResult GetGiveValue()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from p in _context.ApplicationRequestForm
                         where p.CurrentStageId == 2
                         select new
                         {
                             p.ApplicationId,
                             p.AgencyName,
                             p.CompanyEmail,
                             p.CompanyAddress
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt)
               || a.AgencyName.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;

            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }









        [HttpPost]
        public ActionResult GiveValue(string Appid)
        {
            decimal processFeeAmt = 0, statutoryFeeAmt = 0;
            string errorMessage = "";
            string status = "";
            var appRequest = (from a in _context.ApplicationRequestForm where a.ApplicationId == Appid select a).FirstOrDefault();

            /// decimal Arrears = commonHelper.CalculateArrears(Appid, userMaster.UserId, dbCtxt);
            try
            {
                //errorMessage = commonHelper.GetApplicationFees(appRequest, out processFeeAmt, out statutoryFeeAmt);
                log.Info("Response Message =>" + errorMessage);


                var paylog = (from l in _context.PaymentLog where l.ApplicationId == Appid select l).FirstOrDefault();

                if (paylog == null)
                {
                    PaymentLog paymentLog = new PaymentLog();
                    paymentLog.ApplicationId = appRequest.ApplicationId;
                    paymentLog.TransactionDate = DateTime.UtcNow;
                    paymentLog.TransactionId = "Value Given";
                    paymentLog.ApplicantId = appRequest.CompanyEmail;
                    paymentLog.TxnMessage = "Given";
                    paymentLog.Rrreference = "Value Given";
                    paymentLog.AppReceiptId = "Value Given";
                    paymentLog.TxnAmount = processFeeAmt + statutoryFeeAmt;
                    paymentLog.Arrears = 0;
                    paymentLog.Account = _context.Configuration.Where(c => c.ParamId == "ACCOUNT").FirstOrDefault().ParamValue.ToString();
                    paymentLog.BankCode = _context.Configuration.Where(c => c.ParamId == "BANKCODE").FirstOrDefault().ParamValue.ToString();
                    paymentLog.RetryCount = 0;
                    paymentLog.ActionBy = _helpersController.getSessionEmail();
                    paymentLog.Status = "AUTH";
                    log.Info("About to Add Payment Log");
                    _context.PaymentLog.Add(paymentLog);
                    _context.SaveChanges();
                    log.Info("Added Payment Log to Table");


                    status = "success";

                    log.Info("Saved it Successfully");
                }
                else
                {
                    paylog.Status = "AUTH";
                    _context.SaveChanges();
                }

                if (appRequest != null)
                {


                    ResponseWrapper responseWrapper = _workflowHelper.processAction(Appid, "Submit", appRequest.CompanyEmail, "Application was successfully sumbited after value given");
                }


            }
            catch (Exception ex) { ViewBag.message = ex.Message; }
            return Json(new { Status = status });
        }




        [HttpGet]
        public JsonResult GetCompanyNameAutoSearch(string term = "")
        {

            var CompanyName = (from a in _context.ApplicationRequestForm
                               where a.CompanyEmail.Contains(term)
                               group new { a.CompanyEmail } by new { a.CompanyEmail } into g
                               orderby g.Key.CompanyEmail

                               select new
                               {
                                   textvalue = g.Key.CompanyEmail,
                               }).ToList();
            return Json(CompanyName);


        }




        public ActionResult LicenseReport()
        {


            ViewBag.totalNewApp = (from s in _context.ApplicationRequestForm where s.LicenseReference != null && s.ApplicationTypeId == "NEW" select s).ToList().Count();

            ViewBag.totalReNewApp = (from s in _context.ApplicationRequestForm where s.LicenseReference != null && s.ApplicationTypeId == "RENEW" select s).ToList().Count();

            int totalLicense = (from s in _context.ApplicationRequestForm select s).ToList().Count();
            TempData["totalLicense"] = totalLicense;

            return View();
        }





        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetLicenseReport()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from p in _context.ApplicationRequestForm
                         where p.LicenseReference != null
                         select new
                         {
                             p.ApplicationId,
                             p.ApplicationTypeId,
                             p.CompanyEmail,
                             p.Status,
                             p.AgencyName,
                             LicenseIssuedDate = p.LicenseIssuedDate,
                             LicenseExpiryDate = p.LicenseExpiryDate,
                             issueddate = p.LicenseIssuedDate.ToString(),
                             expirydate = p.LicenseExpiryDate.ToString()

                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "All License")
                {
                    staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
                }
                else
                {
                    staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt)
                    || a.Status.Contains(searchTxt) || a.AgencyName.Contains(searchTxt) || a.issueddate.Contains(searchTxt) || a.expirydate.Contains(searchTxt));

                }
            }
            string firstdate = Request.Form["mymin"];
            string lastdate = Request.Form["mymax"];
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate);
                var maxdate = Convert.ToDateTime(lastdate);
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && a.LicenseIssuedDate >= mindate && a.LicenseIssuedDate <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }






        [HttpGet]
        public ActionResult MyDesk()
        {
            String errorMessage = null;
            List<ApplicationRequestForm> appRequestList = new List<ApplicationRequestForm>();
            try
            {
                appRequestList = _helpersController.GetApprovalRequest(out errorMessage);
                log.Info("Application Returned Count =>" + appRequestList.Count);
                ViewBag.ErrorMessage = errorMessage;
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured when calling MyDesk, Please try again Later";
            }

            return View(appRequestList);
        }








        public ActionResult PrintedLicense()
        {
            return View();
        }

        public ActionResult NonPrintedLicense()
        {
            return View();
        }





        [HttpPost]
        public ActionResult GetLicensePrinted()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;

            var staff = (from p in _context.ApplicationRequestForm
                         where p.LicenseReference != null && p.PrintedStatus == "Printed"
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.CompanyEmail,
                             p.CompanyAddress,
                             p.ApplicationTypeId,
                             p.AgencyName

                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.LicenseReference.Contains(searchTxt) || a.AgencyName.Contains(searchTxt)
               || a.CompanyEmail.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        [HttpPost]
        public ActionResult GetNonPrintedLicense()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;

            var staff = (from p in _context.ApplicationRequestForm
                         where p.LicenseReference != null && p.PrintedStatus == "Not Printed"
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.CompanyEmail,
                             p.CompanyAddress,
                             p.ApplicationTypeId,
                             p.AgencyName
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.LicenseReference.Contains(searchTxt) || a.AgencyName.Contains(searchTxt)
               || a.CompanyEmail.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyAddress.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.AgencyName).ToList() : data.OrderBy(p => p.AgencyName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyEmail).ToList() : data.OrderBy(p => p.CompanyEmail).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.CompanyAddress).ToList() : data.OrderBy(p => p.CompanyAddress).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }






        public ActionResult OutOfOffice()
        {
            ViewBag.UserID = _helpersController.getSessionEmail();

            staffJsonList = _context.UserMaster.Where(s => s.UserType != "COMPANY").ToList();
            return View();
        }




        [HttpPost]
        public ActionResult AddOutofOffice(OutofOffice usr)
        {
            try
            {

                var today = DateTime.Now;
                var outofoffice = new OutofOffice()
                {
                    Reliever = usr.Reliever,
                    Relieved = usr.Relieved,
                    EndDate = usr.EndDate,
                    StartDate = usr.StartDate,
                    Comment = usr.Comment
                };
                outofoffice.Status = usr.StartDate < today ? "Started" : "Starting";
                _context.OutofOffice.Add(outofoffice);
                _context.SaveChanges();

                TempData["success"] = "Update was Successful";

            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }

            return RedirectToAction("OutOfOffice", "Admin", "");
        }






        [HttpPost]
        public ActionResult DeleteOutofOffice(OutofOffice office)
        {
            try
            {
                var useremail = (from u in _context.OutofOffice where u.Relieved == office.Relieved select u).ToList();
                if (useremail != null)
                {
                    _context.OutofOffice.Remove(useremail.FirstOrDefault());
                    _context.SaveChanges();
                }

                TempData["success"] = "Out of office was successfully deleted";
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message + " Unable to delete user " + office.Relieved;
            }
            return RedirectToAction("OutOfOffice");
        }






        [HttpPost]
        public ActionResult EditOutofOffice(OutofOffice usr)
        {
            try
            {

                var outofoffice = (from u in _context.OutofOffice where u.Relieved == usr.Relieved select u).FirstOrDefault();
                outofoffice.Reliever = usr.Reliever;
                outofoffice.Relieved = usr.Relieved;
                outofoffice.EndDate = usr.EndDate;
                outofoffice.StartDate = usr.StartDate;
                outofoffice.Comment = usr.Comment;
                _context.SaveChanges();
                TempData["success"] = "Update was Successful";

            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }

            return RedirectToAction("OutOfOffice", "Admin", "");
        }







        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetOutofOfficeStaff()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from u in _context.OutofOffice
                         where u.Relieved == _helpersController.getSessionEmail()
                         select new
                         {
                             u.Reliever,
                             u.Relieved,
                             StartDate = u.StartDate.ToString(),
                             EndDate = u.EndDate.ToString(),
                             u.Comment,
                             u.Status
                         });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(u => u.Reliever.Contains(searchTxt) || u.Relieved.Contains(searchTxt)
               || u.EndDate.Contains(searchTxt) || u.StartDate.Contains(searchTxt) || u.Comment.Contains(searchTxt) || u.Status.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.Reliever + " " + sortColumnDir);
            }


            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        [HttpGet]
        public ActionResult UserIdAutosearch(string term = "")
        {
            List<UserMaster> staffJsonList1 = new List<UserMaster>();

            foreach (UserMaster staff in _context.UserMaster.Where(s => s.UserEmail.ToLower().Contains(term.ToLower())))
            {
                staffJsonList1.Add(new UserMaster() { UserEmail = staff.UserEmail, FirstName = staff.FirstName + " " + staff.LastName });
            }

            log.Info("Fetched Staff Count =>" + staffJsonList.Count);
            return Json(staffJsonList1);

        }




        public ActionResult PaymentList()
        {
            var mypayment = (from s in _context.PaymentLog where s.Status == "AUTH" select s).ToList();

            var totalpayment = mypayment.Sum(s => s.TxnAmount);
            decimal Sumtotalpayment = Convert.ToDecimal(totalpayment);




            TempData["totalpaymentcount"] = mypayment.ToList().Count();

            TempData["Sumtotalpayment"] = Sumtotalpayment.ToString("N") + " (" + generalClass.NumberToWords(Convert.ToInt64(Sumtotalpayment)) + ")";
            return View();
        }





        public ActionResult GetLicenseChart(LicenseRatio licenseobj)
        {
            int totalLicense = (from s in _context.ApplicationRequestForm where s.LicenseReference != null select s).ToList().Count();

            licenseobj.totalLicense = totalLicense;
            licenseobj.Legacy = 0;
            return Json(licenseobj);
        }




        [HttpPost]
        public ActionResult GetPaymentReport()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;


            var staff = (from p in _context.PaymentLog
                         join r in _context.ApplicationRequestForm on p.ApplicationId equals r.ApplicationId
                         where p.Status == "AUTH"

                         select new
                         {
                             p.ApplicationId,
                             p.Status,
                             p.Rrreference,
                             PaymentId = p.PaymentId.ToString(),
                             TxnAmount = p.TxnAmount.ToString(),
                             TransactionDate = p.TransactionDate.ToString(),
                             transDATE = p.TransactionDate,
                             r.ApplicationTypeId,
                             AgencyName = r.AgencyName,
                             CompanyEmail = r.CompanyEmail

                         });







            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (searchTxt == "All Payment")
            {
                staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
            }
            else
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.Status.Contains(searchTxt) || a.Rrreference.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.CompanyEmail.Contains(searchTxt) || a.AgencyName.Contains(searchTxt) || a.PaymentId.Contains(searchTxt) || a.TxnAmount.Contains(searchTxt)
               || a.TransactionDate.Contains(searchTxt));
            }


            string firstdate = Request.Form["mymin"];
            string lastdate = Request.Form["mymax"];
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && a.transDATE >= mindate && a.transDATE <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }






        [HttpGet]
        public ActionResult StaffDesk()
        {
            string ErrorMessage = "";
            StaffDeskModel model = new StaffDeskModel();
            List<StaffDesk> staffDeskList = new List<StaffDesk>();
            var desk = _context.UserMaster.Where(a => a.UserRole != "COMPANY" && a.UserRole != "SUPERADMIN" && a.UserRole != "ICT").ToList();
            foreach (UserMaster up in desk)
            {
                staffDeskList.Add(new StaffDesk()
                {
                    Role = up.UserRole,
                    StaffEmail = up.UserEmail,
                    StaffName = up.FirstName,
                    status = up.Status,
                    OnDesk = (from a in _context.ApplicationRequestForm where a.LastAssignedUser == up.UserEmail select a).Count()
                });

            }
            model.StaffDeskList = staffDeskList;
            ViewBag.ErrorMessage = ErrorMessage;
            ViewBag.UserRole = _helpersController.getSessionRoleName();
            return View(model);
        }




        [HttpGet]
        public ActionResult StaffTaskAssignment(string userid, string role)
        {
            string ErrorMessage = "";
            ViewBag.UserId = userid;
            ViewBag.Rolestaff = role;
            List<ApplicationRequestForm> appRequestList;
            UserMaster up = _context.UserMaster.Where(c => c.UserEmail.Trim() == userid.Trim()).FirstOrDefault();
            appRequestList = _helpersController.GetApprovalRequest(out ErrorMessage);
            ViewBag.ErrorMessage = ErrorMessage;
            return PartialView("TaskAssignment", appRequestList);
        }





        [HttpPost]
        public ActionResult Rerouteuser()
        {
            try
            {
                var arrayitem = Request.Form["arrayitem"].ToString();
                var adcomments = Request.Form["adcomment"];
                string[] itemlist = arrayitem.Split(',');
                var newassigned = Request.Form["newassigned"];
                if (arrayitem == "" || arrayitem == null)
                {
                    TempData["Message"] = "Application(s) to be Assigned Was not Selected";
                }
                else if (newassigned == "")
                {
                    TempData["Message"] = "Please Select New Assigned User";
                }
                else
                {
                    
                        foreach (var item in itemlist)
                        {
                            var apprequest = (from a in _context.ApplicationRequestForm where a.ApplicationId == item select a).FirstOrDefault();
                            var apphistry = (from a in _context.ActionHistory where a.ApplicationId == item select a).ToList().LastOrDefault();

                            if (apprequest != null)
                            {
                                apprequest.LastAssignedUser = newassigned;
                                apprequest.ModifiedDate = DateTime.Now;
                                _context.SaveChanges();
                                var subject = "Re-Routed Application";
                                string content = "Application with the reference number " + apprequest.ApplicationId + " has been re-routed to your desk.";
                                generalClass.SendStaffEmailMessage(newassigned, subject, content);

                            }
                            if (apphistry != null)
                            {
                                string v = adcomments == "" ? "Application was Re-Assigned to " + newassigned : adcomments.ToString();
                                apphistry.Message = v;
                                _context.SaveChanges();
                            }
                        }

                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("StaffDesk");

        }






        [HttpGet]
        public JsonResult AutoSearchCompanyAppId(string term = "")
        {

            var CompanyAppId = (from a in _context.PaymentLog where a.ApplicationId.Contains(term) select new { textvalue = a.ApplicationId }).ToList();
            return Json(CompanyAppId);
        }



        [HttpGet]
        public ActionResult StaffMaintenance()
        {

            //staffJsonList = _context.UserMaster.Where(s => s.UserType != "COMPANY").ToList();

            return View();
        }







        [HttpGet]
        public JsonResult GetUserRole()
        {

            var userRole = (from r in _context.Role
                            where r.RoleId != "COMPANY"
                            select new
                            {
                                role = r.RoleId
                            }).ToList();
            return Json(userRole);

        }







        [HttpGet]
        public ActionResult StaffReport()
        {
            int totalstaff = (from s in _context.UserMaster where s.UserType != "COMPANY" select s).ToList().Count();
            TempData["totalstaff"] = totalstaff;

            return View();
        }




        [HttpPost]
        public ActionResult GetStaffReport()
        {
            var role = TempData["role"];
            var field = TempData["field"];
            var zone = TempData["zone"];
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var staff = (from u in _context.UserMaster where u.UserType != "COMPANY"
                                                          select new
                                                          {
                                                              u.UserEmail,
                                                              Fullname = u.FirstName + " " + u.LastName,
                                                              u.UserRole,
                                                              u.Status,
                                                             
                                                          });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(u => u.UserEmail + " " + sortColumnDir);
            }

            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "ALLRoles")
                {
                    staff = staff.Where(s => s.UserRole == s.UserRole);
                }
                else
                {
                    staff = staff.Where(u => u.UserEmail.Contains(searchTxt) || u.Fullname.Contains(searchTxt)
                   || u.UserRole.Contains(searchTxt) || u.Status.Contains(searchTxt));

                }
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }





        [HttpPost]
        public JsonResult GetNewAssignedUser(string myrole, string myoldemail)
        {

            var Newuseremail = (from r in _context.UserMaster
                                where r.UserRole == myrole && r.Status == "ACTIVE" && r.UserEmail != myoldemail
                                select new
                                {
                                    newuser = r.UserEmail
                                }).ToList();
            return Json(Newuseremail);
        }







        [HttpPost]
        public ActionResult TaskDelegationList()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var searchTxt = Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            string staffemail = Request.Form["mystaffemail"];

            var Applications = (from a in _context.ApplicationRequestForm
                                where a.Status == "Processing" && staffemail.Contains(a.LastAssignedUser)
                                select new { a.ApplicationId, a.AgencyName, a.ApplicationTypeId, a.CompanyEmail, a.LastAssignedUser });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                Applications = Applications.Where(s => s.ApplicationId.Contains(searchTxt) || s.AgencyName.Contains(searchTxt) || s.ApplicationTypeId.Contains(searchTxt) || s.LastAssignedUser.Contains(searchTxt) || s.CompanyEmail.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                Applications = Applications.OrderBy(p => p.ApplicationId + " " + sortColumnDir);
            }


            totalRecords = Applications.Count();
            var data = Applications.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }





        [HttpGet]
        public ActionResult ViewStaffDesk(string userid)
        {
            string ErrorMessage = "";
            ViewBag.UserId = userid;
            List<ApplicationRequestForm> appRequestList;
            UserMaster up = _context.UserMaster.Where(c => c.UserEmail.Trim() == userid.Trim()).FirstOrDefault();
            appRequestList = _helpersController.GetApprovalRequests(up, out ErrorMessage);
            ViewBag.ErrorMessage = ErrorMessage;

            return PartialView("_StaffDesk", appRequestList);
        }





        [HttpGet]
        public ActionResult TransitionHistory(string applicationId)
        {

            List<ActionHistory> NotificationList = new List<ActionHistory>();
            try
            {
                NotificationList = _context.ActionHistory.Where(a => a.ApplicationId == applicationId).ToList();
                ViewBag.ApplicationID = applicationId;
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured when calling Transition History, Please try again Later";
            }

            return View(NotificationList);
        }






        public ActionResult UpdateFacilityInfo()
        {

            List<FacilityInfo> appInfo = new List<FacilityInfo>();

            var Updateinfo = (from a in _context.ApplicationRequestForm.AsEnumerable()
                              select new FacilityInfo
                              {
                                  CompanyEmail = a.CompanyEmail,
                                  AgencyName = a.AgencyName,
                                  ApplicationID = a.ApplicationId,
                                  ApplicationTypeId = a.ApplicationTypeId,
                                  AppliedDate = a.AddedDate.ToString(),
                                  CompanyAddress = a.CompanyAddress,
                                  CurrentStageId = Convert.ToInt16(a.CurrentStageId),
                                  LastAssignedUser = a.LastAssignedUser
                              }).ToList();

            appInfo = Updateinfo.GroupBy(x => x.ApplicationID).Select(x => x.LastOrDefault()).ToList();

            ViewBag.AllFacInformation = appInfo;



            return View(appInfo);

        }

        [HttpPost]
        public ActionResult UpdateFacilityRecord(string ApplicationId, string AgencyName, short CurrentStageId, string lastAssignedUser, string ApplicationType, string Companyemail, string CompanyAddress)
        {
            var result = "";

            try
            {
                var appinfo = (from a in _context.ApplicationRequestForm where a.ApplicationId == ApplicationId select a).FirstOrDefault();

                if (appinfo != null)
                {
                    appinfo.CompanyEmail = Companyemail;
                    appinfo.CompanyAddress = CompanyAddress;
                    appinfo.CurrentStageId = CurrentStageId;
                    appinfo.LastAssignedUser = lastAssignedUser;
                    appinfo.AgencyName = AgencyName;
                    appinfo.ApplicationTypeId = ApplicationType;

                    _context.SaveChanges();
                }

                    
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result);
        }








        [HttpGet]
        public ActionResult ViewApplication(string applicationId)
        {
            string errorMessage = string.Empty;
            ApplicationRequestForm appRequest = null;
            decimal processFeeAmt = 0, statutoryFeeAmt = 0, Arrears;

            try
            {
                var paymentlogdetails = (from a in _context.PaymentLog where a.ApplicationId == applicationId select a).FirstOrDefault();
                appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();

                ViewBag.Paid = false;
                Arrears = paymentlogdetails == null ? 0 : paymentlogdetails.Arrears;
                //Session["ViewApplicationAppId"] = applicationId;
                ViewBag.ExtraPayment = (from e in _context.ExtraPayment where e.ApplicationId == appRequest.ApplicationId select e).FirstOrDefault();
                if (ViewBag.ExtraPayment != null) { ViewBag.Amount = ViewBag.ExtraPayment.TxnAmount; ViewBag.PaymentStatus = ViewBag.ExtraPayment.Status; }

                var expirydate = (from a in _context.ApplicationRequestForm where a.CompanyEmail == appRequest.CompanyEmail && a.LicenseReference != null select a.LicenseExpiryDate).ToList().LastOrDefault();

                var ReceivedFrom = (from a in _context.ActionHistory where a.ApplicationId == applicationId select a).ToList().LastOrDefault();
                ViewBag.ReceivedFrom = ReceivedFrom.TriggeredBy + " (" + ReceivedFrom.TriggeredByRole + ")";
                ViewBag.LastMessage = ReceivedFrom.Message;
                ViewBag.ReceivedDate = (from a in _context.ApplicationRequestForm where a.ApplicationId == applicationId select a.ModifiedDate).FirstOrDefault();
                var CurrentDesk = appRequest.LastAssignedUser;
                ViewBag.Applicationstatus = appRequest.Status;
                ViewBag.Status = _context.WorkFlowState.Where(w => w.StateId == appRequest.CurrentStageId).FirstOrDefault().StateName;
                var currentdeskname = (from u in _context.UserMaster where u.UserEmail == CurrentDesk select new { u.FirstName, u.LastName }).FirstOrDefault();
                ViewBag.CurrentDesk = CurrentDesk + " (" + currentdeskname.FirstName + " " + currentdeskname.LastName + ")";
                var counthist = _context.ActionHistory.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).ToList().Count();
                ViewBag.ActionHistoryList = counthist >= 5 ? _context.ActionHistory.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(5) : _context.ActionHistory.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(1);
                ViewBag.LoggedInUserRole = _helpersController.getSessionRoleName();

                if (expirydate != null)
                {
                    var ExpiredDate = expirydate.Value.Date;
                    ViewBag.ExpiredDate = ExpiredDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                var TodayDate = DateTime.Now.Date;
                ViewBag.TodayDate = TodayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                UserMaster applicantMaster = _context.UserMaster.Where(c => c.UserEmail.Trim() == appRequest.CompanyEmail).FirstOrDefault();
                log.Info("About to Get Application Fees");


                if (paymentlogdetails != null)
                {

                    ViewBag.TotalAmount = (Convert.ToDecimal(paymentlogdetails.TxnAmount) + Arrears + statutoryFeeAmt).ToString("N");

                }

                ViewBag.CompanyUploadedDocument = _helpersController.UploadedCompanyDocument(appRequest.ApplicationId);




                if (_helpersController.isPaymentMade(applicationId, out errorMessage))
                {
                    ViewBag.Paid = true;
                }
                ViewBag.ShowPart = false;
                ViewBag.Arrears = Arrears.ToString("N");
                ViewBag.AppRequest = appRequest;
                ViewBag.ApplicationId = appRequest.ApplicationId;
                ViewBag.ErrorMessage = "SUCCESS";
            }catch(Exception ex) { ViewBag.ErrorMessage = ex.Message; }

            return View(appRequest);
        }







        [HttpGet]
        public ActionResult ApproveLicense(string applicationId, string myaction, string mycomment)
        {
            ResponseWrapper responseWrapper;
            string response = string.Empty;
            
            string roleid = _helpersController.getSessionRoleName();
            string comment = string.Empty;
            try
            {
                ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequestForm))
                {
                    return Content("Application ID with Reference " + applicationId + " Cannot be retrievd from the Database");
                }

                if (roleid.Contains("REGISTRAR"))
                {
                    if (myaction == "Accept")
                    {
                        comment = "Certificate Has Been Issued";
                    }
                    else if (myaction == "Reject")
                    {
                        comment = "Your Application has been Rejected";
                    }
                }

                log.Info("Continuing with the Approval to Process Application");
                responseWrapper = _workflowHelper.processAction(appRequest.ApplicationId, myaction, _helpersController.getSessionEmail(), (string.IsNullOrEmpty(mycomment)) ? comment : mycomment);
                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    log.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return Json(new { status = "failure", Message = "An Exception occur during Transaction, Please try again Later" });
            }
            GenerateLicenseDocument(applicationId);
            return Json(new { status = "success", Message = responseWrapper.value });
        }




        private void GenerateLicenseDocument(string applicationId)
        {
            try
            {
               
               _helpersController.GenerateCertificate(applicationId, _helpersController.getSessionEmail());
                
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }

        }






        public ActionResult ViewCertificate(string id)
        {
            var Host = HttpContext.Request.Scheme + "//" + HttpContext.Request.Host + "" + "" + HttpContext.Request.PathBase;

            return _helpersController.ViewCertificate(id, Host);
        }



    }
}
