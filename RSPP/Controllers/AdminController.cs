using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using RSPP.Helper;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Controllers
{
    public class AdminController : Controller
    {



        public RSPPdbContext _context;
        private WorkFlowHelper workflowHelper;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        GeneralClass generalClass = new GeneralClass();
        HelperController _helpersController;
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
        }






        public ActionResult Index()
        {
            int totalPermitCount = 0;
            int totalAppWorkedOn = 0;
            int totalCancelled = 0;
            String errorMessage = null;


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
                    TempData["AlreadyExist"] = "User Already Exist";
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
                             issueddate = p.LicenseIssuedDate.ToString(),
                             expirydate = p.LicenseExpiryDate.ToString(),
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
                   || a.issueddate.Contains(searchTxt) || a.expirydate.Contains(searchTxt));
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
        public ActionResult CompanyProfile(string ApplicationId, string CompanyEmail)
        {

            var companydetails = (from a in _context.ApplicationRequestForm select a).ToList();

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
                       

                        ResponseWrapper responseWrapper = workflowHelper.processAction(Appid, "Submit", appRequest.CompanyEmail, "Application was successfully sumbited after value given");
                    }

                
            }
            catch (Exception ex) { ViewBag.message = ex.Message; }
            return Json(new { Status = status});
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
                             issuedDATE = p.LicenseIssuedDate,
                             expiryDATE = p.LicenseExpiryDate,
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
                    || a.Status.Contains(searchTxt) || a.AgencyName.Contains(searchTxt)|| a.issueddate.Contains(searchTxt) || a.expirydate.Contains(searchTxt));
                    
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













    }
}
