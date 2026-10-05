using LabConnectPortal.Api.Controllers;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Implementations;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace LabConnectPortal.Tests.Controllers;

public class ProductApiControllerTests
{
    [Fact]
    public async Task GetCategories_ReturnsCategoryAndGroupIdsForProductCommands()
    {
        await using var context = CreateContext();
        var fixture = await SeedAsync(context);
        var service = new ProductCatalogService(
            context,
            null!,
            null!,
            Options.Create(new PortalSettings()),
            Options.Create(new FileStorageSettings()));
        var controller = CreateController(service, fixture.UserId, fixture.CenterProfileId);

        var result = Assert.IsType<OperationResult<List<ProductApiCategoryDto>>>(
            await controller.GetCategories());

        Assert.True(result.Status, result.Message);
        Assert.NotNull(result.Data);
        Assert.Collection(
            result.Data,
            item =>
            {
                Assert.Equal(fixture.FirstCategoryId, item.Id);
                Assert.Equal("دسته اول", item.Title);
                Assert.Equal(fixture.CategoryGroupId, item.ProductCategoryGroupId);
                Assert.Equal("گروه تست", item.ProductCategoryGroupTitle);
            },
            item =>
            {
                Assert.Equal(fixture.SecondCategoryId, item.Id);
                Assert.Equal("دسته دوم", item.Title);
                Assert.Equal(fixture.CategoryGroupId, item.ProductCategoryGroupId);
                Assert.Equal("گروه تست", item.ProductCategoryGroupTitle);
            });
    }

