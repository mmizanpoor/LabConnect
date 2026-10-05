using System.Linq.Expressions;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class LabAgreementRepository : LabConnectRepository<LabAgreement>, ILabAgreementRepository
{
    private const int PrimaryParty = (int)AgreementPartyType.Primary;
    private const int ReceiverParty = (int)AgreementPartyType.Receiver;

    private readonly LabConnectDbContext _context;

    public LabAgreementRepository(LabConnectDbContext context) : base(context)
    {
        _context = context;
    }

    public bool IsExistLabAgreement(GetExistLabAgreementQuery query)
    {
        return _context.LabAgreements.AsNoTracking().Any(x =>
            x.PrimaryAgreementLabCodeNew == query.PrimaryAgreementLabCodeNew &&
            x.ContractNumber == query.ContractNumber &&
            (query.Id.HasValue ? x.Id != query.Id : true));
    }

    public async Task<OperationResult<LabAgreementCommand?>> InsertLabAgreement(LabAgreementCommand command)
    {
        var agreement = _context.LabAgreements.SingleOrDefault(x =>
            x.PrimaryAgreementLabCodeNew == command.PrimaryAgreementLabCodeNew &&
            x.ContractNumber == command.ContractNumber &&
            !x.ParentId.HasValue);

        if (agreement == null)
        {
            agreement = new LabAgreement
            {
                ContractNumber = command.ContractNumber,
                ExpDate = command.ExpDate,
                StartDate = command.StartDate,
                PrimaryAgreementLabCodeNew = command.PrimaryAgreementLabCodeNew,
                LaboratoryAgreementState = command.LaboratoryAgreementState,
                ReceiverAgreementLabCodeNew = command.ReceiverAgreementLabCodeNew,
                Title = command.Title,
                Text = command.Text,
                Children = command.Children?.Select(x => new LabAgreement
                {
                    ContractNumber = x.ContractNumber,
                    ExpDate = x.ExpDate,
                    StartDate = x.StartDate,
                    PrimaryAgreementLabCodeNew = x.PrimaryAgreementLabCodeNew,
                    LaboratoryAgreementState = x.LaboratoryAgreementState,
                    ReceiverAgreementLabCodeNew = x.ReceiverAgreementLabCodeNew,
                    Title = x.Title,
                    Text = x.Text,
                    Attachments = x.Attachments?.Select((c, i) => new LabAgreementAttachment
                    {
                        FileName = c.FileName,
                        Remark = !string.IsNullOrWhiteSpace(c.Remark) ? c.Remark : $"attach{++i}",
                        ContentType = c.ContentType,
                    }).ToList(),
                }).ToList(),
                Attachments = command.Attachments?.Select((x, i) => new LabAgreementAttachment
                {
                    FileName = x.FileName,
                    Remark = !string.IsNullOrWhiteSpace(x.Remark) ? x.Remark : $"attach{++i}",
                    ContentType = x.ContentType,
                }).ToList(),
                TestPrices = command.TestPrices?.Select(x => new LabAgreementTestPrice
                {
                    Approved = x.Approved,
                    BaseTariffApproved = x.BaseTariffApproved,
                    CPNCode = x.CPNCode,
                    FirstAdditions = x.FirstAdditions,
                    NationalCode = x.NationalCode,
                    SecondAdditions = x.SecondAdditions,
                    TestId = x.TestId,
                    TestName = x.TestName,
                    UrgentAmount = x.UrgentAmount,
                }).ToList(),
            };

            _context.LabAgreements.Add(agreement);
            try
            {
                _context.SaveChanges();
                RecordSubmittedHistory(agreement, command.PrimaryActionUserName);
            }
            catch (Exception ex)
            {
                OperationResult.Failure(ex.Message);
            }
        }
        else if (command.IsAddendum.HasValue && command.IsAddendum.Value)
        {
            var parentId = agreement.Id;

            agreement = new LabAgreement
            {
                ContractNumber = command.ContractNumber,
                ExpDate = command.ExpDate,
                StartDate = command.StartDate,
                PrimaryAgreementLabCodeNew = command.PrimaryAgreementLabCodeNew,
                LaboratoryAgreementState = command.LaboratoryAgreementState,
                ReceiverAgreementLabCodeNew = command.ReceiverAgreementLabCodeNew,
                Title = command.Title,
                Text = command.Text,
                ParentId = parentId,
                Attachments = command.Attachments?.Select(x => new LabAgreementAttachment
                {
                    FileName = x.FileName,
                    Remark = x.Remark,
                    ContentType = x.ContentType,
                }).ToList(),
                TestPrices = command.TestPrices?.Select(x => new LabAgreementTestPrice
                {
                    Approved = x.Approved,
                    BaseTariffApproved = x.BaseTariffApproved,
                    CPNCode = x.CPNCode,
                    FirstAdditions = x.FirstAdditions,
                    NationalCode = x.NationalCode,
                    SecondAdditions = x.SecondAdditions,
                    TestId = x.TestId,
                    TestName = x.TestName,
                    UrgentAmount = x.UrgentAmount,
                }).ToList(),
            };

            _context.LabAgreements.Add(agreement);
            try
            {
                _context.SaveChanges();
                RecordSubmittedHistory(agreement, command.PrimaryActionUserName);
            }
            catch (Exception ex)
            {
                OperationResult.Failure(ex.Message);
            }
        }

        var item = await GetLabAgreementById(agreement.Id);
        return OperationResult<LabAgreementCommand?>.Success(item);
    }

    public async Task<LabAgreementCommand?> GetLabAgreementById(long id)
    {
        return await _context.LabAgreements
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.Id == id)
            .Select(ToLabAgreement())
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<LabAgreementTestPriceCommand>> GetMergedTestPricesAsync(long agreementId)
    {
        var parentTestPrices = await _context.LabAgreementTestPrices
            .AsNoTracking()
            .Where(tp => tp.LabAgreementId == agreementId)
            .Select(tp => new LabAgreementTestPriceCommand
            {
                Id = tp.Id,
                Approved = tp.Approved,
                BaseTariffApproved = tp.BaseTariffApproved,
                CPNCode = tp.CPNCode,
                FirstAdditions = tp.FirstAdditions,
                NationalCode = tp.NationalCode,
                SecondAdditions = tp.SecondAdditions,
                TestId = tp.TestId,
                TestName = tp.TestName,
                UrgentAmount = tp.UrgentAmount,
            })
            .ToListAsync();

        var childrenWithTestPrices = await _context.LabAgreements
            .AsNoTracking()
            .Where(a => a.ParentId == agreementId && a.TestPrices!.Any())
            .Select(a => new LabAgreementCommand
            {
                Id = a.Id,
                Title = a.Title,
                StartDate = a.StartDate,
                TestPrices = a.TestPrices!.Select(tp => new LabAgreementTestPriceCommand
                {
                    Id = tp.Id,
                    Approved = tp.Approved,
                    BaseTariffApproved = tp.BaseTariffApproved,
                    CPNCode = tp.CPNCode,
                    FirstAdditions = tp.FirstAdditions,
                    NationalCode = tp.NationalCode,
                    SecondAdditions = tp.SecondAdditions,
                    TestId = tp.TestId,
                    TestName = tp.TestName,
                    UrgentAmount = tp.UrgentAmount,
                }).ToList(),
            })
            .ToListAsync();

        return LabAgreementTestPriceMergeHelper.Merge(parentTestPrices, childrenWithTestPrices);
    }

    public async Task<LabAgreementCommand?> GetLabAgreement(long id)
    {
        return await _context.LabAgreements
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.Id == id)
            .Select(ToLabAgreementDetail())
            .FirstOrDefaultAsync();
    }

    public IReadOnlyList<LabAgreementCommand> GetLabAgreements(GetLabAgreementQuery query)
    {
        var dbQuery = _context.LabAgreements.Include(x => x.Children).AsQueryable();
        dbQuery = dbQuery.Where(x => x.StartDate.Date >= query.StartDateTime.Date && !x.ParentId.HasValue);

        if (query.AgreementDirection == AgreementDirection.Sent)
            dbQuery = dbQuery.Where(x => x.PrimaryAgreementLabCodeNew == query.PrimaryLabCodeNew);
        else
            dbQuery = dbQuery.Where(x => x.ReceiverAgreementLabCodeNew == query.PrimaryLabCodeNew);

        var statesToMatch = query.LaboratoryAgreementStates?.ToArray() ?? [];

        if (query.LaboratoryAgreementStates != null && query.LaboratoryAgreementStates.Count > 0)
        {
            dbQuery = dbQuery.Where(x => statesToMatch.Any(c => c == x.LaboratoryAgreementState));
        }

        return dbQuery.Select(ToLabAgreementForList()).ToList();
    }

    public IReadOnlyList<LabAgreementCommand> GetAllLabAgreements(GetAllLabAgreementQuery query)
    {
        var dbQuery = _context.LabAgreements.Include(x => x.Children).AsQueryable();

        dbQuery = dbQuery.Where(x => x.PrimaryAgreementLabCodeNew == query.PrimaryLabCodeNew && !x.ParentId.HasValue);

        return dbQuery.Select(ToAllLabAgreement()).ToList();
    }

    public void ReceiverActionSeen(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id)
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Receiver,
            PartyActionType.Seen,
            command.ReceiverActionUserName);
    }

    public void ReceiverReject(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id && string.IsNullOrEmpty(x.PrimaryAgreementSign))
            .ExecuteUpdate(x => x
                .SetProperty(c => c.ReceiverAgreementSign, (string?)null)
                .SetProperty(c => c.ReceiverAgreementSignDateTime, (DateTime?)null)
                .SetProperty(c => c.ReceiverAgreementUserName, (string?)null)
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Receiver,
            PartyActionType.Rejected,
            command.ReceiverActionUserName,
            reason: command.ReceiverReturnCause);
    }

    public void PrimaryCanceledSuspendAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id)
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Primary,
            PartyActionType.CanceledSuspend,
            command.PrimaryActionUserName);
    }

    public void ReceiverCanceledSuspendAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id)
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Receiver,
            PartyActionType.CanceledSuspend,
            command.ReceiverActionUserName);
    }

    public void PrimaryCanceledTerminationAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id)
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Primary,
            PartyActionType.CanceledTermination,
            command.PrimaryActionUserName);
    }

    public void ReceiverCanceledTerminationAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id)
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Receiver,
            PartyActionType.CanceledTermination,
            command.ReceiverActionUserName);
    }

    public void PrimarySuspendAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Primary,
            PartyActionType.Suspended,
            command.PrimaryActionUserName,
            reason: command.PrimaryReturnCause);
    }

    public void ReceiverSuspendAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Receiver,
            PartyActionType.Suspended,
            command.ReceiverActionUserName,
            reason: command.ReceiverReturnCause);
    }

    public void PrimaryTerminationAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Primary,
            PartyActionType.Terminated,
            command.PrimaryActionUserName,
            reason: command.PrimaryReturnCause);
    }

    public void ReceiverTerminationAgreement(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
            .ExecuteUpdate(x => x
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Receiver,
            PartyActionType.Terminated,
            command.ReceiverActionUserName,
            reason: command.ReceiverReturnCause);
    }

    public void ReceiverSign(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id && string.IsNullOrEmpty(x.ReceiverAgreementSign))
            .ExecuteUpdate(x => x
                .SetProperty(c => c.ReceiverAgreementSign, command.ReceiverAgreementSign)
                .SetProperty(c => c.ReceiverAgreementSignDateTime, DateTime.Now)
                .SetProperty(c => c.ReceiverAgreementUserName, command.ReceiverAgreementUsername)
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Receiver,
            PartyActionType.Signed,
            command.ReceiverActionUserName);
    }

    public void PrimarySign(LabAgreementCommand command)
    {
        _context.LabAgreements
            .Where(x => x.Id == command.Id && string.IsNullOrEmpty(x.PrimaryAgreementSign))
            .ExecuteUpdate(x => x
                .SetProperty(c => c.PrimaryAgreementSign, command.PrimaryAgreementSign)
                .SetProperty(c => c.PrimaryAgreementSignDateTime, DateTime.Now)
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));

        AppendStatusHistory(
            command.Id!.Value,
            AgreementPartyType.Primary,
            PartyActionType.Signed,
            command.PrimaryActionUserName);
    }

    private void AppendStatusHistory(
        long labAgreementId,
        AgreementPartyType party,
        PartyActionType action,
        string? actionUserName,
        DateTime? actionDateTime = null,
        string? reason = null)
    {
        _context.LabAgreementStatusHistories.Add(new LabAgreementStatusHistory
        {
            LabAgreementId = labAgreementId,
            UserType = (int)party,
            PartyActionType = (int)action,
            ActionUserName = actionUserName ?? string.Empty,
            ActionDateTime = actionDateTime ?? DateTime.Now,
            Reason = reason,
        });
    }

    private void RecordSubmittedHistory(LabAgreement agreement, string? actionUserName)
    {
        AppendStatusHistory(agreement.Id, AgreementPartyType.Primary, PartyActionType.Submitted, actionUserName);

        if (agreement.Children == null)
            return;

        foreach (var child in agreement.Children)
            AppendStatusHistory(child.Id, AgreementPartyType.Primary, PartyActionType.Submitted, actionUserName);
    }

    private static Expression<Func<LabAgreement, LabAgreementCommand>> ToLabAgreement()
    {
        return model => new LabAgreementCommand
        {
            Id = model.Id,
            ContractNumber = model.ContractNumber,
            ExpDate = model.ExpDate,
            LaboratoryAgreementState = model.LaboratoryAgreementState,
            PrimaryAgreementLabCodeNew = model.PrimaryAgreementLabCodeNew,
            PrimaryAgreementSign = model.PrimaryAgreementSign,
            PrimaryAgreementSignDateTime = model.PrimaryAgreementSignDateTime,
            ReceiverAgreementLabCodeNew = model.ReceiverAgreementLabCodeNew,
            ReceiverAgreementSign = model.ReceiverAgreementSign,
            ReceiverAgreementSignDateTime = model.ReceiverAgreementSignDateTime,
            ReceiverAgreementUsername = model.ReceiverAgreementUserName,
            PrimaryReturnCause = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            ReceiverReturnCause = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            StartDate = model.StartDate,
            Text = model.Text,
            Title = model.Title,
            GetRecept = model.GetRecept,
            GetSampling = model.GetSampling,
            ParentId = model.ParentId,
            PrimaryAction = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            ReceiverAction = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            PrimaryActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            ReceiverActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            PrimaryActionUserName = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ReceiverActionUserName = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ChildrenCount = model.Children != null ? model.Children.Count : 0,
            AttachmentCount = model.Attachments != null ? model.Attachments.Count : 0,
            TestPriceCount = model.TestPrices != null ? model.TestPrices.Count : 0,
            RefrenceTestIds = model.TestPrices != null ? model.TestPrices.Select(tp => tp.TestId).ToList() : null,
            ChildrenTestIds = model.Children != null
                ? model.Children
                    .Where(c => c.LaboratoryAgreementState > 0 && c.TestPrices != null)
                    .SelectMany(c => c.TestPrices)
                    .Select(tp => tp.TestId)
                    .ToList()
                : null,
            Children = model.Children != null ? model.Children.Where(x => x.LaboratoryAgreementState > 0).Select(x => new LabAgreementCommand
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                ExpDate = x.ExpDate,
                LaboratoryAgreementState = x.LaboratoryAgreementState,
                PrimaryAgreementLabCodeNew = x.PrimaryAgreementLabCodeNew,
                PrimaryAgreementSign = x.PrimaryAgreementSign,
                PrimaryAgreementSignDateTime = x.PrimaryAgreementSignDateTime,
                ReceiverAgreementLabCodeNew = x.ReceiverAgreementLabCodeNew,
                ReceiverAgreementSign = x.ReceiverAgreementSign,
                ReceiverAgreementSignDateTime = x.ReceiverAgreementSignDateTime,
                ReceiverAgreementUsername = x.ReceiverAgreementUserName,
                PrimaryReturnCause = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.Reason)
                    .FirstOrDefault(),
                ReceiverReturnCause = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.Reason)
                    .FirstOrDefault(),
                StartDate = x.StartDate,
                Text = x.Text,
                Title = x.Title,
                ParentId = x.ParentId,
                GetRecept = x.GetRecept,
                GetSampling = x.GetSampling,
                PrimaryAction = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (int?)h.PartyActionType)
                    .FirstOrDefault(),
                ReceiverAction = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (int?)h.PartyActionType)
                    .FirstOrDefault(),
                PrimaryActionDateTime = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (DateTime?)h.ActionDateTime)
                    .FirstOrDefault(),
                ReceiverActionDateTime = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (DateTime?)h.ActionDateTime)
                    .FirstOrDefault(),
                PrimaryActionUserName = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.ActionUserName)
                    .FirstOrDefault(),
                ReceiverActionUserName = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.ActionUserName)
                    .FirstOrDefault(),
                ChildrenCount = x.Children != null ? x.Children.Count : 0,
                AttachmentCount = x.Attachments != null ? x.Attachments.Count : 0,
                TestPriceCount = x.TestPrices != null ? x.TestPrices.Count : 0,
                RefrenceTestIds = x.TestPrices != null ? x.TestPrices.Select(tp => tp.TestId).ToList() : null,
                Attachments = x.Attachments != null ? x.Attachments.Select(c => new LabAgreementAttachmentCommand
                {
                    Id = c.Id,
                    FileName = c.FileName,
                    Remark = c.Remark,
                    ContentType = c.ContentType,
                }).ToList() : null,
                TestPrices = x.TestPrices != null ? x.TestPrices.Select(c => new LabAgreementTestPriceCommand
                {
                    Id = c.Id,
                    Approved = c.Approved,
                    BaseTariffApproved = c.BaseTariffApproved,
                    CPNCode = c.CPNCode,
                    FirstAdditions = c.FirstAdditions,
                    NationalCode = c.NationalCode,
                    SecondAdditions = c.SecondAdditions,
                    TestId = c.TestId,
                    TestName = c.TestName,
                    UrgentAmount = c.UrgentAmount,
                }).ToList() : null,
            }).ToList() : null,
            Attachments = model.Attachments != null ? model.Attachments.Select(x => new LabAgreementAttachmentCommand
            {
                Id = x.Id,
                FileName = x.FileName,
                Remark = x.Remark,
                ContentType = x.ContentType,
            }).ToList() : null,
            TestPrices = model.TestPrices != null ? model.TestPrices.Select(x => new LabAgreementTestPriceCommand
            {
                Id = x.Id,
                Approved = x.Approved,
                BaseTariffApproved = x.BaseTariffApproved,
                CPNCode = x.CPNCode,
                FirstAdditions = x.FirstAdditions,
                NationalCode = x.NationalCode,
                SecondAdditions = x.SecondAdditions,
                TestId = x.TestId,
                TestName = x.TestName,
                UrgentAmount = x.UrgentAmount,
            }).ToList() : null,
        };
    }

    private static Expression<Func<LabAgreement, LabAgreementCommand>> ToLabAgreementDetail()
    {
        return model => new LabAgreementCommand
        {
            Id = model.Id,
            ContractNumber = model.ContractNumber,
            ExpDate = model.ExpDate,
            LaboratoryAgreementState = model.LaboratoryAgreementState,
            PrimaryAgreementLabCodeNew = model.PrimaryAgreementLabCodeNew,
            PrimaryAgreementSign = model.PrimaryAgreementSign,
            PrimaryAgreementSignDateTime = model.PrimaryAgreementSignDateTime,
            ReceiverAgreementLabCodeNew = model.ReceiverAgreementLabCodeNew,
            ReceiverAgreementSign = model.ReceiverAgreementSign,
            ReceiverAgreementSignDateTime = model.ReceiverAgreementSignDateTime,
            ReceiverAgreementUsername = model.ReceiverAgreementUserName,
            PrimaryReturnCause = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            ReceiverReturnCause = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            StartDate = model.StartDate,
            Text = model.Text,
            Title = model.Title,
            GetRecept = model.GetRecept,
            GetSampling = model.GetSampling,
            PrimaryAction = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            ReceiverAction = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            PrimaryActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            ReceiverActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            PrimaryActionUserName = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ReceiverActionUserName = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ChildrenCount = model.Children != null ? model.Children.Count : 0,
            AttachmentCount = model.Attachments != null ? model.Attachments.Count : 0,
            TestPriceCount = model.TestPrices != null ? model.TestPrices.Count : 0,
            RefrenceTestIds = model.TestPrices != null ? model.TestPrices.Select(tp => tp.TestId).ToList() : null,
            ChildrenTestIds = model.Children != null
                ? model.Children
                    .Where(c => c.TestPrices != null)
                    .SelectMany(c => c.TestPrices)
                    .Select(tp => tp.TestId)
                    .ToList()
                : null,
            Children = model.Children != null ? model.Children.Select(x => new LabAgreementCommand
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                ExpDate = x.ExpDate,
                LaboratoryAgreementState = x.LaboratoryAgreementState,
                PrimaryAgreementLabCodeNew = x.PrimaryAgreementLabCodeNew,
                PrimaryAgreementSign = x.PrimaryAgreementSign,
                PrimaryAgreementSignDateTime = x.PrimaryAgreementSignDateTime,
                ReceiverAgreementLabCodeNew = x.ReceiverAgreementLabCodeNew,
                ReceiverAgreementSign = x.ReceiverAgreementSign,
                ReceiverAgreementSignDateTime = x.ReceiverAgreementSignDateTime,
                ReceiverAgreementUsername = x.ReceiverAgreementUserName,
                PrimaryReturnCause = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.Reason)
                    .FirstOrDefault(),
                ReceiverReturnCause = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.Reason)
                    .FirstOrDefault(),
                StartDate = x.StartDate,
                Text = x.Text,
                Title = x.Title,
                ParentId = x.ParentId,
                GetRecept = x.GetRecept,
                GetSampling = x.GetSampling,
                PrimaryAction = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (int?)h.PartyActionType)
                    .FirstOrDefault(),
                ReceiverAction = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (int?)h.PartyActionType)
                    .FirstOrDefault(),
                PrimaryActionDateTime = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (DateTime?)h.ActionDateTime)
                    .FirstOrDefault(),
                ReceiverActionDateTime = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => (DateTime?)h.ActionDateTime)
                    .FirstOrDefault(),
                PrimaryActionUserName = x.StatusHistories!
                    .Where(h => h.UserType == PrimaryParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.ActionUserName)
                    .FirstOrDefault(),
                ReceiverActionUserName = x.StatusHistories!
                    .Where(h => h.UserType == ReceiverParty)
                    .OrderByDescending(h => h.ActionDateTime)
                    .Select(h => h.ActionUserName)
                    .FirstOrDefault(),
                ChildrenCount = x.Children != null ? x.Children.Count : 0,
                AttachmentCount = x.Attachments != null ? x.Attachments.Count : 0,
                TestPriceCount = x.TestPrices != null ? x.TestPrices.Count : 0,
                RefrenceTestIds = x.TestPrices != null ? x.TestPrices.Select(tp => tp.TestId).ToList() : null,
                Attachments = x.Attachments != null ? x.Attachments.Select(c => new LabAgreementAttachmentCommand
                {
                    Id = c.Id,
                    FileName = c.FileName,
                    Remark = c.Remark,
                    ContentType = c.ContentType,
                }).ToList() : null,
                TestPrices = x.TestPrices != null ? x.TestPrices.Select(c => new LabAgreementTestPriceCommand
                {
                    Id = c.Id,
                    Approved = c.Approved,
                    BaseTariffApproved = c.BaseTariffApproved,
                    CPNCode = c.CPNCode,
                    FirstAdditions = c.FirstAdditions,
                    NationalCode = c.NationalCode,
                    SecondAdditions = c.SecondAdditions,
                    TestId = c.TestId,
                    TestName = c.TestName,
                    UrgentAmount = c.UrgentAmount,
                }).ToList() : null,
            }).ToList() : null,
            Attachments = model.Attachments != null ? model.Attachments.Select(x => new LabAgreementAttachmentCommand
            {
                Id = x.Id,
                FileName = x.FileName,
                Remark = x.Remark,
                ContentType = x.ContentType,
            }).ToList() : null,
            TestPrices = model.TestPrices != null ? model.TestPrices.Select(x => new LabAgreementTestPriceCommand
            {
                Id = x.Id,
                Approved = x.Approved,
                BaseTariffApproved = x.BaseTariffApproved,
                CPNCode = x.CPNCode,
                FirstAdditions = x.FirstAdditions,
                NationalCode = x.NationalCode,
                SecondAdditions = x.SecondAdditions,
                TestId = x.TestId,
                TestName = x.TestName,
                UrgentAmount = x.UrgentAmount,
            }).ToList() : null,
        };
    }

    private static Expression<Func<LabAgreement, LabAgreementCommand>> ToAllLabAgreement()
    {
        return model => new LabAgreementCommand
        {
            Id = model.Id,
            ContractNumber = model.ContractNumber,
            ExpDate = model.ExpDate,
            LaboratoryAgreementState = model.LaboratoryAgreementState,
            PrimaryAgreementLabCodeNew = model.PrimaryAgreementLabCodeNew,
            //PrimaryAgreementSign = model.PrimaryAgreementSign,
            PrimaryAgreementSignDateTime = model.PrimaryAgreementSignDateTime,
            ReceiverAgreementLabCodeNew = model.ReceiverAgreementLabCodeNew,
            //ReceiverAgreementSign = model.ReceiverAgreementSign,
            ReceiverAgreementSignDateTime = model.ReceiverAgreementSignDateTime,
            ReceiverAgreementUsername = model.ReceiverAgreementUserName,
            PrimaryReturnCause = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            ReceiverReturnCause = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            StartDate = model.StartDate,
            //Text = model.Text,
            Title = model.Title,
            GetRecept = model.GetRecept,
            GetSampling = model.GetSampling,
            PrimaryAction = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            ReceiverAction = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            PrimaryActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            ReceiverActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            PrimaryActionUserName = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ReceiverActionUserName = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ChildrenCount = model.Children != null ? model.Children.Count() : 0,
            AttachmentCount = model.Attachments != null ? model.Attachments.Count : 0,
            TestPriceCount = model.TestPrices != null ? model.TestPrices.Count : 0,
            RefrenceTestIds = model.TestPrices != null ? model.TestPrices.Select(tp => tp.TestId).ToList() : null,
            ChildrenTestIds = model.Children != null
                ? model.Children
                    .Where(c => c.TestPrices != null)
                    .SelectMany(c => c.TestPrices)
                    .Select(tp => tp.TestId)
                    .ToList()
                : null,
        };
    }

    private static Expression<Func<LabAgreement, LabAgreementCommand>> ToLabAgreementForList()
    {
        return model => new LabAgreementCommand
        {
            Id = model.Id,
            ContractNumber = model.ContractNumber,
            ExpDate = model.ExpDate,
            LaboratoryAgreementState = model.LaboratoryAgreementState,
            PrimaryAgreementLabCodeNew = model.PrimaryAgreementLabCodeNew,
            PrimaryAgreementSign = model.PrimaryAgreementSign,
            PrimaryAgreementSignDateTime = model.PrimaryAgreementSignDateTime,
            ReceiverAgreementLabCodeNew = model.ReceiverAgreementLabCodeNew,
            ReceiverAgreementSign = model.ReceiverAgreementSign,
            ReceiverAgreementSignDateTime = model.ReceiverAgreementSignDateTime,
            ReceiverAgreementUsername = model.ReceiverAgreementUserName,
            PrimaryReturnCause = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            ReceiverReturnCause = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            StartDate = model.StartDate,
            Text = model.Text,
            Title = model.Title,
            GetRecept = model.GetRecept,
            GetSampling = model.GetSampling,
            PrimaryAction = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            ReceiverAction = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            PrimaryActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            ReceiverActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            PrimaryActionUserName = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ReceiverActionUserName = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ChildrenCount = model.Children != null ? model.Children.Where(x => x.LaboratoryAgreementState > 0).Count() : 0,
            AttachmentCount = model.Attachments != null ? model.Attachments.Count : 0,
            TestPriceCount = model.TestPrices != null ? model.TestPrices.Count : 0,
            RefrenceTestIds = model.TestPrices != null ? model.TestPrices.Select(tp => tp.TestId).ToList() : null,
            ChildrenTestIds = model.Children != null
                ? model.Children
                    .Where(c => c.LaboratoryAgreementState > 0 && c.TestPrices != null)
                    .SelectMany(c => c.TestPrices)
                    .Select(tp => tp.TestId)
                    .ToList()
                : null,
        };
    }

    private static Expression<Func<LabAgreement, LabAgreementCommand>> ToLabAgreementMinimal()
    {
        return model => new LabAgreementCommand
        {
            Id = model.Id,
            ContractNumber = model.ContractNumber,
            ExpDate = model.ExpDate,
            LaboratoryAgreementState = model.LaboratoryAgreementState,
            PrimaryAgreementLabCodeNew = model.PrimaryAgreementLabCodeNew,
            PrimaryAgreementSignDateTime = model.PrimaryAgreementSignDateTime,
            ReceiverAgreementLabCodeNew = model.ReceiverAgreementLabCodeNew,
            ReceiverAgreementSignDateTime = model.ReceiverAgreementSignDateTime,
            ReceiverAgreementUsername = model.ReceiverAgreementUserName,
            PrimaryReturnCause = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            ReceiverReturnCause = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.Reason)
                .FirstOrDefault(),
            StartDate = model.StartDate,
            Title = model.Title,
            GetRecept = model.GetRecept,
            GetSampling = model.GetSampling,
            PrimaryAction = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            ReceiverAction = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (int?)h.PartyActionType)
                .FirstOrDefault(),
            PrimaryActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            ReceiverActionDateTime = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => (DateTime?)h.ActionDateTime)
                .FirstOrDefault(),
            PrimaryActionUserName = model.StatusHistories!
                .Where(h => h.UserType == PrimaryParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
            ReceiverActionUserName = model.StatusHistories!
                .Where(h => h.UserType == ReceiverParty)
                .OrderByDescending(h => h.ActionDateTime)
                .Select(h => h.ActionUserName)
                .FirstOrDefault(),
        };
    }

    public void AddTestPrices(LabAgreementCommand command)
    {
        var labAgreement = _context.LabAgreements.FirstOrDefault(x =>
            x.PrimaryAgreementLabCodeNew == command.PrimaryAgreementLabCodeNew && x.Id == command.Id);

        if (labAgreement == null)
            return;

        labAgreement.GetSampling = command.GetSampling;
        labAgreement.GetRecept = command.GetRecept;
        _context.LabAgreements.Update(labAgreement);

        if (labAgreement.TestPrices != null)
            _context.LabAgreementTestPrices.RemoveRange(labAgreement.TestPrices);

        var newTestPrices = command.TestPrices?.Select(x => new LabAgreementTestPrice
        {
            LabAgreementId = labAgreement.Id,
            Approved = x.Approved,
            BaseTariffApproved = x.BaseTariffApproved,
            CPNCode = x.CPNCode,
            FirstAdditions = x.FirstAdditions,
            NationalCode = x.NationalCode,
            SecondAdditions = x.SecondAdditions,
            TestId = x.TestId,
            TestName = x.TestName,
            UrgentAmount = x.UrgentAmount,
        }).ToList();

        if (newTestPrices != null)
            _context.LabAgreementTestPrices.AddRange(newTestPrices);
    }

    public bool UpdateLabAgreement(LabAgreementCommand command)
    {
        var labAgreement = _context.LabAgreements
            .Include(x => x.Attachments)
            .Include(x => x.TestPrices)
            .FirstOrDefault(x => x.Id == command.Id);

        if (labAgreement == null)
            return false;

        labAgreement.ContractNumber = command.ContractNumber;
        labAgreement.ExpDate = command.ExpDate;
        labAgreement.StartDate = command.StartDate;
        labAgreement.LaboratoryAgreementState = command.LaboratoryAgreementState;
        labAgreement.Title = command.Title;
        labAgreement.Text = command.Text;
        labAgreement.GetSampling = command.GetSampling;
        labAgreement.GetRecept = command.GetRecept;

        if (command.Attachments != null)
        {
            if (labAgreement.Attachments != null)
                _context.LabAgreementAttachments.RemoveRange(labAgreement.Attachments);

            labAgreement.Attachments = command.Attachments.Select((x, i) => new LabAgreementAttachment
            {
                FileName = x.FileName,
                Remark = !string.IsNullOrWhiteSpace(x.Remark) ? x.Remark : $"attach{++i}",
                ContentType = x.ContentType,
            }).ToList();
        }

        if (command.TestPrices != null)
        {
            if (labAgreement.TestPrices != null)
                _context.LabAgreementTestPrices.RemoveRange(labAgreement.TestPrices);

            labAgreement.TestPrices = command.TestPrices.Select(x => new LabAgreementTestPrice
            {
                LabAgreementId = labAgreement.Id,
                Approved = x.Approved,
                BaseTariffApproved = x.BaseTariffApproved,
                CPNCode = x.CPNCode,
                FirstAdditions = x.FirstAdditions,
                NationalCode = x.NationalCode,
                SecondAdditions = x.SecondAdditions,
                TestId = x.TestId,
                TestName = x.TestName,
                UrgentAmount = x.UrgentAmount,
            }).ToList();
        }

        _context.LabAgreements.Update(labAgreement);
        return true;
    }

    public bool UpdateLaboratoryAgreementState(long id, int laboratoryAgreementState)
    {
        var affected = _context.LabAgreements
            .Where(x => x.Id == id)
            .ExecuteUpdate(x => x.SetProperty(c => c.LaboratoryAgreementState, laboratoryAgreementState));

        return affected > 0;
    }

    public Task<LabAgreementStatsDto> GetGlobalStatsAsync()
    {
        var today = DateTime.Now.Date;
        var query = _context.LabAgreements
            .AsNoTracking()
            .Where(x => !x.ParentId.HasValue);

        return ComputeStatsAsync(query, today);
    }

    public async Task<LabAgreementLabStatsDto> GetStatsByLabCodeAsync(int labCodeNew)
    {
        var today = DateTime.Now.Date;
        var rootQuery = _context.LabAgreements
            .AsNoTracking()
            .Where(x => !x.ParentId.HasValue);

        var sentQuery = rootQuery.Where(x => x.PrimaryAgreementLabCodeNew == labCodeNew);
        var receivedQuery = rootQuery.Where(x => x.ReceiverAgreementLabCodeNew == labCodeNew);

        return new LabAgreementLabStatsDto
        {
            Sent = await ComputeStatsAsync(sentQuery, today),
            Received = await ComputeStatsAsync(receivedQuery, today),
        };
    }

    private static async Task<LabAgreementStatsDto> ComputeStatsAsync(IQueryable<LabAgreement> query, DateTime today)
    {
        return new LabAgreementStatsDto
        {
            ActiveCount = await query.CountAsync(x =>
                x.ExpDate.Date > today
                && !string.IsNullOrEmpty(x.PrimaryAgreementSign)
                && !string.IsNullOrEmpty(x.ReceiverAgreementSign)),
            ExpiredCount = await query.CountAsync(x => x.ExpDate.Date <= today),
            PendingCount = await query.CountAsync(x =>
                string.IsNullOrEmpty(x.PrimaryAgreementSign)
                || string.IsNullOrEmpty(x.ReceiverAgreementSign)),
        };
    }

    public async Task<LabAgreementCommand?> GetLabAgreementByLabCode(int primaryLabCode, int receiverLabCode)
    {
        var now = DateTime.Now;
        int[] stateList = [0, 1, 2, 4, 6, 11, 12, 13, 14, 15];
        return await _context.LabAgreements
            .AsNoTracking()
            .Where(x =>
                x.PrimaryAgreementLabCodeNew == primaryLabCode &&
                x.ReceiverAgreementLabCodeNew == receiverLabCode &&
                !x.ParentId.HasValue &&
                x.ExpDate.Date >= now.Date &&
                stateList.Any(s => s == x.LaboratoryAgreementState)
                )
            .Select(ToLabAgreementMinimal())
            .FirstOrDefaultAsync();
    }

    public async Task DeleteLabAgreementTreeAsync(long id)
    {
        var childIds = await _context.LabAgreements
            .Where(x => x.ParentId == id)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var childId in childIds)
            await DeleteLabAgreementTreeAsync(childId);

        var agreement = await _context.LabAgreements.FindAsync(id);
        if (agreement != null)
            _context.LabAgreements.Remove(agreement);
    }

    public async Task<ActiveContractLaboratoryResult> GetActiveContractLaboratory(GetActiveContractLaboratoryQuery query)
    {
        var now = DateTime.Now;
        int[] stateList = [0, 1, 2, 4, 6, 11, 12, 13, 14, 15];

        var labCodes = await _context.LabAgreements
            .AsNoTracking()
            .Where(x =>
                x.PrimaryAgreementLabCodeNew == query.LabCode &&
                !x.ParentId.HasValue &&
                x.ExpDate.Date >= now.Date &&
                stateList.Any(s => s == x.LaboratoryAgreementState)
                )
            .Select(x => x.ReceiverAgreementLabCodeNew)
            .ToListAsync() ?? [];

        return new ActiveContractLaboratoryResult(labCodes);
    }
}
