using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Models
{
    public class AdminModel
    {
    }


    public class ResponseWrapper
    {
        public ResponseWrapper() { }
        public bool status { get; set; }
        public string value { get; set; }
        public string nextStageId { get; set; }
        public string nextStateType { get; set; }
        public string receivedBy { get; set; }
        public string receivedByRole { get; set; }
        public string receivedLocation { get; set; }
    }


    public class Terminal
    {
        public string TerminalName { get; set; }
        public string TerminalLocation { get; set; }
    }

    public class ApplicationRatio
    {
        public int totalapplication { get; set; }
        public int CompleteApplication { get; set; }
        public int UnCompleteApplication { get; set; }
    }

    public class PaymentModel
    {

        public string RRReference { get; set; }
        public string Status { get; set; }
        public Nullable<DateTime> TransactionDate { get; set; }
        public string TransactionID { get; set; }
        public Nullable<decimal> TxnAmount { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicationID { get; set; }
        public string ExtraPaymentBy { get; set; }
        public string CompanyUserId { get; set; }
        public decimal Arrears { get; set; }

    }

    public class ApplicationRequestFormModel
    {
        public string ApplicationId { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string LastAssignedUser { get; set; }
        public string ModifyDate { get; set; }
    }



    public class LicenseRatio
    {
        public int totalLicense { get; set; }
        public int Online { get; set; }
        public int Legacy { get; set; }
    }



    public class RatioDash
    {
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int OnDesk { get; set; }
        public int LegacyInProgress { get; set; }
        public int OnlineInProgress { get; set; }
        public int Processed { get; set; }
    }

}
