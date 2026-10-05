namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class ReceptionViewModel
    {
        public int SourceLabId { get; set; }
        public string SourceReceptId { get; set; }
        public int TargetLabId { get; set; }
        public string? TargetReceptId { get; set; }
        public string? SourceSendReceptDate { get; set; }
        public byte Age { get; set; }
        public string? AgeType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool Gender { get; set; }
        public string? PreviousRecords { get; set; }
        public bool SendFromSite { get; set; }
        public bool IsUrgent { get; set; }
        public string? NIC { get; set; }
        public string? Mobile { get; set; }
        public string? DoctorCode { get; set; }

        public List<ReceptTestViewModel> ReceptTests { get; set; }
    }
}

