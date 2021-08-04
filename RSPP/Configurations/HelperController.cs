using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using RSPP.Controllers;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Configurations
{
    public class HelperController : Controller
    {

        public RSPPdbContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        GeneralClass generalClass = new GeneralClass();
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public HelperController(RSPPdbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string getSessionEmail()
        {
            return _httpContextAccessor.HttpContext.Session.GetString(AccountController.sessionEmail);
        }


        public string getSessionRoleName()
        {
            return _httpContextAccessor.HttpContext.Session.GetString(AccountController.sessionRoleName);
        }



        public int getSessionStaffName()
        {
            return Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetString(AccountController.sessionStaffName));
        }


        public string getSessionCompanyName()
        {
            return _httpContextAccessor.HttpContext.Session.GetString(AccountController.sessionCompanyName);
        }







        public List<ApplicationRequestForm> GetApprovalRequest(out String errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequestForm> allRequest = new List<ApplicationRequestForm>();
            Logger.Info("About to fetch Requests due for User => " + getSessionEmail() + " With Roles =>" + getSessionRoleName());

            try
            {
                List<string> UserRoles = getSessionRoleName().Split(',').ToList();
                List<short> AllStages = _context.WorkFlowNavigation.Where(c => UserRoles.Contains(c.ActionRole)).Select(c => c.CurrentStageId).Distinct().ToList();
                Logger.Info("User Stages =>" + string.Join(",", AllStages.ToArray()));
                //ApplicationRequestForm appmaster = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == allRequest.FirstOrDefault().ApplicationId).FirstOrDefault();
                var currentstage = (from u in _context.ApplicationRequestForm where u.LastAssignedUser == getSessionEmail() select new { u.CurrentStageId}).FirstOrDefault();
                
                foreach (ApplicationRequestForm appRequest in _context.ApplicationRequestForm.Where(a => AllStages.Contains((short)a.CurrentStageId)).ToList())
                {
                    
                        if (isPaymentMade(appRequest.ApplicationId, out errorMessage))
                        {
                            foreach (var item in AllStages)
                            {
                                if ((appRequest.CurrentStageId == item) && getSessionEmail() == appRequest.LastAssignedUser)
                                {
                                    if (UserRoles.Contains("OFFICER") || UserRoles.Contains("SUPERVISOR") || UserRoles.Contains("REGISTRAR"))
                                    {
                                        allRequest.Add(appRequest);
                                    }
                                }
                            }
                           
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                errorMessage = "Error Occured When Searching For User Applications, Please try again Later";
            }

            return allRequest;
        }










        public List<ApplicationRequestForm> GetApprovalRequests(UserMaster userMaster, out String errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequestForm> allRequest = new List<ApplicationRequestForm>();
            Logger.Info("About to fetch Requests due for User => " + userMaster.UserEmail + " With Roles =>" + getSessionRoleName());

            try
            {
                List<string> UserRoles = userMaster.UserRole.Split(',').ToList();
                List<short> AllStages = _context.WorkFlowNavigation.Where(c => UserRoles.Contains(c.ActionRole)).Select(c => c.CurrentStageId).Distinct().ToList();
                Logger.Info("User Stages =>" + string.Join(",", AllStages.ToArray()));
                //ApplicationRequestForm appmaster = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == allRequest.FirstOrDefault().ApplicationId).FirstOrDefault();
                var currentstage = (from u in _context.ApplicationRequestForm where u.LastAssignedUser == getSessionEmail() select new { u.CurrentStageId }).FirstOrDefault();

                foreach (ApplicationRequestForm appRequest in _context.ApplicationRequestForm.Where(a => AllStages.Contains((short)a.CurrentStageId)).ToList())
                {

                    if (isPaymentMade(appRequest.ApplicationId, out errorMessage))
                    {
                        foreach (var item in AllStages)
                        {
                            if ((appRequest.CurrentStageId == item) && getSessionEmail() == appRequest.LastAssignedUser)
                            {
                                if (UserRoles.Contains("OFFICER") || UserRoles.Contains("SUPERVISOR") || UserRoles.Contains("REGISTRAR"))
                                {
                                    allRequest.Add(appRequest);
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                errorMessage = "Error Occured When Searching For User Applications, Please try again Later";
            }

            return allRequest;
        }









        public bool isPaymentMade(string ApplicationId, out string errorMessage)
        {
            bool ispaymentmade = false;
            errorMessage = null;
            try
            {

                PaymentLog paymentlog = _context.PaymentLog.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                if (paymentlog !=
                 default(PaymentLog))
                {
                    if (paymentlog.Status == "AUTH")
                    {
                        ispaymentmade = true;
                    }
                }

            }
            catch (Exception ex)
            {
                errorMessage = "Error Occured Validating Payment, Please Try again Later";
                Logger.Error(ex.StackTrace);
            }

            return ispaymentmade;
        }








        public List<ApplicationRequestFormModel> GetApplicationForPastThreeWks()
        {
            var Pastwksdate = DateTime.Now.AddDays(-21);
            var getpastwksapp = new List<ApplicationRequestFormModel>();

            var wksbackapp = (from p in _context.ApplicationRequestForm
                              where p.ModifiedDate <= Pastwksdate && p.LastAssignedUser != p.CompanyEmail && p.LicenseReference == null
                              select new
                              {
                                  p.ApplicationId,
                                  p.CompanyEmail,
                                  p.CompanyAddress,
                                  ModifyDate = p.ModifiedDate.ToString()
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new ApplicationRequestFormModel()
                {
                    ApplicationId = item.ApplicationId,
                    CompanyEmail = item.CompanyEmail,
                    CompanyAddress = item.CompanyAddress,
                    ModifyDate = Convert.ToDateTime(item.ModifyDate).ToLongDateString()
                });
            }
            return getpastwksapp;
        }




        public List<ApplicationRequestFormModel> GetApplicationForPastFiveDays()
        {
            var Pastwksdate = DateTime.Now.AddDays(-5).Date;
            var getpastwksapp = new List<ApplicationRequestFormModel>();
            var wksbackapp = (from p in _context.ApplicationRequestForm
                              where p.ModifiedDate <= Pastwksdate && p.LastAssignedUser != p.CompanyEmail && p.LicenseReference == null
                              select new
                              {
                                  p.ApplicationId,
                                  p.CompanyEmail,
                                  p.CompanyAddress,
                                  p.LastAssignedUser,
                                  ModifyDate = p.ModifiedDate
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new ApplicationRequestFormModel()
                {
                    ApplicationId = item.ApplicationId,
                    CompanyEmail = item.CompanyEmail,
                    CompanyAddress = item.CompanyAddress,
                    LastAssignedUser = item.LastAssignedUser,
                    ModifyDate = Convert.ToDateTime(item.ModifyDate).ToLongDateString()
                });
            }
            return getpastwksapp;
        }








        public List<ApplicationRequestForm> GetApplications(string userId, string type, out string errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequestForm> AllBaseRequest = new List<ApplicationRequestForm>();
            try
            {
                foreach (ApplicationRequestForm b in _context.ApplicationRequestForm.Where(c => (c.CompanyEmail.Trim() == userId.Trim() && c.LicenseReference != null)).ToList())
                {

                    if (type == "ALL")
                    {
                        AllBaseRequest.Add(b);
                    }
                    else if (type == "EXP")
                    {
                        if (b.LicenseExpiryDate !=
                         default(DateTime) && b.LicenseReference !=
                         default(string))
                        {
                            if (b.LicenseExpiryDate.Value.Subtract(DateTime.UtcNow).Days <= 30)
                            {
                                AllBaseRequest.Add(b);
                            }
                        }

                    }
                    else if (type == "PEM")
                    {
                        if (b.LicenseReference !=
                         default(string))
                        {
                            AllBaseRequest.Add(b);
                        }
                    }
                    else if (type == "PROC")
                    {
                        if (b.LicenseReference ==
                         default(string) && b.CurrentStageId !=
                         default(int))
                        {
                            string stateType = _context.WorkFlowState.Where(w => w.StateId == b.CurrentStageId).FirstOrDefault().StateType;
                            if (stateType !=
                             default(string) && stateType == "PROGRESS")
                            {
                                AllBaseRequest.Add(b);
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                errorMessage = "Error Occured when Generating List of Applications, Please Try again Later";
            }

            return AllBaseRequest;
        }






        public List<GeneralCommentModel> AllHistoryComment(string userId)
        {
            List<GeneralCommentModel> AllComment = new List<GeneralCommentModel>();

            try
            {

                var unapprovedapps = (from ah in _context.ActionHistory
                                      join a in _context.ApplicationRequestForm on ah.ApplicationId equals a.ApplicationId
                                      where a.CompanyEmail == userId && a.Status == "Rejected" && ah.TargetedToRole == "COMPANY" && (ah.Action == "Reject" || ah.Action == "Rejectmk") && a.CurrentStageId == 1 && a.LicenseReference == null
                                      select ah).AsEnumerable().OrderByDescending(d => Convert.ToDateTime(d.ActionDate)).GroupBy(d => d.ApplicationId).ToList();

                foreach (var item in unapprovedapps)
                {
                    AllComment.Add(new GeneralCommentModel()
                    {
                        Comment = item.FirstOrDefault().Message,
                        ApplicationID = item.FirstOrDefault().ApplicationId
                    });
                }



            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
            }

            return AllComment;
        }




        public Dictionary<string, int> GetApplicationStatistics(string userId, out string responseMessage, out Dictionary<string, int> applicationStageReference)
        {
            int totalAppCount = 0;
            int ProcAppCount = 0;
            int yetToSubmitCount = 0;
            int totalPermitCount = 0;
            int permitExpiringCount = 0;
            responseMessage = "SUCCESS";
            Dictionary<string,
             int> appStatisticsTable = new Dictionary<string,
             int>();
            List<int> workFlowStagesToWork = new List<int> { 1, 2, 3, 4, 5, 10, 46, 25, 27, 47 };

            applicationStageReference = new Dictionary<string, int>();


            try
            {
                foreach (ApplicationRequestForm b in _context.ApplicationRequestForm.Where(c => (c.CompanyEmail.Trim() == userId.Trim())).ToList())
                {
                    //ALL
                    totalAppCount = totalAppCount + 1;

                    if (workFlowStagesToWork.Contains(b.CurrentStageId.Value))
                    {
                        applicationStageReference.Add(b.ApplicationId, b.CurrentStageId.Value);
                    }

                    //EXP
                    string stateType1 = _context.WorkFlowState.Where(w => w.StateId == b.CurrentStageId).FirstOrDefault().StateType;
                    if (!string.IsNullOrEmpty(stateType1) && stateType1 == "COMPLETE")
                    {

                        if (b.LicenseExpiryDate.HasValue && !string.IsNullOrEmpty(b.LicenseReference))
                        {
                            //b.LicenseExpiryDate.Value.Subtract(DateTime.UtcNow).Days <= 0) ||
                            if ((b.LicenseExpiryDate.Value.Subtract(DateTime.UtcNow).Days <= 30))
                            {
                                permitExpiringCount = permitExpiringCount + 1;
                            }
                        }
                    }


                    //PEM
                    if (b.LicenseReference != default(string))
                    {
                        totalPermitCount = totalPermitCount + 1;
                    }

                    //PROC
                    if (b.LicenseReference ==
                     default(string) && b.CurrentStageId !=
                     default(int))
                    {
                        string stateType = _context.WorkFlowState.Where(w => w.StateId == b.CurrentStageId).FirstOrDefault().StateType;

                        if (stateType !=
                         default(string) && (stateType == "PROGRESS" || stateType == "CONTINUE"))
                        {
                            ProcAppCount = ProcAppCount + 1;
                        }
                    }



                   

                }

                appStatisticsTable.Add("ALL", totalAppCount);
                appStatisticsTable.Add("EXP", permitExpiringCount);
                appStatisticsTable.Add("PEM", totalPermitCount);
                appStatisticsTable.Add("PROC", ProcAppCount);
                appStatisticsTable.Add("YTST", yetToSubmitCount);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
                responseMessage = "Error Occured when Generating Statistics for the DashBoard, Please Try again Later";
            }

            return appStatisticsTable;
        }








        public List<CompanyMessage> GetCompanyMessages(RSPPdbContext dbCtxt, UserMaster userMaster)
        {
            List<CompanyMessage> messageList = new List<CompanyMessage>();
            Logger.Info("About to Get Notification Messages for Company =>" + userMaster.UserEmail);

            try
            {
                var actionhistory = (from a in _context.ActionHistory join app in dbCtxt.ApplicationRequestForm on a.ApplicationId equals app.ApplicationId where a.NextStateId == 1 || a.NextStateId == 2 || a.NextStateId == 3 || a.NextStateId == 4 || a.NextStateId == 10 || a.NextStateId == 21 || a.NextStateId == 46 && app.CompanyEmail == userMaster.UserEmail select new { a,app}).ToList();
                
                if (actionhistory.Count > 0)
                {
                    foreach (var item in actionhistory)
                    {
                        CompanyMessage companyMessage = new CompanyMessage();
                        companyMessage.ApplicationId = item.a.ApplicationId;
                        companyMessage.Date = item.a.ActionDate.ToString();
                        companyMessage.MessageId = Convert.ToString(item.a.ActionId);
                        companyMessage.Message = item.a.Message;
                        if (item.a.NextStateId == 7 || item.a.NextStateId == 105 || item.a.NextStateId == 106)
                        {
                            companyMessage.MessageType = "Error";
                        }
                        else
                        {
                            companyMessage.MessageType = "Info";
                        }


                        messageList.Add(companyMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
            }

            Logger.Info("Message List Count =>" + messageList.Capacity);

            return messageList;
        }



        public List<OutofOffice> GetReliverStaffOutofOffice(string reliever)
        {
            var today = DateTime.Now.Date;
            var getpastwksapp = new List<OutofOffice>();
            var wksbackapp = (from p in _context.OutofOffice
                              join r in _context.ApplicationRequestForm on p.Relieved equals r.LastAssignedUser
                              where p.Reliever == reliever && p.Status == "Started"
                              select new
                              {
                                  p.Reliever,
                                  p.Relieved,
                                  p.StartDate,
                                  p.EndDate,
                                  p.Status,
                                  p.Comment
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new OutofOffice()
                {
                    Reliever = item.Reliever,
                    Relieved = item.Relieved,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Status = item.Status,
                    Comment = item.Comment
                });
            }
            return getpastwksapp;
        }

        public List<OutofOffice> GetAllOutofOffice()
        {
            var today = DateTime.Now.Date;
            var getpastwksapp = new List<OutofOffice>();
            var wksbackapp = (from p in _context.OutofOffice
                              select new
                              {
                                  p.Reliever,
                                  p.Relieved,
                                  p.StartDate,
                                  p.EndDate,
                                  p.Status,
                                  p.Comment
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new OutofOffice()
                {
                    Reliever = item.Reliever,
                    Relieved = item.Relieved,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Status = item.Status,
                    Comment = item.Comment
                });
            }
            return getpastwksapp;

        }



        public List<ApplicationRequestForm> CompanyInfo(string applicationId)
        {
            var today = DateTime.Now.Date;
            var Appinfo = new List<ApplicationRequestForm>();
            var Appinfoagency = (from p in _context.ApplicationRequestForm
                          where p.ApplicationId == applicationId
                          select new
                          {
                              p.CompanyEmail,
                              p.AgencyName,
                              p.CompanyAddress,
                              p.CompanyWebsite,
                              p.DateofEstablishment
                          }).ToList();
            foreach (var item in Appinfoagency)
            {
                Appinfo.Add(new ApplicationRequestForm()
                {
                    CompanyEmail = item.CompanyEmail,
                    AgencyName = item.AgencyName,
                    CompanyAddress = item.CompanyAddress,
                    CompanyWebsite = item.CompanyWebsite,
                    DateofEstablishment = item.DateofEstablishment
                });
            }
            return Appinfo;

        }




        public List<GovernmentAgency> Governmentagency(string applicationId)
        {
            var govagency = new List<GovernmentAgency>();
            var agency = (from p in _context.GovernmentAgency where p.ApplicationId == applicationId
                          select new
                              {
                                  p.ServicesProvidedInPort,
                                  p.AnyOtherRelevantInfo,
                              }).ToList();
            foreach (var item in agency)
            {
                govagency.Add(new GovernmentAgency()
                {
                    ServicesProvidedInPort = item.ServicesProvidedInPort,
                    AnyOtherRelevantInfo = item.AnyOtherRelevantInfo,
                });
            }
            return govagency;

        }


        public List<LogisticsServiceProvider> LogisticsServiceProvider(string applicationId)
        {
            var logisticsagency = new List<LogisticsServiceProvider>();
            var agency = (from p in _context.LogisticsServiceProvider
                          where p.ApplicationId == applicationId
                          select new
                          {
                              p.CrffnRegistrationNum,
                              p.CrffnRegistratonExpiryDate,
                              p.CustomLicenseExpiryDate,
                              p.CustomLicenseNum,
                              p.LineOfBusiness,
                              p.OtherLicense,
                              p.OtherLicenseExpiryDate,
                              p.AnyOtherInfo

                          }).ToList();
            foreach (var item in agency)
            {
                logisticsagency.Add(new LogisticsServiceProvider()
                {
                    CrffnRegistrationNum = item.CrffnRegistrationNum,
                    CrffnRegistratonExpiryDate = item.CrffnRegistratonExpiryDate,
                    CustomLicenseExpiryDate = item.CustomLicenseExpiryDate,
                    CustomLicenseNum = item.CustomLicenseNum,
                    LineOfBusiness = item.LineOfBusiness,
                    OtherLicense = item.OtherLicense,
                    OtherLicenseExpiryDate = item.OtherLicenseExpiryDate,
                    AnyOtherInfo = item.AnyOtherInfo
                });
            }
            return logisticsagency;

        }


        public List<OtherPortServiceProvider> OtherPortServiceProvider(string applicationId)
        {
            var otherportagency = new List<OtherPortServiceProvider>();
            var agency = (from p in _context.OtherPortServiceProvider
                          where p.ApplicationId == applicationId
                          select new
                          {
                              p.LineOfBusiness,
                              p.AnyOtherInfo,
                          }).ToList();
            foreach (var item in agency)
            {
                otherportagency.Add(new OtherPortServiceProvider()
                {
                    LineOfBusiness = item.LineOfBusiness,
                    AnyOtherInfo = item.AnyOtherInfo
                });
            }
            return otherportagency;

        }


        public List<PortOffDockTerminalOperator> PortOffDockTerminalOperator(string applicationId)
        {
            var portofdockagency = new List<PortOffDockTerminalOperator>();
            var agency = (from p in _context.PortOffDockTerminalOperator
                          where p.ApplicationId == applicationId
                          select new
                          {
                              p.CargoType,
                              p.AnyOtherInfo,
                              p.LineOfBusiness,
                              p.LocationOfTerminal,
                              p.NameOfTerminal,
                              p.StatusOfTerminal
                          }).ToList();
            foreach (var item in agency)
            {
                portofdockagency.Add(new PortOffDockTerminalOperator()
                {
                    CargoType = item.CargoType,
                    AnyOtherInfo = item.AnyOtherInfo,
                    LineOfBusiness = item.LineOfBusiness,
                    LocationOfTerminal = item.LocationOfTerminal,
                    NameOfTerminal = item.NameOfTerminal,
                    StatusOfTerminal = item.StatusOfTerminal
                });
            }
            return portofdockagency;
        }





        public List<ShippingAgency> ShippingAgency(string applicationId)
        {
            var shippingagency = new List<ShippingAgency>();
            var agency = (from p in _context.ShippingAgency
                          where p.ApplicationId == applicationId
                          select new
                          {
                              p.CargoType,
                              p.AnyOtherInfo,
                              p.LineOfBusiness,
                              p.VesselLinesRepresentedInNigeria
                          }).ToList();
            foreach (var item in agency)
            {
                shippingagency.Add(new ShippingAgency()
                {
                    CargoType = item.CargoType,
                    AnyOtherInfo = item.AnyOtherInfo,
                    LineOfBusiness = item.LineOfBusiness,
                    VesselLinesRepresentedInNigeria = item.VesselLinesRepresentedInNigeria
                });
            }
            return shippingagency;
        }




        public AppResponse ChangePassword(string useremail, string oldPassword, string newPassword)
        {
            AppResponse appResponse = new AppResponse();
            ChangePassword changePwd = new ChangePassword();

            try
            {
                Logger.Info("About to ChangePassword On Elps with Email User => " + useremail);

                var oldpass = generalClass.Encrypt(oldPassword);
                var newpass = generalClass.Encrypt(newPassword);
                var UserDetails = (from u in _context.UserMaster where u.UserEmail == useremail && u.Password == oldpass select u).FirstOrDefault();

                if(UserDetails != null)
                {
                    UserDetails.Password = newpass;
                    _context.SaveChanges();
                    appResponse.value = true;
                    appResponse.message = "SUCCESS";
                }
                else
                {
                    appResponse.value = false;
                    appResponse.message = "No Match";

                }


            }
            catch (Exception ex)
            {
                Logger.Error("Last Exception =>" + ex.Message);
                appResponse.message = ex.Message;
            }
            finally
            {
                Logger.Info("About to Return with Message => " + appResponse.message);
            }


            return appResponse;
        }


        public List<UploadedDocuments> CompanyDocument(string companyemail)
        {
            AppResponse appResponse = new AppResponse();
            List<UploadedDocuments> UploadedDocuments = new List<UploadedDocuments>();
            try
            {

               var UploadedDocument = (from u in _context.UploadedDocuments join a in _context.ApplicationRequestForm on u.ApplicationId equals a.ApplicationId where a.CompanyEmail == companyemail select u).ToList();
                if (UploadedDocument.Count() > 0)
                {
                    foreach (var doc in UploadedDocument)
                    {
                        UploadedDocuments.Add(new UploadedDocuments()
                        {
                            DocumentSource = doc.DocumentSource,
                            DocumentName = doc.DocumentName.Split("_")[0]
                        });
                    }

                }
                appResponse.message = "success";
                appResponse.value = true;
            }
            catch (Exception ex)
            {
                appResponse.message = ex.Message;
                appResponse.value = false;
            }
            return UploadedDocuments;
        }






        public List<UploadedDocuments> UploadedCompanyDocument(string Appid)
        {
            AppResponse appResponse = new AppResponse();
            List<UploadedDocuments> UploadedDoc = new List<UploadedDocuments>();
            try
            {
              var UploadedDocument = (from u in _context.UploadedDocuments where u.ApplicationId == Appid select u).ToList();
                if (UploadedDocument.Count() > 0)
                {
                    foreach (var doc in UploadedDocument)
                    {
                        //var splitdocname = doc.DocumentName.Split("_");
                        UploadedDoc.Add(new UploadedDocuments()
                        {
                          DocumentSource = doc.DocumentSource,
                           DocumentName = doc.DocumentName.Split("_")[0]
                        });
                         
                    }
                   

                }
                appResponse.message = "success";
                appResponse.value = true;
            }
            catch (Exception ex)
            {
                appResponse.message = ex.Message;
                appResponse.value = false;
            }
            return UploadedDoc;
        }












        public void GenerateCertificate(string applicationId, string staffemail)
        {


            decimal processFees = 0;
            decimal statutoryFees = 0;
            String errorMessage = null;
            DateTime currentDateTime = DateTime.UtcNow.AddYears(1);
            DateTime currentDate = DateTime.UtcNow;
            try
            {
                Logger.Info("About to generate License");
                DateTime expiryDate = currentDateTime.AddYears(2);
                //var Signatureid = (from u in _context.UserMaster where u.UserEmail == staffemail select u.SignatureId).FirstOrDefault();DateTime.ParseExact(expiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture
                ApplicationRequestForm appRequest = _context.ApplicationRequestForm.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                appRequest.LicenseIssuedDate = appRequest.LicenseIssuedDate == null ? DateTime.UtcNow : appRequest.LicenseIssuedDate;
                appRequest.LicenseExpiryDate = appRequest.LicenseExpiryDate == null ? expiryDate : appRequest.LicenseExpiryDate;
                string LicenseRef = generalClass.GenerateCertificateNumber(_context);
                Logger.Info("Generated License Num => " + LicenseRef);
                appRequest.LicenseReference = appRequest.LicenseReference == null ? LicenseRef : appRequest.LicenseReference;
                //appRequest.SignatureId = Convert.ToInt32(Signatureid);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.InnerException);
            }
        }






        public ViewAsPdf ViewCertificate(string id)
        {

            List<PermitModels> permitmodel = new List<PermitModels>();
            List<Permitmodel> permits = new List<Permitmodel>();

            var signature = (from c in _context.Configuration where c.ParamId == "Signature" select c.ParamValue).FirstOrDefault();

            var details = (from a in _context.ApplicationRequestForm
                           join u in _context.UserMaster on a.CompanyEmail equals u.UserEmail
                           join f in _context.PaymentLog on a.ApplicationId equals f.ApplicationId
                           where a.ApplicationId == id
                           select new { f.TxnAmount, a.CompanyEmail, u.CompanyName, a.ApplicationId, a.LicenseReference, a.LicenseExpiryDate, a.LicenseIssuedDate, a.AgencyName }).FirstOrDefault();



            var Host = HttpContext.Request.Path; //HttpContext.Current.Request.Url.Authority;
            var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
            var QrCode = generalClass.GenerateQR(absolutUrl);
            if (details != null)
            {
                var Pstatus = (from p in _context.ApplicationRequestForm where p.ApplicationId == id select p).FirstOrDefault();
                Pstatus.PrintedStatus = "Printed";
                _context.SaveChanges();

                permits.Add(new Permitmodel()
                {
                    Signature = signature,
                    IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                    IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                    IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                    ExpiryDay = Convert.ToDateTime(details.LicenseExpiryDate).ToString("dd"),
                    ExpiryMonth = Convert.ToDateTime(details.LicenseExpiryDate).ToString("MMMM"),
                    ExpiryYear = Convert.ToDateTime(details.LicenseExpiryDate).ToString("yyyy"),
                    CompanyName = details.CompanyName,
                    AgencyName = details.AgencyName,
                    QrCode = QrCode
                });

                permitmodel.Add(new PermitModels()
                {
                    permitmodels = permits.ToList()
                });

            }

            return new ViewAsPdf("ViewCertificate", permitmodel.ToList())
            {
                PageSize = (Rotativa.AspNetCore.Options.Size?)Rotativa.Options.Size.A4,
                FileName = id + ".pdf"

            };
        }





    }
}
