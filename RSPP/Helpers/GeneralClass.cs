using log4net;
using Microsoft.AspNetCore.Mvc;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RSPP.Helpers
{
    public class GeneralClass : Controller
    {
        private ILog log = log4net.LogManager.GetLogger(typeof(GeneralClass));

        private Object thislock = new Object();
        public static string Approved = "Approved";
        public static string Rejected = "Rejected";
        public static string PaymentPending = "Payment Pending";
        public static string PaymentCompleted = "Payment Completed";
        public static string Processing = "Processing";
        public static string ProposalSubmitted = "Proposal Submitted";
        public static string ResultSubmitted = "Result Submitted";
        public static int ProposalAmount = 250000;
        public static string ApplicaionRequired = "Application Required";
        public static string Resubmitted = "Application Required";
        public static string DocumentsRequired = "Documents Required";
        public static string DocumentsUploaded = "Documents Uploaded";
        public static string BHPCode = "345";

        public static int elpsStateID = 0;

        public static int _Year = DateTime.Now.Year + 1;

        public static string _WAITING = "WAITING";
        public static string _STARTED = "STARTED";
        public static string _FINISHED = "FINISHED";

        public static string START = "START";
        public static string NEXT = "NEXT";
        public static string END = "END";
        public static string PASS = "PASS";
        public static string DONE = "DONE";
        public static string BEGIN = "BEGIN";

        public static string CUSTOMER = "CUSTOMER";
        public static string AD_UMR = "AD RM";
        public static string HEAD_UMR = "HEAD UMR";
        public static string SECTION_HEAD = "MANAGER RS";
        public static string TEAM = "TEAM";
        public static string HOOD = "HOOD";
        public static string COMPANY = "COMPANY";
        public static string SUPER_ADMIN = "SUPER ADMIN";
        public static string ADMIN = "ADMIN";
        public static string ICT_ADMIN = "ICT ADMIN";
        public static string DIRECTOR = "DIRECTOR";
        public static string SUPPORT = "SUPPORT";


        // Sending eamil parameters
        public static string PROPOSAL_REQUEST = "PROPOSAL REQUEST";
        public static string STAFF_NOTIFY = "STAFF NOTIFY";
        public static string COMPANY_NOTIFY = "COMPANY NOTIFY";


        private Object lockThis = new object();


        public string Encrypt(string clearText)
        {
            try
            {
                byte[] b = ASCIIEncoding.ASCII.GetBytes(clearText);
                string crypt = Convert.ToBase64String(b);
                byte[] c = ASCIIEncoding.ASCII.GetBytes(crypt);
                string encrypt = Convert.ToBase64String(c);

                return encrypt;
            }
            catch (Exception ex)
            {
                return "Error";
                throw ex;
            }
        }



        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] b;
                byte[] c;
                string decrypt;
                b = Convert.FromBase64String(cipherText);
                string crypt = ASCIIEncoding.ASCII.GetString(b);
                c = Convert.FromBase64String(crypt);
                decrypt = ASCIIEncoding.ASCII.GetString(c);

                return decrypt;
            }
            catch (Exception ex)
            {
                return "Error";
                throw ex;
            }
        }


        /* Decrypting all ID
        *
        * ids => encrypted ids
        */
        public int DecryptIDs(string ids)
        {
            int id = 0;
            var ID = this.Decrypt(ids);

            if (ID == "Error")
            {
                id = 0;
            }
            else
            {
                id = Convert.ToInt32(ID);
            }

            return id;
        }



        public string GenerateApplicationNo()
        {
            lock (thislock)
            {
                Thread.Sleep(1000);
                return DateTime.Now.ToString("MMddyyHHmmss");

            }

        }



        public string StaffMessageTemplate(string subject, string content)
        {
            string body = "<div>";
            body += "<div style='width: 700px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='http://www.shipperstradedata.gov.ng/ServiceProvider/images/DesktopLogoImage.bmp' alt='Logo'/></div>";
            body += "<div class='text-left' style='background-color: #ece8d4; width: 700px; min-height: 200px;'>";
            body += "<div style='padding: 10px 30px 30px 30px;'>";
            body += "<h5 style='text-align: center; font-weight: 300; padding-bottom: 10px; border-bottom: 1px solid #ddd;'>" + subject + "</h5>";
            body += "<p>Dear Sir/Madam,</p>";
            body += "<p style='line-height: 30px; text-align: justify;'>" + content + "</p>";
            body += "<br>";
            body += "<p>Kindly go to <a href='https://shipperstradedata.gov.ng/'>RSPP PORTAL(CLICK HERE)</a></p>";
            body += "<p>Nigerian Shipper's Council<br/> <small>(RSPP) </small></p> </div>";
            body += "<div style='padding:10px 0 10px; 10px; background-color:#888; color:#f9f9f9; width:700px;'> &copy; " + DateTime.Now.Year + " Nigerian Shipper's Council &minus; NSC Nigeria</div></div></div>";
            return body;
        }


        public string SendStaffEmailMessage(string staffemail, string subject, string content)
        {
            var result = "";

            var apiKey = "BA3MXsvMnfddm6gHyYfbwjuSN/ElLLmZGpyU75ykd4JN";
            var username = "AKIAQCM2OPFBVF6TMNVI";
            var emailFrom = "rspp-noreply@shipperstradedata.gov.ng";
            var Host = "email-smtp.us-west-2.amazonaws.com";
            var Port = 587;



            var msgBody = StaffMessageTemplate(subject, content);

            MailMessage _mail = new MailMessage();
            SmtpClient client = new SmtpClient(Host, Port);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(username, apiKey);
            _mail.From = new MailAddress(emailFrom);
            _mail.To.Add(new MailAddress(staffemail));
            _mail.Subject = subject;
            _mail.IsBodyHtml = true;
            _mail.Body = msgBody;
            try
            {
                client.Send(_mail);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }



        public string GetNextProcessingStaff(RSPPdbContext dbCtxt, ApplicationRequestForm appRequest, string targetRole, string locationId, string action, string ActionRole)
        {
            int UserActivityCount = 0;
            string assignedUser = null;
            Dictionary<string, int> UserActivityCountTbl = new Dictionary<string, int>();

            try
            {

                int AllocInterval = Convert.ToInt32(dbCtxt.Configuration.Where(c => c.ParamId == "APP_ALLOC_INTERVAL").FirstOrDefault().ParamValue);
                var todayDate = DateTime.Today.Date;
                var intervalDate = todayDate.AddDays(-AllocInterval);

                        foreach (var userlist in (from item in dbCtxt.UserMaster
                                                  where item.UserRole == targetRole && item.Status == "ACTIVE"
                                                  select item).OrderBy(x => Guid.NewGuid()).Take(100).ToList())
                        {

                            UserActivityCountTbl.Add(userlist.UserEmail, UserActivityCount);
                        }
                        if (UserActivityCountTbl.Keys.Count != 0)
                        {
                            string returnuser = UserActivityCountTbl.OrderBy(a => a.Value).ToList().First().Key;
                           

                            assignedUser = returnuser;
                            dbCtxt.SaveChanges();
                        }
                    


            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return assignedUser;
        }








    }

}
