namespace LabConnectPortal.Api.Infrastructure.ViewModels.ProductCategoryPrice;

public class ProductCategoryPriceItemDto
{
    public int ProductCategoryId { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public int? ProductCategoryGroupId { get; set; }
    public string GroupTitle { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SaveProductCategoryPriceItemCommand
{
    public int ProductCategoryId { get; set; }
    public decimal Price { get; set; }
}

public class SaveProductCategoryPricesCommand
{
    public List<SaveProductCategoryPriceItemCommand> Items { get; set; } = [];
}
