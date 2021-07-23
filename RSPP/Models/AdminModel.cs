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



    public class StaffDeskModel
    {
        public List<StaffDesk> StaffDeskList { get; set; }
    }

    public class StaffDesk
    {
        public string StaffEmail { get; set; }
        public string StaffName { get; set; }
        public string BranchName { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
        public int OnDesk { get; set; }
        public string status { get; set; }
    }


    public class StaffJson
    {
        public string userid { get; set; }
        public string name { get; set; }
    }

    public class Staff
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userId { get; set; }
        public string email { get; set; }
    }



    public class FacilityInfo
    {
        public string CompanyEmail { get; set; }
        public string AgencyName { get; set; }
        public string ApplicationID { get; set; }
        public string FacilityName { get; set; }
        public string CompanyAddress { get; set; }
        public string ApplicationTypeId { get; set; }
        public string AppliedDate { get; set; }
        public string AplicationCodeType { get; set; }
        public int CurrentStageId { get; set; }
        public string LastAssignedUser { get; set; }
        public string GPS { get; set; }

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
