﻿using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using RSPP.Configurations;
using RSPP.Controllers;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RSPP.Helpers
{
    public class GeneralClass : Controller
    {
        private ILog log = log4net.LogManager.GetLogger(typeof(GeneralClass));
        private Object thislock = new Object();
        public string merchantId = "2547916";
        public string AppKey = "1946";
        public string ServiceId = "4430731";
        public string AccountNumber = "0230188961016";
        public string BankCode = "000";

        public string PostPaymentUrl = "https://remitademo.net/remita/exapp/api/v1/send/api/echannelsvc/merchant/api/paymentinit";
        public string GetPaymentBaseUrl = "https://remitademo.net/remita/exapp/api/v1/send/api/echannelsvc/";
        public string PortalBaseUrl = "http://rprspu-demo.azurewebsites.net";

        public decimal GovAgency = 25000;
        public decimal CargoConsolidators = 10000;
        public decimal Chandling = 5000;
        public decimal FreightForwarders = 5000;
        public decimal ClearingAgent = 5000;
        public decimal RoadHaulage = 5000;
        public decimal Stevedoring = 5000;
        public decimal Warehousing = 5000;
        public decimal SeaPortTerminal = 20000; 
        public decimal ICD = 25000;
        public decimal OffDockTerminal = 10000;
        public decimal DryPort = 25000;
        public decimal ShippingAgency = 10000;
        public decimal ShippingCompanyLine = 50000;
        public decimal Users = 5000;
        public decimal OtherPortServiceProviders = 5000;
        
        private Object lockThis = new object();





        public string GenerateSHA512(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }




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




        





        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        public Byte[] GenerateQR(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var imageResult = BitmapToBytes(qrCodeImage);
            return imageResult;
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
            var apiKey = "BNW5He3DoWQAJVMkeMlEzPTtbYIXNveS4t+GuGtXzxQJ";
            var username = "AKIAQCM2OPFBW35OSTFV";
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






        public string NumberToWords(long number)
        {
            if (number == 0)
                return "zero";
            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";
            if ((number / 1000000000) > 0)
            {
                words += NumberToWords(number / 1000000000) + " billion ";
                number %= 1000000000;
            }
            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }




        public string GenerateCertificateNumber(RSPPdbContext dbCtxt)
        {
            String LicenseNum = null;

            LicenseNum = "NSC/RPRSPU/";

            CertificateSerialNumber seriallist = (from c in dbCtxt.CertificateSerialNumber select c).FirstOrDefault();
            long result = Convert.ToInt64(seriallist.SerialNumber);
            seriallist.SerialNumber = result + 1;
            dbCtxt.SaveChanges();
            return LicenseNum + zeroPadder(Convert.ToString(result), 3) + "/" + DateTime.Now.Year;

        }

        private String zeroPadder(String text, int maxlenght)
        {
            String retText = text;

            for (int j = retText.Length; j < maxlenght; j++)
            {
                retText = "0" + retText;
            }
            return retText;
        }

    }

}
