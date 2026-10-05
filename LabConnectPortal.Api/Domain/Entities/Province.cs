using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class Province
{
    [Key]
    public int ProvinceId { get; set; }

    [MaxLength(100)]
    public string ProvinceName { get; set; } = string.Empty;
}
