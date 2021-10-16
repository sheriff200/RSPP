using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class ApplicationRequestForm
    {
        public ApplicationRequestForm()
        {
            ActionHistory = new HashSet<ActionHistory>();
            LogisticsServiceProvider = new HashSet<LogisticsServiceProvider>();
            MissingDocuments = new HashSet<MissingDocuments>();
            OtherPortServiceProvider = new HashSet<OtherPortServiceProvider>();
            PortOffDockTerminalOperator = new HashSet<PortOffDockTerminalOperator>();
            ShippingAgency = new HashSet<ShippingAgency>();
            UploadedDocuments = new HashSet<UploadedDocuments>();
        }

        public string ApplicationId { get; set; }
        public string AgencyName { get; set; }
        public DateTime? DateofEstablishment { get; set; }
        public string CompanyAddress { get; set; }
        public string PostalAddress { get; set; }
        public string PhoneNum { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public int? AgencyId { get; set; }
        public string LicenseReference { get; set; }
        public DateTime? LicenseIssuedDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public int? CurrentStageId { get; set; }
        public string LastAssignedUser { get; set; }
        public DateTime? AddedDate { get; set; }
        public string Status { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ApplicationTypeId { get; set; }
        public string PrintedStatus { get; set; }
        public int? LineOfBusinessId { get; set; }
        public string CacregNum { get; set; }
        public string NameOfAssociation { get; set; }

        public virtual Agency Agency { get; set; }
        public virtual ICollection<ActionHistory> ActionHistory { get; set; }
        public virtual ICollection<LogisticsServiceProvider> LogisticsServiceProvider { get; set; }
        public virtual ICollection<MissingDocuments> MissingDocuments { get; set; }
        public virtual ICollection<OtherPortServiceProvider> OtherPortServiceProvider { get; set; }
        public virtual ICollection<PortOffDockTerminalOperator> PortOffDockTerminalOperator { get; set; }
        public virtual ICollection<ShippingAgency> ShippingAgency { get; set; }
        public virtual ICollection<UploadedDocuments> UploadedDocuments { get; set; }
    }
}
