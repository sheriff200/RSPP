using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RSPP.Helpers
{
    public class UtilityHelper : Controller
    {

        public RSPPdbContext _context;
        GeneralClass generalClass = new GeneralClass();

        PaymentTransactionModel paymentRequest = new PaymentTransactionModel();
        private static ILog log = log4net.LogManager.GetLogger(typeof(UtilityHelper));
        public UtilityHelper(RSPPdbContext context)
        {
            _context = context;

        }

        public WebResponse RemitaPayment(PaymentTransactionModel model, string APIHash)
        {
            WebResponse webResponse = new WebResponse();

            var client = new RestClient(generalClass.PostPaymentUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "remitaConsumerKey="+generalClass.merchantId+",remitaConsumerToken="+APIHash);
            var requestobj = JsonConvert.SerializeObject(model); 
            request.AddParameter("application/json", requestobj, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            string content = response.Content;
            var removeunwantedstring1 = content.Replace("jsonp (", "");
            var removeunwantedstring2 = removeunwantedstring1.Replace(")","");


            if (response.IsSuccessful)
            {
                webResponse.message = "Success";
                webResponse.value = JsonConvert.DeserializeObject<RemitaModel>(removeunwantedstring2);
            }
            else
            {
                webResponse.message = "Failed";
            }
            return webResponse;
        }


        public WebResponse GetRemitaPaymentDetails(string APIHash, string rrr)
        {
            WebResponse webResponse = new WebResponse();

            var client = new RestClient(generalClass.GetPaymentBaseUrl+ generalClass.merchantId+"/"+rrr+"/"+ APIHash+"/status.reg");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "remitaConsumerKey=" + generalClass.merchantId + ",remitaConsumerToken=" + APIHash);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                webResponse.message = "Success";
                webResponse.value = JsonConvert.DeserializeObject<GetPaymentResponse>(response.Content);
            }
            else
            {
                webResponse.message = "Failed";
            }
            return webResponse;
        }



        public string GeneratePaymentReference(string Applicationid, string baseurl, string paymentname, decimal paymentamonut)
       {
            try
            {
                var applicationdetails = (from a in _context.ApplicationRequestForm where a.ApplicationId == Applicationid select a).FirstOrDefault();

                var agencydetails = (from a in _context.UserMaster where a.UserEmail == applicationdetails.CompanyEmail select a).FirstOrDefault();

                PaymentLog paymentLogs = _context.PaymentLog.Where(c => c.ApplicationId == Applicationid && (c.Status == "INIT" || c.Status == "AUTH" || c.Status == "FAIL")).FirstOrDefault();
                if (paymentLogs != null)
                {
                    log.Info("RRR is Already Generated =>" + paymentLogs.Rrreference);
                    return paymentLogs.Rrreference;
                }
                

                paymentRequest.serviceTypeId = generalClass.ServiceId;
                paymentRequest.orderId = Applicationid;
                paymentRequest.amount = Decimal.ToInt32(paymentamonut).ToString();
                paymentRequest.payerName = agencydetails.CompanyName;
                paymentRequest.payerEmail = applicationdetails.CompanyEmail;
                paymentRequest.payerPhone = applicationdetails.PhoneNum;
                paymentRequest.description = applicationdetails.AgencyName;
                //paymentRequest.returnSuccessUrl = baseurl + "/Company/PaymentSuccess/" + Applicationid;
                //paymentRequest.returnFailureUrl = baseurl + "/Company/PaymentSuccess/" + Applicationid;
                //paymentRequest.returnBankPaymentUrl = baseurl + "/Company/PaymentSuccess/" + Applicationid;
                //merchantId+serviceTypeId+orderId+totalAmount + apiKey

                string AppkeyHash = generalClass.merchantId + generalClass.ServiceId + Applicationid + Decimal.ToInt32(paymentamonut).ToString() + generalClass.AppKey;
                string AppkeyHashed = generalClass.GenerateSHA512(AppkeyHash);
                WebResponse webResponse = RemitaPayment(paymentRequest, AppkeyHashed);

                RemitaModel paymentResponse = (RemitaModel)webResponse.value;

                PaymentLog paymentLog = new PaymentLog();
                paymentLog.ApplicationId = Applicationid;
                paymentLog.TransactionDate = DateTime.UtcNow;
                paymentLog.LastRetryDate = DateTime.UtcNow;
                paymentLog.PaymentCategory = paymentname;
                paymentLog.TransactionId = paymentResponse.statuscode;
                paymentLog.ApplicantId = applicationdetails.CompanyEmail;
                paymentLog.Rrreference = paymentResponse.RRR;
                paymentLog.AppReceiptId = "APPID";
                paymentLog.TxnAmount = paymentamonut;
                paymentLog.Arrears = 0;
                paymentLog.TxnMessage = paymentResponse.status;
                paymentLog.Account = _context.Configuration.Where(c => c.ParamId == "AccountNumber").FirstOrDefault().ParamValue;
                paymentLog.BankCode = _context.Configuration.Where(c => c.ParamId == "BankCode").FirstOrDefault().ParamValue;
                paymentLog.RetryCount = 0;
                paymentLog.Status = "INIT";

                log.Info("About to Add Payment Log");
                _context.PaymentLog.Add(paymentLog);

                log.Info("Added Payment Log to Table");
                _context.SaveChanges();
                log.Info("Saved it Successfully");
                return paymentResponse.RRR;

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                return "An Error Occured Generating Payment Reference, Pls Try again Later";
            }
           
        }



        public LineOfBusiness Fees(string feename)
        {
            var details = (from a in _context.LineOfBusiness where a.LineOfBusinessName == feename select a).FirstOrDefault();
            //decimal Amount = 0;

            //if (feename == "Government Agency")
            //{
            //    Amount = generalClass.GovAgency;
            //}
            //else if (feename == "Cargo Consolidators")
            //{
            //    Amount = generalClass.CargoConsolidators;
            //}
            //else if (feename == "Chandling")
            //{
            //    Amount = generalClass.Chandling;

            //}
            //else if (feename == "Freight Forwarding")
            //{
            //    Amount = generalClass.FreightForwarders;

            //}
            //else if(feename == "Clearing Agents")
            //{
            //    Amount = generalClass.ClearingAgent;

            //}
            //else if (feename == "Road Haulage")
            //{
            //    Amount = generalClass.RoadHaulage;

            //}
            //else if (feename == "Stevedoring")
            //{
            //    Amount = generalClass.Stevedoring;

            //}
            //else if (feename == "Warehousing")
            //{
            //    Amount = generalClass.Warehousing;

            //}
            //else if (feename == "Port Terminal")
            //{
            //    Amount = generalClass.SeaPortTerminal;

            //}
            //else if (feename == "ICD")
            //{
            //    Amount = generalClass.ICD;

            //}
            //else if (feename == "Off Dock Terminal")
            //{
            //    Amount = generalClass.OffDockTerminal;

            //}
            //else if (feename == "Dry Port")
            //{
            //    Amount = generalClass.DryPort;

            //}
            //else if (feename == "Shipping Agency")
            //{
            //    Amount = generalClass.ShippingAgency;

            //}
            //else if (feename == "Shipping Company/Line")
            //{
            //    Amount = generalClass.ShippingCompanyLine;

            //}
            //else if (feename == "Users")
            //{
            //    Amount = generalClass.Users;

            //}
            //else if (feename == "OtherPortServiceProviders")
            //{
            //    Amount = generalClass.OtherPortServiceProviders;

            //}



            return details;
        }




    }
}
