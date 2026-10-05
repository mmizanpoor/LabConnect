using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CompanyRegulation;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class CompanyRegulationService(LabConnectDbContext context) : ICompanyRegulationService
{
    public async Task<OperationResult<List<CompanyRegulationDto>>> GetAllAsync()
    {
        var items = await context.CompanyRegulations.AsNoTracking()
            .OrderBy(x => x.Type)
            .Select(x => new CompanyRegulationDto
            {
                CompanyRegulationId = x.CompanyRegulationId,
                Type = x.Type,
                Body = x.Body,
                CreatedAt = x.CreatedAt,
                ModifiedAt = x.ModifiedAt,
            })
            .ToListAsync();

        return OperationResult<List<CompanyRegulationDto>>.Success(items);
    }

    public async Task<OperationResult<CompanyRegulationDto>> GetByIdAsync(Guid companyRegulationId)
    {
        var entity = await context.CompanyRegulations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyRegulationId == companyRegulationId);

        if (entity == null)
            return OperationResult<CompanyRegulationDto>.Failure("رکورد یافت نشد");

        return OperationResult<CompanyRegulationDto>.Success(Map(entity));
    }

    public async Task<OperationResult<CompanyRegulationDto>> CreateAsync(SaveCompanyRegulationCommand command)
    {
        if (!Enum.IsDefined(typeof(CompanyRegulationType), command.Type))
            return OperationResult<CompanyRegulationDto>.Failure("نوع سند نامعتبر است");

        var exists = await context.CompanyRegulations.AnyAsync(x => x.Type == command.Type);
        if (exists)
            return OperationResult<CompanyRegulationDto>.Failure("برای این نوع سند قبلاً یک رکورد ثبت شده است");

        var now = DateTime.UtcNow.ToLocalTime();
        var entity = new CompanyRegulation
        {
            CompanyRegulationId = Guid.NewGuid(),
            Type = command.Type,
            Body = command.Body ?? string.Empty,
            CreatedAt = now,
            ModifiedAt = now,
        };

        context.CompanyRegulations.Add(entity);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return OperationResult<CompanyRegulationDto>.Failure("برای این نوع سند قبلاً یک رکورد ثبت شده است");
        }

        return OperationResult<CompanyRegulationDto>.Success(Map(entity));
    }

    public async Task<OperationResult<CompanyRegulationDto>> UpdateAsync(UpdateCompanyRegulationCommand command)
    {
        var entity = await context.CompanyRegulations
            .FirstOrDefaultAsync(x => x.CompanyRegulationId == command.CompanyRegulationId);

        if (entity == null)
            return OperationResult<CompanyRegulationDto>.Failure("رکورد یافت نشد");

        entity.Body = command.Body ?? string.Empty;
        entity.ModifiedAt = DateTime.UtcNow.ToLocalTime();

        await context.SaveChangesAsync();
        return OperationResult<CompanyRegulationDto>.Success(Map(entity));
    }

    public async Task<OperationResult> DeleteAsync(Guid companyRegulationId)
    {
        var entity = await context.CompanyRegulations
            .FirstOrDefaultAsync(x => x.CompanyRegulationId == companyRegulationId);

        if (entity == null)
            return OperationResult.Failure("رکورد یافت نشد");

        context.CompanyRegulations.Remove(entity);
        await context.SaveChangesAsync();
        return OperationResult.SuccessResult();
    }

    private static CompanyRegulationDto Map(CompanyRegulation entity) => new()
    {
        CompanyRegulationId = entity.CompanyRegulationId,
        Type = entity.Type,
        Body = entity.Body,
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt,
    };
}

public class PublicCompanyRegulationService(LabConnectDbContext context) : IPublicCompanyRegulationService
{
    public async Task<OperationResult<List<CompanyRegulationDto>>> GetAllAsync()
    {
        var items = await context.CompanyRegulations.AsNoTracking()
            .OrderBy(x => x.Type)
            .Select(x => new CompanyRegulationDto
            {
                CompanyRegulationId = x.CompanyRegulationId,
                Type = x.Type,
                Body = x.Body,
                CreatedAt = x.CreatedAt,
                ModifiedAt = x.ModifiedAt,
            })
            .ToListAsync();

        return OperationResult<List<CompanyRegulationDto>>.Success(items);
    }
}