    [Fact]
    public async Task Create_WithBase64FeaturedImage_PersistsImageOnDiskAndPathInDatabase()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"labconnect-product-api-{Guid.NewGuid():N}");
        Directory.CreateDirectory(contentRoot);

        try
        {
            await using var context = CreateContext();
            var fixture = await SeedAsync(context);
            var storageSettings = new FileStorageSettings
            {
                StorageRoot = "uploads",
                MaxFileSizeBytes = 5 * 1024 * 1024,
            };
            var fileStorage = new LocalFileStorageService(
                new TestHostEnvironment(contentRoot),
                Options.Create(storageSettings));
            var service = new ProductCatalogService(
                context,
                fileStorage,
                null!,
                Options.Create(new PortalSettings()),
                Options.Create(storageSettings));
            var controller = CreateController(service, fixture.UserId, fixture.CenterProfileId);
            const string imageBase64 =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y9ZQmcAAAAASUVORK5CYII=";

            var result = Assert.IsType<OperationResult<ProductDto>>(
                await controller.Create(new ProductApiCreateProductCommand
                {
                    Title = "محصول دارای تصویر",
                    BrandId = fixture.FirstBrandId,
                    ProductCategoryGroupId = fixture.CategoryGroupId,
                    Price = 1_000_000,
                    IsNegotiablePrice = false,
                    CategoryIds = [fixture.FirstCategoryId],
                    FeaturedImageBase64 = $"data:image/png;base64,{imageBase64}",
                }));

            Assert.True(result.Status, result.Message);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.HasFeaturedImage);
            Assert.False(string.IsNullOrWhiteSpace(result.Data.FeaturedImagePath));

            var absolutePath = Path.Combine(
                contentRoot,
                result.Data.FeaturedImagePath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(absolutePath));
            Assert.Equal(Convert.FromBase64String(imageBase64), await File.ReadAllBytesAsync(absolutePath));

            context.ChangeTracker.Clear();
            var product = await context.Products.AsNoTracking()
                .SingleAsync(x => x.ProductId == result.Data.ProductId);
            Assert.Equal(result.Data.FeaturedImagePath, product.FeaturedImagePath);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Create_WithInvalidBase64_DoesNotCreateProduct()
    {
        await using var context = CreateContext();
        var fixture = await SeedAsync(context);
        var service = new ProductCatalogService(
            context,
            null!,
            null!,
            Options.Create(new PortalSettings()),
            Options.Create(new FileStorageSettings()));
        var controller = CreateController(service, fixture.UserId, fixture.CenterProfileId);

        var result = Assert.IsType<OperationResult<ProductDto>>(
            await controller.Create(new ProductApiCreateProductCommand
            {
                Title = "محصول با تصویر نامعتبر",
                BrandId = fixture.FirstBrandId,
                ProductCategoryGroupId = fixture.CategoryGroupId,
                Price = 1_000_000,
                IsNegotiablePrice = false,
                CategoryIds = [fixture.FirstCategoryId],
                FeaturedImageBase64 = "data:image/png;base64,not-valid-base64",
            }));

        Assert.False(result.Status);
        Assert.Empty(await context.Products.ToListAsync());
    }

    [Fact]
    public async Task CreateThenUpdate_PersistsProductForApiKeyCenter()
    {
        await using var context = CreateContext();
        var fixture = await SeedAsync(context);
        var service = new ProductCatalogService(
            context,
            null!,
            null!,
            Options.Create(new PortalSettings()),
            Options.Create(new FileStorageSettings()));
        var controller = CreateController(service, fixture.UserId, fixture.CenterProfileId);

        var createResult = Assert.IsType<OperationResult<ProductDto>>(
            await controller.Create(new ProductApiCreateProductCommand
            {
                Title = "  محصول اولیه  ",
                BrandId = fixture.FirstBrandId,
                Warranty = "یک سال",
                Description = "توضیحات اولیه",
                ProductCategoryGroupId = fixture.CategoryGroupId,
                Price = 1_000_000,
                IsNegotiablePrice = false,
                StockQuantity = 3,
                CategoryIds = [fixture.FirstCategoryId],
                AttributeValues =
                [
                    new SaveProductAttributeValueCommand
                    {
                        ProductAttributeId = fixture.FirstAttributeId,
                        Value = "مقدار اولیه",
                    },
                ],
            }));

        Assert.True(createResult.Status, createResult.Message);
        Assert.NotNull(createResult.Data);
        var productId = createResult.Data.ProductId;

        context.ChangeTracker.Clear();
        var created = await context.Products.AsNoTracking()
            .Include(x => x.CategoryAssignments)
            .Include(x => x.AttributeValues)
            .SingleAsync(x => x.ProductId == productId);
        Assert.Equal("محصول اولیه", created.Title);
        Assert.Equal(fixture.UserId, created.CreatedByUserId);
        Assert.Equal(fixture.CenterProfileId, created.CreatedByCenterProfileId);
        Assert.Equal(fixture.FirstBrandId, created.BrandId);
        Assert.Equal(fixture.FirstCategoryId, Assert.Single(created.CategoryAssignments).ProductCategoryId);
        Assert.Equal("مقدار اولیه", Assert.Single(created.AttributeValues).Value);

        var updateResult = Assert.IsType<OperationResult<ProductDto>>(
            await controller.Update(new UpdateProductCommand
            {
                ProductId = productId,
                Title = "محصول ویرایش‌شده",
                BrandId = fixture.SecondBrandId,
                Warranty = "دو سال",
                Description = "توضیحات جدید",
                ProductCategoryGroupId = fixture.CategoryGroupId,
                Price = 2_500_000,
                IsNegotiablePrice = false,
                IsUsed = true,
                StockQuantity = 7,
                DiscountPercent = 10,
                CategoryIds = [fixture.SecondCategoryId],
                AttributeValues =
                [
                    new SaveProductAttributeValueCommand
                    {
                        ProductAttributeId = fixture.SecondAttributeId,
                        Value = "مقدار جدید",
                    },
                ],
            }));

        Assert.True(updateResult.Status, updateResult.Message);
        context.ChangeTracker.Clear();
        var updated = await context.Products.AsNoTracking()
            .Include(x => x.CategoryAssignments)
            .Include(x => x.AttributeValues)
            .SingleAsync(x => x.ProductId == productId);
        Assert.Equal("محصول ویرایش‌شده", updated.Title);
        Assert.Equal(fixture.SecondBrandId, updated.BrandId);
        Assert.Equal("دو سال", updated.Warranty);
        Assert.Equal("توضیحات جدید", updated.Description);
        Assert.Equal(2_500_000, updated.Price);
        Assert.True(updated.IsUsed);
        Assert.Equal(7, updated.StockQuantity);
        Assert.Equal(10, updated.DiscountPercent);
        Assert.Equal(fixture.SecondCategoryId, Assert.Single(updated.CategoryAssignments).ProductCategoryId);
        var updatedAttribute = Assert.Single(updated.AttributeValues);
        Assert.Equal(fixture.SecondAttributeId, updatedAttribute.ProductAttributeId);
        Assert.Equal("مقدار جدید", updatedAttribute.Value);
    }

    private static LabConnectDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LabConnectDbContext>()
            .UseInMemoryDatabase($"product-api-tests-{Guid.NewGuid():N}")
            .Options;
        return new LabConnectDbContext(options);
    }

    private static ProductApiController CreateController(
        ProductCatalogService service,
        Guid userId,
        Guid centerProfileId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items[ApiKeyAuthorizationFilter.ContextItemKey] = new ApiKeyAuthorizationDto
        {
            ApiKeyId = Guid.NewGuid(),
            UserId = userId,
            CenterProfileId = centerProfileId,
        };

        return new ProductApiController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
        };
    }

    private static async Task<ProductFixture> SeedAsync(LabConnectDbContext context)
    {
        var centerProfileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const int groupId = 101;
        const int firstCategoryId = 201;
        const int secondCategoryId = 202;
        const int firstBrandId = 301;
        const int secondBrandId = 302;
        const int firstAttributeId = 401;
        const int secondAttributeId = 402;

        context.CenterProfiles.Add(new CenterProfile
        {
            Id = centerProfileId,
            CenterType = CenterType.Store,
            Name = "مرکز تست",
            IsApproved = true,
            IsApiKeyEnabled = true,
            OwnerUserId = userId,
        });
        context.Users.Add(new User
        {
            Id = userId,
            UserType = UserType.Store,
            CenterProfileId = centerProfileId,
            Username = "product-api-test-user",
            MobileNumber = "09000000000",
            IsActive = true,
        });
        context.ProductCategoryGroups.Add(new ProductCategoryGroup
        {
            ProductCategoryGroupId = groupId,
            Name = "گروه تست",
        });
        context.ProductCategories.AddRange(
            new ProductCategory
            {
                ProductCategoryId = firstCategoryId,
                ProductCategoryGroupId = groupId,
                Title = "دسته اول",
            },
            new ProductCategory
            {
                ProductCategoryId = secondCategoryId,
                ProductCategoryGroupId = groupId,
                Title = "دسته دوم",
            });
        context.Brands.AddRange(
            new Brand { BrandId = firstBrandId, Title = "برند اول" },
            new Brand { BrandId = secondBrandId, Title = "برند دوم" });
        context.ProductAttributes.AddRange(
            new ProductAttribute
            {
                ProductAttributeId = firstAttributeId,
                ProductCategoryId = firstCategoryId,
                Title = "ویژگی اول",
            },
            new ProductAttribute
            {
                ProductAttributeId = secondAttributeId,
                ProductCategoryId = secondCategoryId,
                Title = "ویژگی دوم",
            });
        await context.SaveChangesAsync();

        return new ProductFixture(
            centerProfileId,
            userId,
            groupId,
            firstCategoryId,
            secondCategoryId,
            firstBrandId,
            secondBrandId,
            firstAttributeId,
            secondAttributeId);
    }

    private sealed record ProductFixture(
        Guid CenterProfileId,
        Guid UserId,
        int CategoryGroupId,
        int FirstCategoryId,
        int SecondCategoryId,
        int FirstBrandId,
        int SecondBrandId,
        int FirstAttributeId,
        int SecondAttributeId);

    private sealed class TestHostEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "LabConnectPortal.Tests";
        public string WebRootPath { get; set; } = contentRoot;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
