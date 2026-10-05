namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class LabAgreementCommand
    {
        public long? Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpDate { get; set; }
        public string ContractNumber { get; set; }
        public int ReceiverAgreementLabCodeNew { get; set; }
        public long ReceiverAgreementLabId { get; set; }
        public int PrimaryAgreementLabCodeNew { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int LaboratoryAgreementState { get; set; }
        public string? PrimaryAgreementSign { get; set; }
        public DateTime? PrimaryAgreementSignDateTime { get; set; }
        public string? ReceiverAgreementSign { get; set; }
        public DateTime? ReceiverAgreementSignDateTime { get; set; }
        public string? ReceiverAgreementUsername { get; set; }
        public string? PrimaryReturnCause { get; set; }
        public string? ReceiverReturnCause { get; set; }
        public long? ParentId { get; set; }
        public bool? IsAddendum { get; set; }
        public bool? GetSampling { get; set; }
        public bool? GetRecept { get; set; }
        
        public int? PrimaryAction { get; set; }
        public int? ReceiverAction { get; set; }
        public string? PrimaryActionUserName { get; set; }
        public string? ReceiverActionUserName { get; set; }
        public DateTime? PrimaryActionDateTime { get; set; }
        public DateTime? ReceiverActionDateTime { get; set; }

        public int ChildrenCount { get; set; }
        public int AttachmentCount { get; set; }
        public int TestPriceCount { get; set; }
        public int AllTestPriceCount => (RefrenceTestIds ?? Array.Empty<long>())
            .Concat(ChildrenTestIds ?? Array.Empty<long>())
            .Distinct()
            .Count();

        public IReadOnlyList<long>? RefrenceTestIds { get; set; }
        public IReadOnlyList<long>? ChildrenTestIds { get; set; }

        public IReadOnlyList<LabAgreementCommand>? Children { get; set; }
        public IReadOnlyList<LabAgreementAttachmentCommand>? Attachments { get; set; }
        public IReadOnlyList<LabAgreementTestPriceCommand>? TestPrices { get; set; }
        public IReadOnlyList<LabAgreementTestPriceCommand>? MergedTestPrices { get; set; }
    }

    public class LabAgreementAttachmentCommand
    {
        public long? Id { get; set; }
        public string? Remark { get; set; }
        public string FileName { get; set; }
        public string? ContentType { get; set; }
    }
}

