using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.Utils;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ProductCatalogService(
    LabConnectDbContext context,
    IFileStorageService fileStorageService,
    INotificationService notificationService,
    IOptions<PortalSettings> portalSettings,
    IOptions<FileStorageSettings> fileStorageSettings) : IProductCatalogService
{
    private readonly PortalSettings _portalSettings = portalSettings.Value;
    private readonly FileStorageSettings _fileStorageSettings = fileStorageSettings.Value;
    public async Task<OperationResult<PagedResult<ProductCategoryDto>>> GetCategoriesAsync(GetProductCategoriesQuery query)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.ProductCategories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(c => c.Title.Contains(query.Title.Trim()));

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ProductCategoryDto
            {
                ProductCategoryId = c.ProductCategoryId,
                Title = c.Title,
                AcceptsResume = c.AcceptsResume,
                ProductCategoryGroupId = c.ProductCategoryGroupId,
            })
            .ToListAsync();

        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<List<ProductCategoryDto>>> GetAllCategoriesAsync()
    {
        var items = await context.ProductCategories.AsNoTracking()
            .OrderBy(c => c.Title)
            .Select(c => new ProductCategoryDto
            {
                ProductCategoryId = c.ProductCategoryId,
                Title = c.Title,
                AcceptsResume = c.AcceptsResume,
                ProductCategoryGroupId = c.ProductCategoryGroupId,
            })
            .ToListAsync();
        return OperationResult<List<ProductCategoryDto>>.Success(items);
    }

    public async Task<OperationResult<List<ProductApiCategoryDto>>> GetApiCategoriesAsync()
    {
        var items = await context.ProductCategories.AsNoTracking()
            .OrderBy(c => c.CategoryGroup == null ? string.Empty : c.CategoryGroup.Name)
            .ThenBy(c => c.Title)
            .Select(c => new ProductApiCategoryDto
            {
                Id = c.ProductCategoryId,
                Title = c.Title,
                ProductCategoryGroupId = c.ProductCategoryGroupId,
                ProductCategoryGroupTitle = c.CategoryGroup == null ? null : c.CategoryGroup.Name,
                AcceptsResume = c.AcceptsResume,
            })
            .ToListAsync();

        return OperationResult<List<ProductApiCategoryDto>>.Success(items);
    }

    public async Task<OperationResult<List<CategoryOptionDto>>> GetCategoriesLookupAsync()
    {
        var items = await context.ProductCategoryGroups.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryOptionDto
            {
                Id = c.ProductCategoryGroupId,
                Title = c.Name,
            })
            .ToListAsync();
        return OperationResult<List<CategoryOptionDto>>.Success(items);
    }

    public async Task<OperationResult<List<ProductCategoryMappingDto>>> GetProductCategoriesByProductIdsAsync(GetProductCategoriesByProductIdsQuery query)
    {
        var productIds = query.ProductIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? [];
        if (productIds.Count == 0)
            return OperationResult<List<ProductCategoryMappingDto>>.Success([]);

        var items = await context.ProductCategoryAssignments.AsNoTracking()
            .Where(a => productIds.Contains(a.ProductId) && a.Category.ProductCategoryGroupId.HasValue)
            .Select(a => new ProductCategoryMappingDto
            {
                ProductId = a.ProductId,
                ProductCategoryId = a.Category.ProductCategoryGroupId!.Value,
            })
            .ToListAsync();

        return OperationResult<List<ProductCategoryMappingDto>>.Success(items);
    }

    public async Task<OperationResult<ProductCategoryDto>> CreateCategoryAsync(SaveProductCategoryCommand command)
    {
        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<ProductCategoryDto>.Failure("عنوان الزامی است");

        if (await context.ProductCategories.AnyAsync(c => c.Title == title))
            return OperationResult<ProductCategoryDto>.Failure("این عنوان قبلاً ثبت شده است");

        var entity = new ProductCategory
        {
            Title = title,
            AcceptsResume = command.AcceptsResume,
        };
        context.ProductCategories.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductCategoryDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductCategoryDto>.Success(MapCategory(entity));
    }

    public async Task<OperationResult<ProductCategoryDto>> UpdateCategoryAsync(UpdateProductCategoryCommand command)
    {
        var entity = await context.ProductCategories.FirstOrDefaultAsync(c => c.ProductCategoryId == command.ProductCategoryId);
        if (entity == null)
            return OperationResult<ProductCategoryDto>.Failure("دسته یافت نشد");

        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<ProductCategoryDto>.Failure("عنوان الزامی است");

        if (await context.ProductCategories.AnyAsync(c => c.Title == title && c.ProductCategoryId != command.ProductCategoryId))
            return OperationResult<ProductCategoryDto>.Failure("این عنوان قبلاً ثبت شده است");

        entity.Title = title;
        entity.AcceptsResume = command.AcceptsResume;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductCategoryDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductCategoryDto>.Success(MapCategory(entity));
    }

    public async Task<OperationResult> DeleteCategoryAsync(int productCategoryId)
    {
        var entity = await context.ProductCategories.FirstOrDefaultAsync(c => c.ProductCategoryId == productCategoryId);
        if (entity == null)
            return OperationResult.Failure("دسته یافت نشد");

        var inUse = await context.ProductCategoryAssignments.AnyAsync(a => a.ProductCategoryId == productCategoryId);
        if (inUse)
            return OperationResult.Failure("این دسته در محصولات استفاده شده و قابل حذف نیست");

        context.ProductCategories.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<PagedResult<ProductCategoryGroupDto>>> GetCategoryGroupsAsync(GetProductCategoryGroupsQuery query)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.ProductCategoryGroups.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Name))
            dbQuery = dbQuery.Where(g => g.Name.Contains(query.Name.Trim()));

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new ProductCategoryGroupDto
            {
                ProductCategoryGroupId = g.ProductCategoryGroupId,
                Name = g.Name,
                HomePageImagePath = g.HomePageImagePath,
                ShowOnHomePage = g.ShowOnHomePage,
                HasHomePageImage = g.HomePageImagePath != null && g.HomePageImagePath != string.Empty,
                CategoryCount = g.Categories.Count,
                Categories = g.Categories
                    .OrderBy(c => c.Title)
                    .Select(c => new ProductCategoryGroupItemDto
                    {
                        ProductCategoryId = c.ProductCategoryId,
                        Title = c.Title,
                    })
                    .ToList(),
            })
            .ToListAsync();

        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<List<ProductCategoryGroupDto>>> GetAllCategoryGroupsAsync()
    {
        var items = await context.ProductCategoryGroups.AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new ProductCategoryGroupDto
            {
                ProductCategoryGroupId = g.ProductCategoryGroupId,
                Name = g.Name,
                HomePageImagePath = g.HomePageImagePath,
                ShowOnHomePage = g.ShowOnHomePage,
                HasHomePageImage = g.HomePageImagePath != null && g.HomePageImagePath != string.Empty,
                CategoryCount = g.Categories.Count,
                Categories = g.Categories
                    .OrderBy(c => c.Title)
                    .Select(c => new ProductCategoryGroupItemDto
                    {
                        ProductCategoryId = c.ProductCategoryId,
                        Title = c.Title,
                    })
                    .ToList(),
            })
            .ToListAsync();

        return OperationResult<List<ProductCategoryGroupDto>>.Success(items);
    }

    public async Task<OperationResult<ProductCategoryGroupDto>> CreateCategoryGroupAsync(SaveProductCategoryGroupCommand command)
    {
        var name = command.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<ProductCategoryGroupDto>.Failure("نام سرگروه الزامی است");

        if (await context.ProductCategoryGroups.AnyAsync(g => g.Name == name))
            return OperationResult<ProductCategoryGroupDto>.Failure("این نام قبلاً ثبت شده است");

        var categoryIds = (command.ProductCategoryIds ?? []).Distinct().ToList();
        var assignResult = await ValidateAndLoadAssignableCategoriesAsync(categoryIds, currentGroupId: null);
        if (!assignResult.Status)
            return OperationResult<ProductCategoryGroupDto>.Failure(assignResult.Message ?? "خطا در اعتبارسنجی دسته‌ها");

        var entity = new ProductCategoryGroup
        {
            Name = name,
            ShowOnHomePage = command.ShowOnHomePage,
        };
        context.ProductCategoryGroups.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductCategoryGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        foreach (var category in assignResult.Data!)
            category.ProductCategoryGroupId = entity.ProductCategoryGroupId;

        save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductCategoryGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductCategoryGroupDto>.Success(await MapCategoryGroupAsync(entity.ProductCategoryGroupId));
    }

    public async Task<OperationResult<ProductCategoryGroupDto>> UpdateCategoryGroupAsync(UpdateProductCategoryGroupCommand command)
    {
        var entity = await context.ProductCategoryGroups
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.ProductCategoryGroupId == command.ProductCategoryGroupId);
        if (entity == null)
            return OperationResult<ProductCategoryGroupDto>.Failure("سرگروه یافت نشد");

        var name = command.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<ProductCategoryGroupDto>.Failure("نام سرگروه الزامی است");

        if (await context.ProductCategoryGroups.AnyAsync(g =>
                g.Name == name && g.ProductCategoryGroupId != command.ProductCategoryGroupId))
            return OperationResult<ProductCategoryGroupDto>.Failure("این نام قبلاً ثبت شده است");

        var categoryIds = (command.ProductCategoryIds ?? []).Distinct().ToList();
        var assignResult = await ValidateAndLoadAssignableCategoriesAsync(categoryIds, command.ProductCategoryGroupId);
        if (!assignResult.Status)
            return OperationResult<ProductCategoryGroupDto>.Failure(assignResult.Message ?? "خطا در اعتبارسنجی دسته‌ها");

        entity.Name = name;
        entity.ShowOnHomePage = command.ShowOnHomePage;

        var selectedIds = categoryIds.ToHashSet();
        foreach (var category in entity.Categories.Where(c => !selectedIds.Contains(c.ProductCategoryId)).ToList())
            category.ProductCategoryGroupId = null;

        foreach (var category in assignResult.Data!)
            category.ProductCategoryGroupId = entity.ProductCategoryGroupId;

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductCategoryGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductCategoryGroupDto>.Success(await MapCategoryGroupAsync(entity.ProductCategoryGroupId));
    }

    public async Task<OperationResult> DeleteCategoryGroupAsync(int productCategoryGroupId)
    {
        var entity = await context.ProductCategoryGroups
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.ProductCategoryGroupId == productCategoryGroupId);
        if (entity == null)
            return OperationResult.Failure("سرگروه یافت نشد");

        foreach (var category in entity.Categories)
            category.ProductCategoryGroupId = null;

        fileStorageService.DeleteFileIfExists(entity.HomePageImagePath);
        context.ProductCategoryGroups.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<ProductCategoryGroupDto>> UploadCategoryGroupHomePageImageAsync(
        int productCategoryGroupId,
        IFormFile file)
    {
        var entity = await context.ProductCategoryGroups
            .FirstOrDefaultAsync(g => g.ProductCategoryGroupId == productCategoryGroupId);
        if (entity == null)
            return OperationResult<ProductCategoryGroupDto>.Failure("سرگروه یافت نشد");

        var saveResult = await fileStorageService.SaveCategoryGroupHomePageImageAsync(
            productCategoryGroupId,
            file,
            entity.HomePageImagePath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<ProductCategoryGroupDto>.Failure(saveResult.Message ?? "خطا در آپلود");

        entity.HomePageImagePath = saveResult.Data;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductCategoryGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductCategoryGroupDto>.Success(await MapCategoryGroupAsync(entity.ProductCategoryGroupId));
    }

    public async Task<OperationResult<ProductCategoryGroupDto>> DeleteCategoryGroupHomePageImageAsync(
        int productCategoryGroupId)
    {
        var entity = await context.ProductCategoryGroups
            .FirstOrDefaultAsync(g => g.ProductCategoryGroupId == productCategoryGroupId);
        if (entity == null)
            return OperationResult<ProductCategoryGroupDto>.Failure("سرگروه یافت نشد");

        if (string.IsNullOrWhiteSpace(entity.HomePageImagePath))
            return OperationResult<ProductCategoryGroupDto>.Failure("تصویری برای این سرگروه ثبت نشده است");

        fileStorageService.DeleteFileIfExists(entity.HomePageImagePath);
        entity.HomePageImagePath = null;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductCategoryGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductCategoryGroupDto>.Success(await MapCategoryGroupAsync(entity.ProductCategoryGroupId));
    }

    public async Task<OperationResult<PagedResult<ProductAttributeDto>>> GetAttributesAsync(GetProductAttributesQuery query)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.ProductAttributes.AsNoTracking().Include(a => a.Category).AsQueryable();
        if (query.ProductCategoryId.HasValue)
            dbQuery = dbQuery.Where(a => a.ProductCategoryId == query.ProductCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(a => a.Title.Contains(query.Title.Trim()));

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderBy(a => a.Category.Title).ThenBy(a => a.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ProductAttributeDto
            {
                ProductAttributeId = a.ProductAttributeId,
                ProductCategoryId = a.ProductCategoryId,
                CategoryTitle = a.Category.Title,
                Title = a.Title,
                FieldType = a.FieldType,
            })
            .ToListAsync();

        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<List<ProductAttributeDto>>> GetAttributesByCategoryIdsAsync(GetAttributesByCategoryIdsQuery query)
    {
        var categoryIds = query.CategoryIds?.Distinct().ToList() ?? [];
        if (categoryIds.Count == 0)
            return OperationResult<List<ProductAttributeDto>>.Success([]);

        var items = await context.ProductAttributes.AsNoTracking()
            .Include(a => a.Category)
            .Where(a => categoryIds.Contains(a.ProductCategoryId))
            .OrderBy(a => a.Category.Title).ThenBy(a => a.Title)
            .Select(a => new ProductAttributeDto
            {
                ProductAttributeId = a.ProductAttributeId,
                ProductCategoryId = a.ProductCategoryId,
                CategoryTitle = a.Category.Title,
                Title = a.Title,
                FieldType = a.FieldType,
            })
            .ToListAsync();

        return OperationResult<List<ProductAttributeDto>>.Success(items);
    }

    public async Task<OperationResult<List<ProductApiLookupItemDto>>> GetApiAttributesAsync(
        GetAttributesByCategoryIdsQuery query)
    {
        var categoryIds = query.CategoryIds?.Distinct().ToList() ?? [];
        var dbQuery = context.ProductAttributes.AsNoTracking().AsQueryable();
        if (categoryIds.Count > 0)
            dbQuery = dbQuery.Where(x => categoryIds.Contains(x.ProductCategoryId));

        var items = await dbQuery
            .OrderBy(x => x.Title)
            .Select(x => new ProductApiLookupItemDto
            {
                Id = x.ProductAttributeId,
                Title = x.Title,
            })
            .ToListAsync();

        return OperationResult<List<ProductApiLookupItemDto>>.Success(items);
    }

    public async Task<OperationResult<ProductAttributeDto>> CreateAttributeAsync(SaveProductAttributeCommand command)
    {
        var validation = await ValidateAttributeCommandAsync(command.ProductCategoryId, command.Title, null);
        if (validation != null)
            return OperationResult<ProductAttributeDto>.Failure(validation);

        var entity = new ProductAttribute
        {
            ProductCategoryId = command.ProductCategoryId,
            Title = command.Title.Trim(),
            FieldType = command.FieldType,
        };
        context.ProductAttributes.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductAttributeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var category = await context.ProductCategories.AsNoTracking()
            .FirstAsync(c => c.ProductCategoryId == entity.ProductCategoryId);

        return OperationResult<ProductAttributeDto>.Success(new ProductAttributeDto
        {
            ProductAttributeId = entity.ProductAttributeId,
            ProductCategoryId = entity.ProductCategoryId,
            CategoryTitle = category.Title,
            Title = entity.Title,
            FieldType = entity.FieldType,
        });
    }

    public async Task<OperationResult<ProductAttributeDto>> UpdateAttributeAsync(UpdateProductAttributeCommand command)
    {
        var entity = await context.ProductAttributes.FirstOrDefaultAsync(a => a.ProductAttributeId == command.ProductAttributeId);
        if (entity == null)
            return OperationResult<ProductAttributeDto>.Failure("ویژگی یافت نشد");

        var validation = await ValidateAttributeCommandAsync(command.ProductCategoryId, command.Title, command.ProductAttributeId);
        if (validation != null)
            return OperationResult<ProductAttributeDto>.Failure(validation);

        entity.ProductCategoryId = command.ProductCategoryId;
        entity.Title = command.Title.Trim();
        entity.FieldType = command.FieldType;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductAttributeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var category = await context.ProductCategories.AsNoTracking()
            .FirstAsync(c => c.ProductCategoryId == entity.ProductCategoryId);

        return OperationResult<ProductAttributeDto>.Success(new ProductAttributeDto
        {
            ProductAttributeId = entity.ProductAttributeId,
            ProductCategoryId = entity.ProductCategoryId,
            CategoryTitle = category.Title,
            Title = entity.Title,
            FieldType = entity.FieldType,
        });
    }

    public async Task<OperationResult> DeleteAttributeAsync(int productAttributeId)
    {
        var entity = await context.ProductAttributes.FirstOrDefaultAsync(a => a.ProductAttributeId == productAttributeId);
        if (entity == null)
            return OperationResult.Failure("ویژگی یافت نشد");

        var inUse = await context.ProductAttributeValues.AnyAsync(v => v.ProductAttributeId == productAttributeId);
        if (inUse)
            return OperationResult.Failure("این ویژگی در محصولات استفاده شده و قابل حذف نیست");

        context.ProductAttributes.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<PagedResult<BrandDto>>> GetBrandsAsync(GetBrandsQuery query)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.Brands.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(b => b.Title.Contains(query.Title.Trim()));

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BrandDto
            {
                BrandId = b.BrandId,
                Title = b.Title,
                ImagePath = b.ImagePath,
                HasImage = b.ImagePath != null && b.ImagePath != string.Empty,
                ShowOnHomePage = b.ShowOnHomePage,
            })
            .ToListAsync();

        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<List<BrandDto>>> GetAllBrandsAsync()
    {
        var items = await context.Brands.AsNoTracking()
            .OrderBy(b => b.Title)
            .Select(b => new BrandDto
            {
                BrandId = b.BrandId,
                Title = b.Title,
                ImagePath = b.ImagePath,
                HasImage = b.ImagePath != null && b.ImagePath != string.Empty,
                ShowOnHomePage = b.ShowOnHomePage,
            })
            .ToListAsync();
        return OperationResult<List<BrandDto>>.Success(items);
    }

    public async Task<OperationResult<List<ProductApiLookupItemDto>>> GetApiBrandsAsync()
    {
        var items = await context.Brands.AsNoTracking()
            .OrderBy(x => x.Title)
            .Select(x => new ProductApiLookupItemDto
            {
                Id = x.BrandId,
                Title = x.Title,
            })
            .ToListAsync();

        return OperationResult<List<ProductApiLookupItemDto>>.Success(items);
    }

    public async Task<OperationResult<BrandDto>> CreateBrandAsync(SaveBrandCommand command)
    {
        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<BrandDto>.Failure("عنوان الزامی است");

        if (await context.Brands.AnyAsync(b => b.Title == title))
            return OperationResult<BrandDto>.Failure("این عنوان قبلاً ثبت شده است");

        var entity = new Brand
        {
            Title = title,
            ShowOnHomePage = command.ShowOnHomePage,
        };
        context.Brands.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<BrandDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<BrandDto>.Success(MapBrand(entity));
    }

    public async Task<OperationResult<BrandDto>> UpdateBrandAsync(UpdateBrandCommand command)
    {
        var entity = await context.Brands.FirstOrDefaultAsync(b => b.BrandId == command.BrandId);
        if (entity == null)
            return OperationResult<BrandDto>.Failure("برند یافت نشد");

        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<BrandDto>.Failure("عنوان الزامی است");

        if (await context.Brands.AnyAsync(b => b.Title == title && b.BrandId != command.BrandId))
            return OperationResult<BrandDto>.Failure("این عنوان قبلاً ثبت شده است");

        entity.Title = title;
        entity.ShowOnHomePage = command.ShowOnHomePage;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<BrandDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<BrandDto>.Success(MapBrand(entity));
    }

    public async Task<OperationResult> DeleteBrandAsync(int brandId)
    {
        var entity = await context.Brands.FirstOrDefaultAsync(b => b.BrandId == brandId);
        if (entity == null)
            return OperationResult.Failure("برند یافت نشد");

        var inUse = await context.Products.AnyAsync(p => p.BrandId == brandId);
        if (inUse)
            return OperationResult.Failure("این برند در محصولات استفاده شده و قابل حذف نیست");

        fileStorageService.DeleteFileIfExists(entity.ImagePath);
        context.Brands.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<BrandDto>> UploadBrandImageAsync(int brandId, IFormFile file)
    {
        var entity = await context.Brands.FirstOrDefaultAsync(b => b.BrandId == brandId);
        if (entity == null)
            return OperationResult<BrandDto>.Failure("برند یافت نشد");

        var saveResult = await fileStorageService.SaveBrandImageAsync(brandId, file, entity.ImagePath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<BrandDto>.Failure(saveResult.Message ?? "خطا در آپلود");

        entity.ImagePath = saveResult.Data;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<BrandDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<BrandDto>.Success(MapBrand(entity));
    }

    public async Task<OperationResult<BrandDto>> DeleteBrandImageAsync(int brandId)
    {
        var entity = await context.Brands.FirstOrDefaultAsync(b => b.BrandId == brandId);
        if (entity == null)
            return OperationResult<BrandDto>.Failure("برند یافت نشد");

        if (string.IsNullOrWhiteSpace(entity.ImagePath))
            return OperationResult<BrandDto>.Failure("تصویری برای این برند ثبت نشده است");

        fileStorageService.DeleteFileIfExists(entity.ImagePath);
        entity.ImagePath = null;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<BrandDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<BrandDto>.Success(MapBrand(entity));
    }

    public async Task<OperationResult<PagedResult<ProductListItemDto>>> GetProductsAsync(GetProductsQuery query, Guid? userId = null)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var access = userId.HasValue ? await ResolveProductAccessAsync(userId.Value) : null;

        var dbQuery = context.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.CreatedByUser)
            .Include(p => p.CreatedByCenterProfile)
            .Include(p => p.CategoryAssignments).ThenInclude(a => a.Category)
            .AsQueryable();

        if (access is { IsSiteAdmin: false })
        {
            if (!access.CenterProfileId.HasValue)
                return OperationResult<PagedResult<ProductListItemDto>>.Failure("پروفایل مرکز یافت نشد");
            dbQuery = dbQuery.Where(p => p.CreatedByCenterProfileId == access.CenterProfileId);
        }
        else if (query.MineOnly == true && access?.CenterProfileId is Guid centerId)
        {
            dbQuery = dbQuery.Where(p => p.CreatedByCenterProfileId == centerId);
        }

        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(p => p.Title.Contains(query.Title.Trim()));
        if (!string.IsNullOrWhiteSpace(query.CreatedByUserName))
        {
            var userTerm = query.CreatedByUserName.Trim();
            dbQuery = dbQuery.Where(p =>
                p.CreatedByUser != null &&
                (
                    (p.CreatedByUser.FirstName + " " + p.CreatedByUser.LastName).Contains(userTerm) ||
                    p.CreatedByUser.FirstName.Contains(userTerm) ||
                    p.CreatedByUser.LastName.Contains(userTerm) ||
                    p.CreatedByUser.Username.Contains(userTerm)
                ));
        }
        if (query.CreatedByCenterProfileId.HasValue)
            dbQuery = dbQuery.Where(p => p.CreatedByCenterProfileId == query.CreatedByCenterProfileId.Value);
        if (!string.IsNullOrWhiteSpace(query.CreatedByCenterName))
        {
            var centerTerm = query.CreatedByCenterName.Trim();
            dbQuery = dbQuery.Where(p =>
                p.CreatedByCenterProfile != null &&
                p.CreatedByCenterProfile.Name.Contains(centerTerm));
        }
        if (query.BrandId.HasValue)
            dbQuery = dbQuery.Where(p => p.BrandId == query.BrandId.Value);
        if (query.CategoryId.HasValue)
            dbQuery = dbQuery.Where(p => p.CategoryAssignments.Any(a => a.ProductCategoryId == query.CategoryId.Value));
        if (query.PendingApprovalOnly == true)
            dbQuery = dbQuery.Where(p => p.Status == ProductStatus.PendingApproval);
        else if (query.Status.HasValue)
            dbQuery = dbQuery.Where(p => p.Status == query.Status.Value);
        // Match product-form isResumeListing: every assigned category accepts resume.
        if (query.AcceptsResume == true)
        {
            dbQuery = dbQuery.Where(p =>
                p.CategoryAssignments.Any() &&
                p.CategoryAssignments.All(a =>
                    context.ProductCategories.Any(c =>
                        c.ProductCategoryId == a.ProductCategoryId && c.AcceptsResume)));
        }
        else if (query.AcceptsResume == false)
        {
            dbQuery = dbQuery.Where(p =>
                !p.CategoryAssignments.Any() ||
                p.CategoryAssignments.Any(a =>
                    context.ProductCategories.Any(c =>
                        c.ProductCategoryId == a.ProductCategoryId && !c.AcceptsResume)));
        }
        if (query.UpdatedFrom.HasValue)
            dbQuery = dbQuery.Where(p => p.UpdatedAt >= query.UpdatedFrom.Value);
        if (query.UpdatedTo.HasValue)
            dbQuery = dbQuery.Where(p => p.UpdatedAt <= query.UpdatedTo.Value);

        var totalCount = await dbQuery.CountAsync();
        var products = await dbQuery
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = products.Select(MapListItem).ToList();
        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<PagedResult<ProductCatalogOptionDto>>> SearchCatalogAsync(
        SearchCatalogQuery query,
        Guid? userId = null)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Where(p => p.Status == ProductStatus.Approved);

        if (userId.HasValue)
        {
            var access = await ResolveProductAccessAsync(userId.Value);
            if (!access.IsSiteAdmin)
            {
                if (!access.CenterProfileId.HasValue)
                    return OperationResult<PagedResult<ProductCatalogOptionDto>>.Failure("پروفایل مرکز یافت نشد");
                dbQuery = dbQuery.Where(p => p.CreatedByCenterProfileId == access.CenterProfileId);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            var term = query.Title.Trim();
            dbQuery = dbQuery.Where(p =>
                p.Title.Contains(term) || (p.Brand != null && p.Brand.Title.Contains(term)));
        }

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderBy(p => p.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductCatalogOptionDto
            {
                ProductId = p.ProductId,
                Title = p.Title,
                BrandTitle = p.Brand != null ? p.Brand.Title : string.Empty,
            })
            .ToListAsync();

        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<ProductDto>> GetProductByIdAsync(Guid productId, Guid? userId = null)
    {
        var product = await LoadProductAsync(productId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        if (userId.HasValue)
        {
            var access = await ResolveProductAccessAsync(userId.Value);
            if (!CanViewProduct(access, product))
                return OperationResult<ProductDto>.Failure("دسترسی به این محصول مجاز نیست");
        }

        return OperationResult<ProductDto>.Success(ToProductDto(product));
    }

    public Task<OperationResult<ProductDto>> CreateProductAsync(SaveProductCommand command, Guid userId)
        => CreateProductInternalAsync(command, userId, null);

    public async Task<OperationResult<ProductDto>> CreateProductFromApiAsync(
        ProductApiCreateProductCommand command,
        Guid userId)
    {
        var imageError = TryDecodeApiProductImage(command, out var image);
        if (imageError != null)
            return OperationResult<ProductDto>.Failure(imageError);

        return await CreateProductInternalAsync(command, userId, image);
    }

    private async Task<OperationResult<ProductDto>> CreateProductInternalAsync(
        SaveProductCommand command,
        Guid userId,
        ProductApiImagePayload? featuredImage)
    {
        var access = await ResolveProductAccessAsync(userId);
        if (access.IsSiteAdmin)
            return OperationResult<ProductDto>.Failure("مدیر سیستم مجاز به ایجاد محصول نیست");
        if (!access.CanManageCatalog)
            return OperationResult<ProductDto>.Failure("دسترسی ایجاد محصول وجود ندارد");

        var centerProfileId = access.CenterProfileId;
        if (centerProfileId == null)
            return OperationResult<ProductDto>.Failure("پروفایل مرکز یافت نشد");

        var validation = await ValidateProductCommandAsync(command);
        if (validation != null)
            return OperationResult<ProductDto>.Failure(validation);

        var offerValidation = ValidateOfferFields(command);
        if (offerValidation != null)
            return OperationResult<ProductDto>.Failure(offerValidation);

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Title = command.Title.Trim(),
            BrandId = NormalizeBrandId(command.BrandId),
            Warranty = command.Warranty?.Trim() ?? string.Empty,
            Description = command.Description?.Trim() ?? string.Empty,
            Status = ProductStatus.Draft,
            CreatedByUserId = userId,
            CreatedByCenterProfileId = centerProfileId,
            CreatedAt = DateTime.UtcNow.ToLocalTime(),
            UpdatedAt = DateTime.UtcNow.ToLocalTime(),
        };

        await ApplyOfferFieldsAsync(product, command);

        if (featuredImage != null)
        {
            await using var imageStream = new MemoryStream(featuredImage.Bytes, writable: false);
            var imageFile = new FormFile(
                imageStream,
                0,
                imageStream.Length,
                nameof(ProductApiCreateProductCommand.FeaturedImageBase64),
                featuredImage.FileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = featuredImage.ContentType,
            };

            var imageSave = await fileStorageService.SaveProductImageAsync(product.ProductId, imageFile, null);
            if (!imageSave.Status || imageSave.Data == null)
                return OperationResult<ProductDto>.Failure(imageSave.Message ?? "خطا در ذخیره تصویر");

            product.FeaturedImagePath = imageSave.Data;
        }

        context.Products.Add(product);
        await SyncCategoriesAsync(product, command.CategoryIds);
        await SyncAttributeValuesAsync(product, command.AttributeValues);
        await SyncExpertReviewsAsync(product, command.ExpertReviews);

        var save = await SaveChangesAsync();
        if (!save.Success)
        {
            if (!string.IsNullOrWhiteSpace(product.FeaturedImagePath))
                fileStorageService.DeleteFileIfExists(product.FeaturedImagePath);
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");
        }

        var created = await LoadProductAsync(product.ProductId);
        return OperationResult<ProductDto>.Success(ToProductDto(created!));
    }

    public async Task<OperationResult<ProductDto>> UpdateProductAsync(UpdateProductCommand command, Guid userId)
    {
        var product = await context.Products
            .Include(p => p.CategoryAssignments)
            .Include(p => p.AttributeValues)
            .Include(p => p.ExpertReviews)
            .FirstOrDefaultAsync(p => p.ProductId == command.ProductId);

        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanEditProductAsync(access, product))
            return OperationResult<ProductDto>.Failure("ویرایش این محصول مجاز نیست");

        if (!access.IsSiteAdmin && product.Status is ProductStatus.PendingApproval or ProductStatus.Approved)
            return OperationResult<ProductDto>.Failure("برای ویرایش، ابتدا محصول را از حالت انتشار خارج کنید");

        var validation = await ValidateProductCommandAsync(command);
        if (validation != null)
            return OperationResult<ProductDto>.Failure(validation);

        var offerValidation = ValidateOfferFields(command);
        if (offerValidation != null)
            return OperationResult<ProductDto>.Failure(offerValidation);

        product.Title = command.Title.Trim();
        product.BrandId = NormalizeBrandId(command.BrandId);
        product.Warranty = command.Warranty?.Trim() ?? string.Empty;
        product.Description = command.Description?.Trim() ?? string.Empty;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        if (product.Status == ProductStatus.Rejected)
            product.Status = ProductStatus.Draft;
        else
            MarkPendingAfterOwnerEdit(access, product);

        context.ProductCategoryAssignments.RemoveRange(product.CategoryAssignments);
        context.ProductAttributeValues.RemoveRange(product.AttributeValues);
        context.ProductExpertReviews.RemoveRange(product.ExpertReviews);

        await SyncCategoriesAsync(product, command.CategoryIds);
        await SyncAttributeValuesAsync(product, command.AttributeValues);
        await SyncExpertReviewsAsync(product, command.ExpertReviews);
        await ApplyOfferFieldsAsync(product, command);

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var updated = await LoadProductAsync(product.ProductId);
        return OperationResult<ProductDto>.Success(ToProductDto(updated!));
    }

    public async Task<OperationResult> DeleteProductAsync(Guid productId, Guid userId)
    {
        var product = await context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            return OperationResult.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (access.IsSiteAdmin)
            return OperationResult.Failure("مدیر سیستم مجاز به حذف محصول نیست");
        if (!await CanEditProductAsync(access, product))
            return OperationResult.Failure("حذف این محصول مجاز نیست");

        if (!access.IsSiteAdmin && product.Status != ProductStatus.Draft && product.Status != ProductStatus.Rejected)
            return OperationResult.Failure("فقط پیش‌نویس یا ردشده قابل حذف است");

        foreach (var image in product.Images)
            fileStorageService.DeleteFileIfExists(image.ImagePath);
        fileStorageService.DeleteFileIfExists(product.FeaturedImagePath);

        context.Products.Remove(product);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<ProductDto>> SubmitForApprovalAsync(Guid productId, Guid userId)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanEditProductAsync(access, product))
            return OperationResult<ProductDto>.Failure("ارسال برای تأیید مجاز نیست");

        if (product.Status is not (ProductStatus.Draft or ProductStatus.Rejected))
            return OperationResult<ProductDto>.Failure("فقط پیش‌نویس یا ردشده قابل ارسال برای تأیید است");

        var completeness = await ValidateProductCompletenessAsync(productId);
        if (completeness != null)
            return OperationResult<ProductDto>.Failure(completeness);

        product.Status = ProductStatus.PendingApproval;
        product.SubmittedAt = DateTime.UtcNow.ToLocalTime();
        product.RejectionReason = null;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductDto>.Success(ToProductDto((await LoadProductAsync(productId))!));
    }

    public async Task<OperationResult<ProductDto>> UnpublishProductAsync(Guid productId, Guid userId)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanManageProductLifecycleAsync(access, product))
            return OperationResult<ProductDto>.Failure("لغو انتشار مجاز نیست");

        if (product.Status != ProductStatus.Approved)
            return OperationResult<ProductDto>.Failure("فقط محصول منتشرشده قابل لغو انتشار است");

        product.Status = ProductStatus.Unpublished;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductDto>.Success(ToProductDto((await LoadProductAsync(productId))!));
    }

    public async Task<OperationResult<ProductDto>> RepublishProductAsync(Guid productId, Guid userId)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanManageProductLifecycleAsync(access, product))
            return OperationResult<ProductDto>.Failure("انتشار مجدد مجاز نیست");

        if (product.Status != ProductStatus.Unpublished)
            return OperationResult<ProductDto>.Failure("فقط محصول لغو‌انتشار‌شده بدون تغییر قابل انتشار مجدد است");

        product.Status = ProductStatus.Approved;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductDto>.Success(ToProductDto((await LoadProductAsync(productId))!));
    }

    public async Task<OperationResult<ProductDto>> ApproveProductAsync(Guid productId, Guid adminUserId)
    {
        var access = await ResolveProductAccessAsync(adminUserId);
        if (!access.IsSiteAdmin)
            return OperationResult<ProductDto>.Failure("فقط مدیر سایت می‌تواند محصول را تأیید کند");

        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        var completeness = await ValidateProductCompletenessAsync(productId);
        if (completeness != null)
            return OperationResult<ProductDto>.Failure(completeness);

        product.Status = ProductStatus.Approved;
        product.ApprovedAt = DateTime.UtcNow.ToLocalTime();
        product.ApprovedByUserId = adminUserId;
        product.RejectionReason = null;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        await notificationService.SendNotificationAsync(new NotificationCommandBase
        {
            Title = "آگهی جدید",
            Message = $"«{product.Title}» در سامانه منتشر شد.",
            Picture = await ToBase64PictureAsync(product.FeaturedImagePath),
            ExpireDate = DateTime.UtcNow.ToLocalTime().AddDays(3),
            IsActive = true,
            TargetUsers = [],
            NotificationActions =
            [
                new NotificationAction
                {
                    NotificationActionType = NotificationActionType.SystemEntity,
                    Label = "مشاهده آگهی",
                    Color = "bg-primary-600 text-white",
                    SystemEntityId = new Guid("7f3e21a8-4b9c-4d6e-9a1f-2c8e5d4b6a70"),
                    Url = _portalSettings.BuildPublicUrl($"products/{product.ProductId}"),
                    OpenTab = true
                },
            ],
        });

        return OperationResult<ProductDto>.Success(ToProductDto((await LoadProductAsync(productId))!));
    }

    public async Task<OperationResult<ProductDto>> RejectProductAsync(RejectProductCommand command, Guid adminUserId)
    {
        var access = await ResolveProductAccessAsync(adminUserId);
        if (!access.IsSiteAdmin)
            return OperationResult<ProductDto>.Failure("فقط مدیر سایت می‌تواند محصول را رد کند");

        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == command.ProductId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        product.Status = ProductStatus.Rejected;
        product.RejectionReason = command.RejectionReason?.Trim();
        product.ApprovedAt = null;
        product.ApprovedByUserId = null;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductDto>.Success(ToProductDto((await LoadProductAsync(command.ProductId))!));
    }

    public async Task<OperationResult<ProductImageDto>> UploadImageAsync(Guid productId, IFormFile file, Guid userId)
    {
        var product = await context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            return OperationResult<ProductImageDto>.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanEditProductAsync(access, product))
            return OperationResult<ProductImageDto>.Failure("آپلود تصویر مجاز نیست");

        if (!CanModifyProductContent(access, product))
            return OperationResult<ProductImageDto>.Failure("برای تغییر تصویر، ابتدا محصول را از حالت انتشار خارج کنید");

        var saveResult = await fileStorageService.SaveProductImageAsync(productId, file, null);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<ProductImageDto>.Failure(saveResult.Message ?? "خطا در آپلود");

        var sortOrder = product.Images.Count > 0 ? product.Images.Max(i => i.SortOrder) + 1 : 0;
        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ImagePath = saveResult.Data,
            SortOrder = sortOrder,
        };

        context.ProductImages.Add(image);
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        MarkPendingAfterOwnerEdit(access, product);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductImageDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductImageDto>.Success(new ProductImageDto
        {
            Id = image.Id,
            ImagePath = image.ImagePath,
            SortOrder = image.SortOrder,
        });
    }

    public async Task<OperationResult> DeleteImageAsync(Guid productId, Guid imageId, Guid userId)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanEditProductAsync(access, product))
            return OperationResult.Failure("حذف تصویر مجاز نیست");

        if (!CanModifyProductContent(access, product))
            return OperationResult.Failure("برای تغییر تصویر، ابتدا محصول را از حالت انتشار خارج کنید");

        var image = await context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId);

        if (image == null)
            return OperationResult.Failure("تصویر یافت نشد");

        fileStorageService.DeleteFileIfExists(image.ImagePath);
        context.ProductImages.Remove(image);
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        MarkPendingAfterOwnerEdit(access, product);

        return await SaveChangesAsync();
    }

    public async Task<OperationResult<ProductDto>> UploadFeaturedImageAsync(Guid productId, IFormFile file, Guid userId)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanEditProductAsync(access, product))
            return OperationResult<ProductDto>.Failure("آپلود تصویر شاخص مجاز نیست");

        if (!CanModifyProductContent(access, product))
            return OperationResult<ProductDto>.Failure("برای تغییر تصویر، ابتدا محصول را از حالت انتشار خارج کنید");

        var saveResult = await fileStorageService.SaveProductImageAsync(productId, file, product.FeaturedImagePath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<ProductDto>.Failure(saveResult.Message ?? "خطا در آپلود");

        product.FeaturedImagePath = saveResult.Data;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        MarkPendingAfterOwnerEdit(access, product);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductDto>.Success(ToProductDto((await LoadProductAsync(productId))!));
    }

    public async Task<OperationResult<ProductDto>> DeleteFeaturedImageAsync(Guid productId, Guid userId)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult<ProductDto>.Failure("محصول یافت نشد");

        var access = await ResolveProductAccessAsync(userId);
        if (!await CanEditProductAsync(access, product))
            return OperationResult<ProductDto>.Failure("حذف تصویر شاخص مجاز نیست");

        if (!CanModifyProductContent(access, product))
            return OperationResult<ProductDto>.Failure("برای تغییر تصویر، ابتدا محصول را از حالت انتشار خارج کنید");

        if (string.IsNullOrWhiteSpace(product.FeaturedImagePath))
            return OperationResult<ProductDto>.Failure("تصویر شاخصی ثبت نشده است");

        fileStorageService.DeleteFileIfExists(product.FeaturedImagePath);
        product.FeaturedImagePath = null;
        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        MarkPendingAfterOwnerEdit(access, product);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductDto>.Success(ToProductDto((await LoadProductAsync(productId))!));
    }

    public async Task<OperationResult<ProductProvinceDto>> ResolveProvinceFromLocationAsync(
        decimal latitude,
        decimal longitude)
    {
        var province = await ProvinceLocationResolver.ResolveAsync(context, latitude, longitude);
        if (province == null)
            return OperationResult<ProductProvinceDto>.Failure("استان متناظر با این موقعیت یافت نشد");

        return OperationResult<ProductProvinceDto>.Success(new ProductProvinceDto
        {
            ProvinceId = province.ProvinceId,
            ProvinceName = province.ProvinceName,
        });
    }

    private async Task<string?> ToBase64PictureAsync(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var (stream, contentType) = await fileStorageService.OpenProductImageAsync(relativePath);
        if (stream == null)
            return null;

        await using (stream)
        {
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            var base64 = Convert.ToBase64String(memory.ToArray());
            var mime = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;
            return $"data:{mime};base64,{base64}";
        }
    }

    private async Task<Product?> LoadProductAsync(Guid productId)
        => await context.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Province)
            .Include(p => p.CreatedByUser)
            .Include(p => p.CreatedByCenterProfile)
            .Include(p => p.CategoryAssignments).ThenInclude(a => a.Category).ThenInclude(c => c!.CategoryGroup)
            .Include(p => p.AttributeValues).ThenInclude(v => v.Attribute).ThenInclude(a => a.Category)
            .Include(p => p.Images)
            .Include(p => p.ExpertReviews)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

    private static ProductListItemDto MapListItem(Product product)
        => new()
        {
            ProductId = product.ProductId,
            Title = product.Title,
            BrandTitle = product.Brand?.Title ?? string.Empty,
            CategoryTitles = product.CategoryAssignments.Select(a => a.Category.Title).ToList(),
            Status = product.Status,
            FeaturedImagePath = product.FeaturedImagePath,
            HasFeaturedImage = !string.IsNullOrWhiteSpace(product.FeaturedImagePath),
            Price = product.Price,
            IsNegotiablePrice = product.IsNegotiablePrice,
            StockQuantity = product.StockQuantity,
            DiscountPercent = product.DiscountPercent,
            CreatedByUserId = product.CreatedByUserId,
            CreatedByUserName = FormatUserName(product.CreatedByUser),
            CreatedByCenterProfileId = product.CreatedByCenterProfileId,
            CreatedByCenterName = product.CreatedByCenterProfile?.Name ?? string.Empty,
            CreatedBySiteAdmin = IsCreatedBySiteAdmin(product.CreatedByUser),
            AcceptsResume = product.CategoryAssignments.Any(a => a.Category != null && a.Category.AcceptsResume),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
        };

    private static string FormatUserName(User? user)
    {
        if (user == null) return string.Empty;
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName;
        return string.IsNullOrWhiteSpace(user.Username) ? string.Empty : user.Username;
    }

    private static ProductDto MapProduct(Product product)
    {
        var categories = product.CategoryAssignments.Select(a => a.Category).ToList();
        var groupId = categories
            .Select(c => c.ProductCategoryGroupId)
            .FirstOrDefault(id => id.HasValue);
        var groupName = categories
            .FirstOrDefault(c => c.ProductCategoryGroupId == groupId)?.CategoryGroup?.Name;

        return new ProductDto
        {
            ProductId = product.ProductId,
            Title = product.Title,
            BrandId = product.BrandId,
            BrandTitle = product.Brand?.Title ?? string.Empty,
            Warranty = product.Warranty,
            Description = product.Description,
            ProductCategoryGroupId = groupId,
            ProductCategoryGroupName = groupName,
            CategoryIds = product.CategoryAssignments.Select(a => a.ProductCategoryId).ToList(),
            CategoryTitles = product.CategoryAssignments.Select(a => a.Category.Title).ToList(),
            AttributeValues = product.AttributeValues.Select(v => new ProductAttributeValueDto
            {
                ProductAttributeId = v.ProductAttributeId,
                AttributeTitle = v.Attribute.Title,
                ProductCategoryId = v.Attribute.ProductCategoryId,
                CategoryTitle = v.Attribute.Category.Title,
                Value = v.Value,
                FieldType = v.Attribute.FieldType,
            }).ToList(),
            Images = product.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImagePath = i.ImagePath,
                SortOrder = i.SortOrder,
            }).ToList(),
            ExpertReviews = product.ExpertReviews.OrderBy(r => r.SortOrder).Select(r => new ProductExpertReviewDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                SortOrder = r.SortOrder,
            }).ToList(),
            Status = product.Status,
            FeaturedImagePath = product.FeaturedImagePath,
            HasFeaturedImage = !string.IsNullOrWhiteSpace(product.FeaturedImagePath),
            RejectionReason = product.RejectionReason,
            CreatedByCenterProfileId = product.CreatedByCenterProfileId,
            CreatedByCenterName = product.CreatedByCenterProfile?.Name ?? string.Empty,
            CreatedByUserId = product.CreatedByUserId,
            CreatedByUserName = FormatUserName(product.CreatedByUser),
            CreatedBySiteAdmin = IsCreatedBySiteAdmin(product.CreatedByUser),
            Price = product.Price,
            IsNegotiablePrice = product.IsNegotiablePrice,
            IsUsed = product.IsUsed,
            StockQuantity = product.StockQuantity,
            DiscountPercent = product.DiscountPercent,
            Latitude = product.Latitude,
            Longitude = product.Longitude,
            ProvinceId = product.ProvinceId,
            ProvinceName = product.Province?.ProvinceName,
            SubmittedAt = product.SubmittedAt,
            ApprovedAt = product.ApprovedAt,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
        };
    }

    private static ProductDto ToProductDto(Product product) => MapProduct(product);

    private static string? ValidateOfferFields(SaveProductCommand command)
    {
        if (command.Price < 0)
            return "قیمت نمی‌تواند منفی باشد";

        if (!command.IsNegotiablePrice && command.Price <= 0)
            return "در حالت غیرتوافقی، وارد کردن قیمت الزامی است";

        if (command.StockQuantity < 0)
            return "موجودی نمی‌تواند منفی باشد";

        if (command.DiscountPercent is < 0 or > 100)
            return "درصد تخفیف باید بین ۰ تا ۱۰۰ باشد";

        return null;
    }

    private async Task ApplyOfferFieldsAsync(Product product, SaveProductCommand command)
    {
        var price = command.IsNegotiablePrice && command.Price < 0 ? 0 : command.Price;
        var stock = command.StockQuantity < 0 ? 0 : command.StockQuantity;
        var discount = command.DiscountPercent is > 0 and <= 100
            ? command.DiscountPercent
            : null;

        product.Price = price;
        product.IsNegotiablePrice = command.IsNegotiablePrice;
        product.IsUsed = command.IsUsed;
        product.StockQuantity = stock;
        product.DiscountPercent = discount;
        product.Latitude = command.Latitude;
        product.Longitude = command.Longitude;

        var province = await ProvinceLocationResolver.ResolveAsync(context, command.Latitude, command.Longitude);
        product.ProvinceId = province?.ProvinceId;
    }

    private async Task<string?> ValidateAttributeCommandAsync(int categoryId, string title, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "عنوان الزامی است";

        if (!await context.ProductCategories.AnyAsync(c => c.ProductCategoryId == categoryId))
            return "دسته یافت نشد";

        var trimmed = title.Trim();
        var exists = await context.ProductAttributes.AnyAsync(a =>
            a.ProductCategoryId == categoryId &&
            a.Title == trimmed &&
            (!excludeId.HasValue || a.ProductAttributeId != excludeId.Value));

        return exists ? "این ویژگی برای این دسته قبلاً ثبت شده است" : null;
    }

    private async Task<string?> ValidateProductCommandAsync(SaveProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return "عنوان محصول الزامی است";

        var categoryIds = command.CategoryIds?.Distinct().ToList() ?? [];
        if (categoryIds.Count == 0)
            return "حداقل یک دسته باید انتخاب شود";

        var categories = await context.ProductCategories
            .Where(c => categoryIds.Contains(c.ProductCategoryId))
            .ToListAsync();

        if (categories.Count != categoryIds.Count)
            return "یک یا چند دسته نامعتبر است";

        if (CategorySelectionRequiresBrand(categories))
        {
            if (!command.BrandId.HasValue || command.BrandId.Value <= 0)
                return "برند الزامی است";

            if (!await context.Brands.AnyAsync(b => b.BrandId == command.BrandId.Value))
                return "برند یافت نشد";
        }
        else if (command.BrandId is > 0 &&
                 !await context.Brands.AnyAsync(b => b.BrandId == command.BrandId.Value))
        {
            return "برند یافت نشد";
        }

        if (!await context.ProductCategoryGroups.AnyAsync(g => g.ProductCategoryGroupId == command.ProductCategoryGroupId))
            return "سرگروه دسته‌بندی یافت نشد";

        if (categories.Any(c => c.ProductCategoryGroupId != command.ProductCategoryGroupId))
            return "تمام دسته‌ها باید متعلق به سرگروه انتخاب‌شده باشند";

        var allowedAttributeIds = await context.ProductAttributes
            .Where(a => categoryIds.Contains(a.ProductCategoryId))
            .Select(a => a.ProductAttributeId)
            .ToListAsync();

        var providedIds = command.AttributeValues?.Select(v => v.ProductAttributeId).Distinct().ToList() ?? [];
        if (providedIds.Any(id => !allowedAttributeIds.Contains(id)))
            return "یک یا چند ویژگی برای دسته‌های انتخاب‌شده معتبر نیست";

        return null;
    }

    private async Task<string?> ValidateProductCompletenessAsync(Guid productId)
    {
        var product = await context.Products.AsNoTracking()
            .Include(p => p.CategoryAssignments).ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            return "محصول یافت نشد";

        var categoryIds = product.CategoryAssignments.Select(a => a.ProductCategoryId).Distinct().ToList();
        if (categoryIds.Count == 0)
            return "حداقل یک دسته باید انتخاب شود";

        var categories = product.CategoryAssignments
            .Select(a => a.Category)
            .Where(c => c != null)
            .Cast<ProductCategory>()
            .ToList();

        var groupIds = categories
            .Select(c => c.ProductCategoryGroupId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (groupIds.Count == 0)
            return "سرگروه دسته‌بندی محصول مشخص نیست";

        if (groupIds.Count > 1)
            return "تمام دسته‌ها باید متعلق به یک سرگروه باشند";

        if (CategorySelectionRequiresBrand(categories) &&
            (!product.BrandId.HasValue || product.BrandId.Value <= 0))
        {
            return "برند الزامی است";
        }

        return null;
    }

    private static int? NormalizeBrandId(int? brandId)
        => brandId is > 0 ? brandId : null;

    private static bool CategorySelectionRequiresBrand(IReadOnlyList<ProductCategory> categories)
        => categories.Count == 0 || categories.Any(c => !c.AcceptsResume);

    private sealed class ProductAccessContext
    {
        public bool IsSiteAdmin { get; init; }
        public bool CanManageCatalog { get; init; }
        public Guid? CenterProfileId { get; init; }
    }

    private async Task<ProductAccessContext> ResolveProductAccessAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return new ProductAccessContext();

        var isSiteAdmin = user.UserType is UserType.Administrator or UserType.Admin;
        if (isSiteAdmin)
        {
            return new ProductAccessContext
            {
                IsSiteAdmin = true,
                CanManageCatalog = true,
                CenterProfileId = user.CenterProfileId,
            };
        }

        var isCenterUser = user.UserType is UserType.AdminLab or UserType.UserLab or UserType.Store;
        return new ProductAccessContext
        {
            IsSiteAdmin = false,
            CanManageCatalog = isCenterUser && user.CenterProfileId.HasValue,
            CenterProfileId = user.CenterProfileId,
        };
    }

    private static bool CanViewProduct(ProductAccessContext access, Product product)
    {
        if (access.IsSiteAdmin)
            return true;
        if (product.Status == ProductStatus.Approved)
            return true;
        return access.CenterProfileId.HasValue && product.CreatedByCenterProfileId == access.CenterProfileId;
    }

    /// <summary>
    /// Site admins may edit any product (moderation). Center users may edit products of their center.
    /// </summary>
    private Task<bool> CanEditProductAsync(ProductAccessContext access, Product product)
    {
        if (access.IsSiteAdmin)
            return Task.FromResult(true);

        if (!access.CanManageCatalog || !access.CenterProfileId.HasValue)
            return Task.FromResult(false);

        return Task.FromResult(product.CreatedByCenterProfileId == access.CenterProfileId);
    }

    /// <summary>
    /// Approve/reject/unpublish lifecycle: site admin can act on any product; centers only on theirs.
    /// </summary>
    private async Task<bool> CanManageProductLifecycleAsync(ProductAccessContext access, Product product)
    {
        if (access.IsSiteAdmin)
            return true;

        return await CanEditProductAsync(access, product);
    }

    private static bool IsCreatedBySiteAdmin(User? creator)
    {
        // Legacy rows without creator are treated as site-admin catalog products.
        if (creator == null)
            return true;

        return creator.UserType is UserType.Administrator or UserType.Admin;
    }

    private static bool CanModifyProductContent(ProductAccessContext access, Product product)
    {
        if (access.IsSiteAdmin)
            return true;
        return product.Status is ProductStatus.Draft or ProductStatus.Rejected or ProductStatus.Unpublished;
    }

    private static void MarkPendingAfterOwnerEdit(ProductAccessContext access, Product product)
    {
        if (access.IsSiteAdmin || product.Status != ProductStatus.Unpublished)
            return;

        product.Status = ProductStatus.PendingApproval;
        product.SubmittedAt = DateTime.UtcNow.ToLocalTime();
        product.RejectionReason = null;
    }

    private Task SyncCategoriesAsync(Product product, List<int> categoryIds)
    {
        foreach (var categoryId in categoryIds.Distinct())
        {
            context.ProductCategoryAssignments.Add(new ProductCategoryAssignment
            {
                Id = Guid.NewGuid(),
                ProductId = product.ProductId,
                ProductCategoryId = categoryId,
            });
        }

        return Task.CompletedTask;
    }

    private Task SyncAttributeValuesAsync(Product product, List<SaveProductAttributeValueCommand> values)
    {
        foreach (var item in values ?? [])
        {
            context.ProductAttributeValues.Add(new ProductAttributeValue
            {
                Id = Guid.NewGuid(),
                ProductId = product.ProductId,
                ProductAttributeId = item.ProductAttributeId,
                Value = item.Value.Trim(),
            });
        }

        return Task.CompletedTask;
    }

    private Task SyncExpertReviewsAsync(Product product, List<ProductExpertReviewDto> reviews)
    {
        var sortOrder = 0;
        foreach (var review in reviews ?? [])
        {
            if (string.IsNullOrWhiteSpace(review.Title))
                continue;

            context.ProductExpertReviews.Add(new ProductExpertReview
            {
                Id = Guid.NewGuid(),
                ProductId = product.ProductId,
                Title = review.Title.Trim(),
                Description = review.Description?.Trim() ?? string.Empty,
                SortOrder = sortOrder++,
            });
        }

        return Task.CompletedTask;
    }

    private string? TryDecodeApiProductImage(
        ProductApiCreateProductCommand command,
        out ProductApiImagePayload? image)
    {
        image = null;
        if (string.IsNullOrWhiteSpace(command.FeaturedImageBase64))
            return null;

        var encoded = command.FeaturedImageBase64.Trim();
        string? extension;
        string? contentType;

        if (encoded.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var commaIndex = encoded.IndexOf(',');
            if (commaIndex <= 5)
                return "ساختار Data URI تصویر معتبر نیست";

            var metadata = encoded[5..commaIndex]
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (metadata.Length < 2 ||
                !metadata[^1].Equals("base64", StringComparison.OrdinalIgnoreCase))
            {
                return "ساختار Data URI تصویر معتبر نیست";
            }

            contentType = metadata[0].ToLowerInvariant();
            extension = ExtensionFromImageContentType(contentType);
            encoded = encoded[(commaIndex + 1)..];
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.FeaturedImageFileName))
                return "برای تصویر Base64 خام، featuredImageFileName الزامی است";

            extension = Path.GetExtension(command.FeaturedImageFileName)
                .TrimStart('.')
                .ToLowerInvariant();
            contentType = ContentTypeFromImageExtension(extension);
        }

        if (extension == null || contentType == null)
            return "فرمت تصویر مجاز نیست؛ فرمت‌های مجاز jpg، jpeg، png و webp هستند";

        var compactBase64 = string.Concat(encoded.Where(c => !char.IsWhiteSpace(c)));
        var maxFileSize = Math.Max(1, _fileStorageSettings.MaxFileSizeBytes);
        var maxEncodedLength = 4L * ((maxFileSize + 2L) / 3L);
        if (compactBase64.Length == 0 || compactBase64.Length > maxEncodedLength)
            return "حجم تصویر بیش از حد مجاز است";

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(compactBase64);
        }
        catch (FormatException)
        {
            return "مقدار featuredImageBase64 معتبر نیست";
        }

        if (bytes.Length == 0)
            return "تصویر خالی است";
        if (bytes.LongLength > maxFileSize)
            return "حجم تصویر بیش از حد مجاز است";
        if (!HasExpectedImageSignature(bytes, extension))
            return "محتوای تصویر با فرمت اعلام‌شده مطابقت ندارد";

        image = new ProductApiImagePayload(
            bytes,
            $"featured-image.{extension}",
            contentType);
        return null;
    }

    private static string? ExtensionFromImageContentType(string contentType)
        => contentType switch
        {
            "image/jpeg" or "image/jpg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            _ => null,
        };

    private static string? ContentTypeFromImageExtension(string extension)
        => extension switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "webp" => "image/webp",
            _ => null,
        };

    private static bool HasExpectedImageSignature(byte[] bytes, string extension)
        => extension switch
        {
            "jpg" or "jpeg" => bytes.Length >= 3 &&
                                 bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            "png" => bytes.Length >= 8 &&
                     bytes.AsSpan(0, 8).SequenceEqual(
                         new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "webp" => bytes.Length >= 12 &&
                      bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                      bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false,
        };

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (DbUpdateException ex)
        {
            return OperationResult.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;
    private static int NormalizePageSize(int pageSize) => pageSize < 1 ? 20 : Math.Min(pageSize, 100);

    private static OperationResult<PagedResult<T>> SuccessPaged<T>(List<T> items, int totalCount, int page, int pageSize)
        => OperationResult<PagedResult<T>>.Success(new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });

    private static ProductCategoryDto MapCategory(ProductCategory category) => new()
    {
        ProductCategoryId = category.ProductCategoryId,
        Title = category.Title,
        AcceptsResume = category.AcceptsResume,
        ProductCategoryGroupId = category.ProductCategoryGroupId,
    };

    private async Task<ProductCategoryGroupDto> MapCategoryGroupAsync(int productCategoryGroupId)
    {
        return await context.ProductCategoryGroups.AsNoTracking()
            .Where(g => g.ProductCategoryGroupId == productCategoryGroupId)
            .Select(g => new ProductCategoryGroupDto
            {
                ProductCategoryGroupId = g.ProductCategoryGroupId,
                Name = g.Name,
                HomePageImagePath = g.HomePageImagePath,
                ShowOnHomePage = g.ShowOnHomePage,
                HasHomePageImage = g.HomePageImagePath != null && g.HomePageImagePath != string.Empty,
                CategoryCount = g.Categories.Count,
                Categories = g.Categories
                    .OrderBy(c => c.Title)
                    .Select(c => new ProductCategoryGroupItemDto
                    {
                        ProductCategoryId = c.ProductCategoryId,
                        Title = c.Title,
                    })
                    .ToList(),
            })
            .FirstAsync();
    }

    private async Task<OperationResult<List<ProductCategory>>> ValidateAndLoadAssignableCategoriesAsync(
        List<int> categoryIds,
        int? currentGroupId)
    {
        if (categoryIds.Count == 0)
            return OperationResult<List<ProductCategory>>.Success([]);

        var categories = await context.ProductCategories
            .Where(c => categoryIds.Contains(c.ProductCategoryId))
            .ToListAsync();

        if (categories.Count != categoryIds.Count)
            return OperationResult<List<ProductCategory>>.Failure("یک یا چند دسته یافت نشد");

        var conflicted = categories
            .Where(c => c.ProductCategoryGroupId.HasValue
                        && (!currentGroupId.HasValue || c.ProductCategoryGroupId != currentGroupId.Value))
            .ToList();

        if (conflicted.Count > 0)
        {
            var titles = string.Join("، ", conflicted.Select(c => c.Title));
            return OperationResult<List<ProductCategory>>.Failure(
                $"دسته‌های زیر قبلاً به سرگروه دیگری اختصاص یافته‌اند: {titles}");
        }

        return OperationResult<List<ProductCategory>>.Success(categories);
    }

    private static BrandDto MapBrand(Brand brand) => new()
    {
        BrandId = brand.BrandId,
        Title = brand.Title,
        ImagePath = brand.ImagePath,
        HasImage = !string.IsNullOrWhiteSpace(brand.ImagePath),
        ShowOnHomePage = brand.ShowOnHomePage,
    };

    private sealed record ProductApiImagePayload(byte[] Bytes, string FileName, string ContentType);
}
