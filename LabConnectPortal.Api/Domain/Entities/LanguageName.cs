using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class LanguageName
{
    [Key]
    public int LanguageNameId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
