using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.Utils;

public static class ContentPostTypeHelper
{
    public static string ToPersianTitle(ContentPostType type) => type switch
    {
        ContentPostType.News => "اخبار",
        ContentPostType.Articles => "مقالات",
        ContentPostType.Documents => "اسناد",
        _ => type.ToString(),
    };

    public static bool IsDefined(ContentPostType type)
        => Enum.IsDefined(typeof(ContentPostType), type);
}
