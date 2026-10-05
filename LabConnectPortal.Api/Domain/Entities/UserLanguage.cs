using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class UserLanguage
{
    [Key]
    public Guid UserLanguageId { get; set; }

    public Guid UserId { get; set; }

    public int LanguageNameId { get; set; }

    public LanguageProficiencyLevel? ProficiencyLevel { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual LanguageName LanguageName { get; set; } = null!;
}
