using log4net;
using RSPP.Helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Controllers
{
    public class CompanyController : Controller
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
        public CompanyController(RSPPdbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnv)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnv = hostingEnv;
            _helpersController = new HelperController(_context, _configuration, _httpContextAccessor);
        }
        public IActionResult Index()
        {
            string responseMessage = null;
            Dictionary<string, int> appStatistics;
            Dictionary<string, int> appStageReference = null;
            var companyemail = _helpersController.getSessionEmail();
            try
            {
                log.Info("About To Generate User DashBoard Information");
                ViewBag.TotalPermitCount = 0;
                ViewBag.ApplicationCount = 0;
                ViewBag.PermitExpiringCount = 0;
                ViewBag.ProcessedApplicationCount = 0;
                ViewBag.CompanyName = generalClass.Decrypt(_helpersController.getSessionCompanyName());

                var Rejectcomment = (from u in _context.UserMaster
                                     where u.UserEmail == _helpersController.getSessionEmail()
                                     join a in _context.ApplicationRequestForm on u.UserEmail equals a.CompanyEmail
                                     join ah in _context.ActionHistory on a.ApplicationId equals ah.ApplicationId
                                     orderby ah.ActionId descending
                                     select new { ah.Message, ah.Action }).FirstOrDefault();



                if (Rejectcomment != null)
                {
                    TempData["Rejectcomment"] = Rejectcomment.Message;
                    TempData["Acceptcomment"] = Rejectcomment.Action;
                }



                var extrapay = (from a in _context.ApplicationRequestForm
                                join e in _context.ExtraPayment on a.ApplicationId equals e.ApplicationId
                                where a.ApplicationId == e.ApplicationId && e.Status != "AUTH"
                                select new { e.TxnAmount, a.ApplicationId, a.CompanyEmail }).ToList().LastOrDefault();
                if (extrapay != null)
                {
                    ViewBag.ExtraPaymentAmount = extrapay.TxnAmount;
                    ViewBag.ExtraPaymentAPPID = extrapay.ApplicationId;
                    ViewBag.ExtraPay = extrapay;
                    ViewBag.LoggedInUser = _helpersController.getSessionEmail();
                    ViewBag.ExtraPaymentEmail = extrapay.CompanyEmail;
                }




                log.Info("About To Get Applications and Company Notification Messages");

                var userMaster = (from u in _context.UserMaster where u.UserEmail == companyemail select u).FirstOrDefault();

                ViewBag.AllMessages = _helpersController.GetCompanyMessages(_context, userMaster);

                ViewBag.AllUserApplication = _helpersController.GetApplications(userMaster.UserEmail, "ALL", out responseMessage);

                log.Info("GetApplications ResponseMessage => " + responseMessage);

                if (responseMessage == "SUCCESS")
                {

                    ViewBag.Allcomments = _helpersController.AllHistoryComment(_helpersController.getSessionEmail());

                    appStatistics = _helpersController.GetApplicationStatistics(_helpersController.getSessionEmail(), out responseMessage, out appStageReference);
                    ViewBag.TotalPermitCount = appStatistics["PEM"];
                    ViewBag.ApplicationCount = appStatistics["ALL"];
                    ViewBag.PermitExpiringCount = appStatistics["EXP"];
                    ViewBag.ProcessedApplicationCount = appStatistics["PROC"];

                    //ViewBag.AllUserApplication = _helpersController.GetApplications(_helpersController.getSessionEmail(), "ALL", out responseMessage);
                    //ViewBag.AllUserApplication = _helpersController.GetUnissuedLicense(_helpersController.getSessionEmail(), "ALL", out responseMessage);

                    log.Info("GetApplicationStatistics ResponseMessage=> " + responseMessage);
                }

                log.Info("GetApplicationStatistics Count => " + appStageReference.Count);
                ViewBag.StageReferenceList = appStageReference;
                ViewBag.ErrorMessage = responseMessage;

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
                ViewBag.ErrorMessage = "Error Occured Getting the Company DashBoard, Please Try again Later";
            }

            return View();
        }


        public IActionResult ApplicationForm(string ApplicationId = null)
        {
            ApplicationRequestForm appdetail = new ApplicationRequestForm();
            var agencyid = 2;//_helpersController.sessionAgencyId();
            var AgencyId = Convert.ToInt32(agencyid);
            ViewBag.AgencyID = AgencyId;
            var appdetails = (from a in _context.ApplicationRequestForm where a.ApplicationId == ApplicationId select a).FirstOrDefault();
            var govagencydetails = (from a in _context.GovernmentAgency where a.ApplicationId == ApplicationId select a).FirstOrDefault();
            var logisticsserviceprovider = (from a in _context.LogisticsServiceProvider where a.ApplicationId == ApplicationId select a).FirstOrDefault();
            var otherportserviceprovider = (from a in _context.OtherPortServiceProvider where a.ApplicationId == ApplicationId select a).FirstOrDefault();
            var portoffdockserviceprovider = (from a in _context.PortOffDockTerminalOperator where a.ApplicationId == ApplicationId select a).FirstOrDefault();
            var shippingagency = (from a in _context.ShippingAgency where a.ApplicationId == ApplicationId select a).FirstOrDefault();

            if (appdetails != null)
            {
                ViewBag.ApplicationId = appdetails.ApplicationId;
                appdetail.AgencyId = AgencyId;
                appdetail.ApplicationId = ApplicationId;
                appdetail.AgencyName = appdetails.AgencyName;
                appdetail.DateofEstablishment = appdetails.DateofEstablishment;
                appdetail.CompanyAddress = appdetails.CompanyAddress;
                appdetail.PostalAddress = appdetails.PostalAddress;
                appdetail.PhoneNum = appdetails.PhoneNum;
                appdetail.CompanyEmail = appdetails.CompanyEmail;
                appdetail.CompanyWebsite = appdetails.CompanyWebsite;
            }

            if (govagencydetails != null && AgencyId == 1)
            {
                ViewBag.ServicesProvidedInPort = govagencydetails.ServicesProvidedInPort;
                ViewBag.AnyOtherRelevantInfo = govagencydetails.AnyOtherRelevantInfo;
            }


            if (logisticsserviceprovider != null && AgencyId == 2)
            {
                ViewBag.LineOfBusiness = logisticsserviceprovider.LineOfBusiness;
                ViewBag.CustomLicenseNum = logisticsserviceprovider.CustomLicenseNum;
                ViewBag.CrffnRegistrationNum = logisticsserviceprovider.CrffnRegistrationNum;
                ViewBag.AnyOtherInfo = logisticsserviceprovider.AnyOtherInfo;
                ViewBag.OtherLicense = logisticsserviceprovider.OtherLicense;
                ViewBag.CustomLicenseExpiryDate = logisticsserviceprovider.CustomLicenseExpiryDate;
                ViewBag.CrffnRegistratonExpiryDate = logisticsserviceprovider.CrffnRegistratonExpiryDate;
                ViewBag.OtherLicenseExpiryDate = logisticsserviceprovider.OtherLicenseExpiryDate;
            }

            if (portoffdockserviceprovider != null && AgencyId == 3)
            {
                ViewBag.portoffdockLineOfBusiness = portoffdockserviceprovider.LineOfBusiness;
                ViewBag.StatusOfTerminal = portoffdockserviceprovider.StatusOfTerminal;
                ViewBag.portoffdockCargoType = portoffdockserviceprovider.CargoType;
                ViewBag.portoffdockAnyOtherInfo = portoffdockserviceprovider.AnyOtherInfo;

                List<PortOffDockTerminalOperator> terminal = new List<PortOffDockTerminalOperator>();

                var Terminals = (from t in _context.PortOffDockTerminalOperator
                                 where t.ApplicationId == ApplicationId
                                 select t).ToList();

                foreach (var item in Terminals)
                {
                    terminal.Add(new PortOffDockTerminalOperator()
                    {
                        NameOfTerminal = item.NameOfTerminal,
                        LocationOfTerminal = item.LocationOfTerminal
                    });

                }

                ViewBag.TerminalList = Terminals;
            }

            if (shippingagency != null && AgencyId == 4)
            {
                ViewBag.ShippingagencyCargoType = shippingagency.CargoType;
                ViewBag.VesselLinesRepresentedInNigeria = shippingagency.VesselLinesRepresentedInNigeria;
                ViewBag.LineOfBusiness = shippingagency.LineOfBusiness;
                ViewBag.ShippingagencyAnyOtherInfo = shippingagency.AnyOtherInfo;
            }

            if (otherportserviceprovider != null && AgencyId == 5)
            {
                ViewBag.otherportLineOfBusiness = otherportserviceprovider.LineOfBusiness;
                ViewBag.otherportAnyOtherInfo = otherportserviceprovider.AnyOtherInfo;
            }

            return View(appdetail);
        }


        [HttpPost]
        public JsonResult ApplicationForm(ApplicationRequestForm model, List<Terminal> MyTerminals)
        {
            string status = string.Empty;
            string message = string.Empty;
            ApplicationRequestForm appdetails = null;
            GovernmentAgency govagencydetails = null;
            LogisticsServiceProvider logisticsserviceprovider = null;
            OtherPortServiceProvider otherportserviceprovider = null;
            PortOffDockTerminalOperator portoffdockserviceprovider = null;
            ShippingAgency shippingagency = null;
            var generatedapplicationid = generalClass.GenerateApplicationNo();
            var checkappexist = (from a in _context.ApplicationRequestForm where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
            int agencyId = Convert.ToInt32(Request.Form["txtagency"]);

            try
            {
                var agencyid = _helpersController.getSessionEmail();
                appdetails = checkappexist == null ? new ApplicationRequestForm() : (from a in _context.ApplicationRequestForm where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
                govagencydetails = (from a in _context.GovernmentAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new GovernmentAgency() : (from a in _context.GovernmentAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
                logisticsserviceprovider = (from a in _context.LogisticsServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new LogisticsServiceProvider() : (from a in _context.LogisticsServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
                otherportserviceprovider = (from a in _context.OtherPortServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new OtherPortServiceProvider() : (from a in _context.OtherPortServiceProvider where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
                portoffdockserviceprovider = (from a in _context.PortOffDockTerminalOperator where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new PortOffDockTerminalOperator() : (from a in _context.PortOffDockTerminalOperator where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();
                shippingagency = (from a in _context.ShippingAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault() == null ? new ShippingAgency() : (from a in _context.ShippingAgency where a.ApplicationId == model.ApplicationId select a).FirstOrDefault();

                appdetails.ApplicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId;
                appdetails.AgencyName = model.AgencyName;
                appdetails.DateofEstablishment = model.DateofEstablishment;
                appdetails.CompanyAddress = model.CompanyAddress;
                appdetails.PostalAddress = model.PostalAddress;
                appdetails.PhoneNum = model.PhoneNum;
                appdetails.CompanyEmail = model.CompanyEmail;
                appdetails.CompanyWebsite = model.CompanyWebsite;
                appdetails.AgencyId = agencyId;
                appdetails.CurrentStageId = 1;

                if (checkappexist == null)
                {
                    _context.Add(appdetails);
                }

                if (agencyId != 0)
                {

                    if (agencyId == 1)//government Agency
                    {
                        govagencydetails.ApplicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId;
                        govagencydetails.ServicesProvidedInPort = Request.Form["ServicesProvidedInPort"].ToString();
                        govagencydetails.AnyOtherRelevantInfo = Request.Form["AnyOtherRelevantInfo"].ToString();

                        if (govagencydetails == null)
                        {
                            _context.Add(govagencydetails);
                        }
                    }
                    else if (agencyId == 2)//Logistics Services Providers
                    {
                        logisticsserviceprovider.ApplicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId;
                        logisticsserviceprovider.LineOfBusiness = Request.Form["LogisticsLineOfBusiness"].ToString();
                        logisticsserviceprovider.CustomLicenseNum = Request.Form["CustomLicenseNum"].ToString();
                        logisticsserviceprovider.CrffnRegistrationNum = Request.Form["CrffnRegistrationNum"].ToString();
                        logisticsserviceprovider.AnyOtherInfo = Request.Form["LogisticsAnyOtherRelevantInfo"].ToString();
                        logisticsserviceprovider.OtherLicense = Request.Form["OtherLicense"].ToString();
                        logisticsserviceprovider.CustomLicenseExpiryDate = Convert.ToDateTime(Request.Form["CustomLicenseExpiryDate"]);
                        logisticsserviceprovider.CrffnRegistratonExpiryDate = Convert.ToDateTime(Request.Form["CrffnRegistratonExpiryDate"]);
                        logisticsserviceprovider.OtherLicenseExpiryDate = Convert.ToDateTime(Request.Form["OtherLicenseExpiryDate"]);

                        if (logisticsserviceprovider == null)
                        {
                            _context.Add(logisticsserviceprovider);
                        }
                    }

                    else if (agencyId == 3)//Port/Off Dock Terminals Operators
                    {
                        //portoffdockserviceprovider.ApplicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId;
                        //portoffdockserviceprovider.LineOfBusiness = Request.Form["portoffdockLineOfBusiness"].ToString();
                        //portoffdockserviceprovider.StatusOfTerminal = Request.Form["StatusOfTerminal"].ToString();
                        //portoffdockserviceprovider.CargoType = Request.Form["portoffdockCargoType"].ToString();
                        //portoffdockserviceprovider.AnyOtherInfo = Request.Form["portoffdockAnyOtherInfo"].ToString();



                        if (MyTerminals.Count > 0)
                        {


                            foreach (var item in MyTerminals)
                            {
                                var checkterminalexist = (from t in _context.PortOffDockTerminalOperator where t.ApplicationId == model.ApplicationId && t.LocationOfTerminal == item.TerminalLocation select t).ToList();

                                if (checkterminalexist.Count == 0)
                                {

                                    PortOffDockTerminalOperator terminals = new PortOffDockTerminalOperator()
                                    {
                                        ApplicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId,
                                        NameOfTerminal = item.TerminalName,
                                        LocationOfTerminal = item.TerminalLocation,
                                        LineOfBusiness = Request.Form["portoffdockLineOfBusiness"].ToString(),
                                        StatusOfTerminal = Request.Form["StatusOfTerminal"].ToString(),
                                        CargoType = Request.Form["portoffdockCargoType"].ToString(),
                                        AnyOtherInfo = Request.Form["portoffdockAnyOtherInfo"].ToString()
                                    };
                                    _context.PortOffDockTerminalOperator.Add(terminals);
                                }

                                else if (checkterminalexist.Count > 0 && MyTerminals.Count == checkterminalexist.Count)
                                {

                                    checkterminalexist.FirstOrDefault().NameOfTerminal = item.TerminalName;
                                    checkterminalexist.FirstOrDefault().LocationOfTerminal = item.TerminalLocation;
                                }


                            }
                        }



                        if (portoffdockserviceprovider == null)
                        {
                            _context.Add(portoffdockserviceprovider);
                        }
                    }

                    else if (agencyId == 4)//Shipping Agencies/Companies/Lines
                    {
                        shippingagency.ApplicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId;
                        shippingagency.LineOfBusiness = Request.Form["ShippingagencyLineOfBusiness"].ToString();
                        shippingagency.VesselLinesRepresentedInNigeria = Request.Form["VesselLinesRepresentedInNigeria"].ToString();
                        shippingagency.CargoType = Request.Form["ShippingagencyCargoType"].ToString();
                        shippingagency.AnyOtherInfo = Request.Form["ShippingagencyAnyOtherInfo"].ToString();
                    }

                    else if (agencyId == 5) //Other Port Service Providers And Users
                    {
                        otherportserviceprovider.ApplicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId;
                        otherportserviceprovider.LineOfBusiness = Request.Form["otherportLineOfBusiness"].ToString();
                        otherportserviceprovider.AnyOtherInfo = Request.Form["otherportAnyOtherInfo"].ToString();

                        if (otherportserviceprovider == null)
                        {
                            _context.Add(otherportserviceprovider);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                message = "Unable to save record " + ex.Message;
                status = "failed";
            }


            _context.SaveChanges();

            message = "Your record was saved successfully";
            status = "success";

            ResponseWrapper responseWrapper = workflowHelper.processAction(generatedapplicationid, "Proceed", _helpersController.getSessionEmail(), "Initiated Application");
            if (responseWrapper.status == true)
            {
                return Json(new { Status = "success", applicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId, Message = responseWrapper.value });

            }

            return Json(new { Status = status, applicationId = checkappexist == null ? generatedapplicationid : model.ApplicationId, Message = message });
        }









        [HttpGet]
        public ActionResult MyApplications()
        {
            var appdetails = new List<ApplicationRequestForm>();

            try
            {
                var wksbackapp = (from p in _context.ApplicationRequestForm
                                  where p.CompanyEmail == _helpersController.getSessionEmail()
                                  select new
                                  {
                                      p.ApplicationId,
                                      p.CompanyEmail,
                                      p.AgencyName,
                                      p.AddedDate,
                                      p.Status,
                                      p.CurrentStageId
                                  }).ToList();
                foreach (var item in wksbackapp)
                {
                    appdetails.Add(new ApplicationRequestForm()
                    {
                        ApplicationId = item.ApplicationId,
                        CompanyEmail = item.CompanyEmail,
                        AgencyName = item.AgencyName,
                        AddedDate = item.AddedDate,
                        Status = item.Status,
                        CurrentStageId = item.CurrentStageId
                    });
                }


            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ResponseMessage = "Error Occured Getting  Application List, Please Try Again Later";
            }
            return View(appdetails);
        }






        [HttpPost]
        public JsonResult DeleteApplication(string AppID)
        {
            string message = "";
            try
            {
                var deleteapp = (from a in _context.ApplicationRequestForm where a.ApplicationId == AppID select a).FirstOrDefault();


                if (deleteapp != null)
                {

                    _context.ApplicationRequestForm.Remove(deleteapp);

                }

                message = "success";
                _context.SaveChanges();
            }
            catch (Exception ex) { message = ex.Message; }

            return Json(message);
        }






        [HttpGet]
        public ActionResult ChargeSummary(string ApplicationId)
        {

            ViewBag.Applicationid = ApplicationId;

            return View();
        }


        [HttpPost]
        public JsonResult ChargeSummary(string ApplicationId, string PaymentName, decimal paymentamount)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {

                PaymentLog pay = new PaymentLog();
                pay.Rrreference = "09034554333";
                pay.ApplicationId = ApplicationId;
                pay.TransactionDate = DateTime.UtcNow;
                pay.LastRetryDate = DateTime.UtcNow;
                pay.PaymentCategory = PaymentName;
                pay.TxnAmount = paymentamount;
                pay.ApplicantId = _helpersController.getSessionEmail();
                _context.PaymentLog.Add(pay);
                _context.SaveChanges();

                status = "success";
                message = "Successful";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Something went wrong " + ex.Message;
            }
            ResponseWrapper responseWrapper = workflowHelper.processAction(ApplicationId, "GenerateRRR", _helpersController.getSessionEmail(), "Remita Retrieval Reference Generated");
            if(responseWrapper.status == true)
            {
                return Json(new { Status = "success", Message = responseWrapper.value });

            }


            return Json(new { Status = status, Message = message });
        }




        public ActionResult DocumentUpload(string ApplicationId)
        {
            List<DocUpload> DocList = new List<DocUpload>();
            var agencyid = (from a in _context.ApplicationRequestForm where a.ApplicationId == ApplicationId select a.AgencyId).FirstOrDefault();
            var doclist = (from d in _context.Documents where d.AgencyId == agencyid select d).ToList();
            ViewBag.MyApplicationID = ApplicationId;
            if (doclist != null)
            {
                foreach (var item in doclist)
                {
                    DocList.Add(new DocUpload()
                    {

                        DocumentName = item.DocumentName,
                        DocId = item.DocId,
                        IsMandatory = item.IsMandatory

                    });
                }
            }

            return View(DocList);
        }


        [HttpPost]
        public async Task<IActionResult> DocumentUpload(IList<IFormFile> MyFile)
        {
            var appid = Request.Form["txtApplicationId"].ToString().Split(',');
            var Docname = Request.Form["txtDocumentName"];
            var filename = Request.Form["txtDocumentsource"];

          
            string status = string.Empty;
            string message = string.Empty;

            try
            {
                if (MyFile != null)
                {

                    var myfilename = Request.Form["txtDocumentsource"].ToString().Split(',');
                    var mydocname = Request.Form["txtDocumentName"].ToString().Split(',');

                    var checkapprejected = (from a in _context.ApplicationRequestForm where a.ApplicationId == appid[0] select a.Status).FirstOrDefault();


                    if (checkapprejected == "Rejected")
                    {
                        var deleteexistingdoc = (from u in _context.UploadedDocuments where u.ApplicationId == appid[0] select u).ToList();

                        if (deleteexistingdoc.Count() >0 )
                        {
                            foreach (var deleteitems in deleteexistingdoc)
                            {
                                _context.UploadedDocuments.Remove(deleteitems);
                            }
                            await _context.SaveChangesAsync();
                        }
                    }


                    foreach (var filedocname in myfilename)
                    {

                        foreach (var docmentname in mydocname)
                        {
                            var ceck = (from u in _context.UploadedDocuments select new { u.DocumentSource, u.DocumentName }).ToList();

                            var listdocname = ceck.Select(c => c.DocumentName).ToList();
                            var listdocsrc = ceck.Select(c => c.DocumentSource).ToList();

                            

                            if ((!(listdocsrc.Contains(Path.GetFileName(appid[0] + "_" + filedocname)))) && (!(listdocname.Contains(docmentname + "_" + appid[0]))))
                            {

                                UploadedDocuments doc = new UploadedDocuments()
                                {
                                    ApplicationId = appid[0],
                                    DocumentSource = Path.GetFileName(appid[0] + "_" + filedocname), //Path.Combine(_hostingEnv.WebRootPath, "images", appid[0] + "_" + filedocname),
                                DocumentName = docmentname+ "_"+appid[0]//item.FileName;
                                };
                                _context.UploadedDocuments.Add(doc);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    
                    foreach (var item in MyFile)
                    {
                        var fileName = Path.GetFileName(appid[0] + "_" + item.FileName);
                        var filePath = Path.Combine(_hostingEnv.WebRootPath, "UploadedFiles", fileName);
                        using (var fileSteam = new FileStream(filePath, FileMode.Create))
                        {
                            await item.CopyToAsync(fileSteam);
                        }
                    }

                    status = "success";
                    message = "Your documents was successfully uploaded";

                }
                else
                {
                    status = "empty";
                    message = "List of documents to be uploaded is empty";
                }
            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to submit your document. Please try again later " + ex.Message;
            }
            ResponseWrapper responseWrapper = workflowHelper.processAction(appid[0], "Submit", _helpersController.getSessionEmail(), "Application was successfully sumbited after document upload");

            return RedirectToAction("MyApplications");
        }








        public JsonResult ByPassPayment(string ApplicationId, string PaymentName, decimal paymentamount)
        {

            decimal processFeeAmt = 0, statutoryFeeAmt = 0, Arrears = 0;

            try
            {
                log.Info("ApplicationID =>" + ApplicationId);
                ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();

                PaymentLog paymentLog = new PaymentLog();
                paymentLog.ApplicationId = appRequest.ApplicationId;
                paymentLog.TransactionDate = DateTime.UtcNow;
                paymentLog.LastRetryDate = DateTime.UtcNow;
                paymentLog.PaymentCategory = PaymentName;
                paymentLog.TransactionId = "TXNID";
                paymentLog.ApplicantId = _helpersController.getSessionEmail();
                paymentLog.Rrreference = "RRR";
                paymentLog.AppReceiptId = "APPID";
                paymentLog.TxnAmount = paymentamount;
                // paymentLog.TxnAmount = processFeeAmt + statutoryFeeAmt + (Arrears + lateRenewalAmt + NonRenewalAmt);
                paymentLog.Arrears = Arrears;

                paymentLog.Account = _context.Configuration.Where(c => c.ParamId == "AccountNumber").FirstOrDefault().ParamValue;
                paymentLog.BankCode = _context.Configuration.Where(c => c.ParamId == "BankCode").FirstOrDefault().ParamValue;
                paymentLog.RetryCount = 0;
                paymentLog.Status = "AUTH";

                log.Info("About to Add Payment Log");
                _context.PaymentLog.Add(paymentLog);

                log.Info("Added Payment Log to Table");
                _context.SaveChanges();
                log.Info("Saved it Successfully");





                //ResponseWrapper responseWrapper = workflowHelper.processAction(dbCtxt, ApplicationId, "Proceed", userMaster.UserId, "Document Submitted", appRequest.CurrentOfficeLocation, "");

                ResponseWrapper responseWrapper = workflowHelper.processAction(ApplicationId, "GenerateRRR", _helpersController.getSessionEmail(), "Remita Retrieval Reference Generated");
                if (responseWrapper.status == true)
                {
                    return Json(new { Status = "success", Message = responseWrapper.value });

                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace, ex);
            }

            return Json(new
            {
                Status = "success",
                Message = "Payment was successfully bypassed",
            });

        }













        public ActionResult GetAllAgencies()
        {
            var Agencydetails = (from a in _context.Agency select new { a.AgencyId, a.AgencyName }).ToList();
            return Json(Agencydetails);
        }



        public ActionResult AllAgencyFees()
        {
            var Feedetails = (from a in _context.PaymentCategory select new { PaymentAmount = Convert.ToDecimal(a.PaymentAmount).ToString("N"), a.PaymentCategoryName }).ToList();
            return Json(Feedetails);
        }


    }
}
