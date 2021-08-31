﻿using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSPP.Configurations;
using RSPP.Helper;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Helpers
{
    public class BackgroundCheck : Controller
    {
        GeneralClass generalClass = new GeneralClass();
        private ILog _logger = log4net.LogManager.GetLogger(typeof(BackgroundCheck));
        public RSPPdbContext _context;
        UtilityHelper _utilityHelper;
        WorkFlowHelper _workflowHelper;
        public BackgroundCheck(RSPPdbContext context)
        {
            _context = context;
            _utilityHelper = new UtilityHelper(_context);
            _workflowHelper = new WorkFlowHelper(_context);

        }

        public async void CheckPayment()
        {

            try
            {
                
                    List<PaymentLog> paymentlog = (from p in _context.PaymentLog where p.Status == "INIT" && p.Rrreference != null && p.Rrreference != "RRR" select p).ToList();
                    if (paymentlog.Count > 0)
                    {
                        foreach (var item1 in paymentlog)
                        {

                            string APIHash = item1.Rrreference + generalClass.AppKey + generalClass.merchantId;
                            string AppkeyHashed = generalClass.GenerateSHA512(APIHash);

                            WebResponse webResponse = _utilityHelper.GetRemitaPaymentDetails(AppkeyHashed, item1.Rrreference);

                            GetPaymentResponse paymentResponse = (GetPaymentResponse)webResponse.value;

                            var paymentdetails = (from a in _context.PaymentLog join p in _context.ApplicationRequestForm on a.ApplicationId equals p.ApplicationId where a.ApplicationId == item1.ApplicationId select new {a, p }).FirstOrDefault();

                            if (paymentResponse != null && paymentdetails != null)
                            {
                                if (paymentdetails.p.Status != "Rejected" && paymentdetails.p.CurrentStageId < 4)
                                {
                                    if (paymentResponse.message == "Successful" || paymentResponse.status == "00")
                                    {
                                        paymentdetails.a.Status = "AUTH";
                                        paymentdetails.a.TxnMessage = paymentResponse.message;
                                        paymentdetails.a.TransactionId = paymentResponse.status;
                                        paymentdetails.a.TransactionDate = Convert.ToDateTime(paymentResponse.transactiontime);

                                        ResponseWrapper responseWrapper = _workflowHelper.processAction(item1.ApplicationId, "GenerateRRR", item1.ApplicantId, "Remita Retrieval Reference Generated");

                                    }
                                    else
                                    {
                                        paymentdetails.a.Status = "INIT";
                                    }
                                }
                                await Task.Delay(_context.SaveChanges());
                            }

                        }

                    }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }

        }

    }
}
