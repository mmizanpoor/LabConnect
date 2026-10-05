using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Domain;
using LabConnectPortal.Infra.Context;
using LabConnectPortal.Infra.Service.Interfaces;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Repositories
{
    public class LabAgreementRepository : Repository<LabAgreement>, ILabAgreement
    {
        private readonly SamanehDbContext _context;

        public LabAgreementRepository(SamanehDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<OperationResult<LabAgreementCommand?>> InsertLabAgreement(LabAgreementCommand command)
        {
            var agreement = _context.LabAgreements.SingleOrDefault(x => x.PrimaryAgreementLabCodeNew == command.PrimaryAgreementLabCodeNew && x.ContractNumber == command.ContractNumber && !x.ParentId.HasValue);
            if (agreement == null)
            {
                agreement = new LabAgreement()
                {
                    PrimaryAgreementId = command.PrimaryAgreementId,
                    ContractNumber = command.ContractNumber,
                    ExpDate = command.ExpDate,
                    StartDate = command.StartDate,
                    PrimaryAgreementLabCodeNew = command.PrimaryAgreementLabCodeNew,
                    LaboratoryAgreementState = command.LaboratoryAgreementState,
                    ReceiverAgreementLabCodeNew = command.ReceiverAgreementLabCodeNew,
                    Title = command.Title,
                    Text = command.Text,
                    PrimaryAction = command.PrimaryAction,
                    PrimaryActionUserName = command.PrimaryActionUserName,
                    PrimaryActionDateTime = DateTime.Now,
                    ReceiverAction = command.ReceiverAction,
                    Children = command.Children?.Select(x => new LabAgreement()
                    {
                        PrimaryAgreementId = x.PrimaryAgreementId,
                        ContractNumber = x.ContractNumber,
                        ExpDate = x.ExpDate,
                        StartDate = x.StartDate,
                        PrimaryAgreementLabCodeNew = x.PrimaryAgreementLabCodeNew,
                        LaboratoryAgreementState = x.LaboratoryAgreementState,
                        ReceiverAgreementLabCodeNew = x.ReceiverAgreementLabCodeNew,
                        Title = x.Title,
                        Text = x.Text,
                        PrimaryAction = command.PrimaryAction,
                        PrimaryActionUserName = command.PrimaryActionUserName,
                        PrimaryActionDateTime = DateTime.Now,
                        Attachments = x.Attachments?.Select((c, i) => new LabAgreementAttachment()
                        {
                            FileName = c.FileName,
                            Remark = !string.IsNullOrWhiteSpace(c.Remark) ? c.Remark : $"attach{++i}"
                        }).ToList()
                    }).ToList(),
                    Attachments = command.Attachments?.Select((x, i) => new LabAgreementAttachment()
                    {
                        FileName = x.FileName,
                        Remark = !string.IsNullOrWhiteSpace(x.Remark) ? x.Remark : $"attach{++i}"
                    }).ToList(),
                    TestPrices = command.TestPrices?.Select(x => new LabAgreementTestPrice()
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
                    }).ToList()
                };

                _context.LabAgreements.Add(agreement);
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    OperationResult.Failure(ex.Message);
                }
            }
            else
            {
                if (command.IsAddendum.HasValue && command.IsAddendum.Value)
                {
                    var parentId = agreement.Id;

                    agreement = new LabAgreement()
                    {
                        PrimaryAgreementId = command.PrimaryAgreementId,
                        ContractNumber = command.ContractNumber,
                        ExpDate = command.ExpDate,
                        StartDate = command.StartDate,
                        PrimaryAgreementLabCodeNew = command.PrimaryAgreementLabCodeNew,
                        LaboratoryAgreementState = command.LaboratoryAgreementState,
                        ReceiverAgreementLabCodeNew = command.ReceiverAgreementLabCodeNew,
                        Title = command.Title,
                        Text = command.Text,
                        ParentId = parentId,
                        Attachments = command.Attachments?.Select(x => new LabAgreementAttachment()
                        {
                            FileName = x.FileName,
                            Remark = x.Remark
                        }).ToList(),
                        TestPrices = command.TestPrices?.Select(x => new LabAgreementTestPrice()
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
                        }).ToList()
                    };

                    _context.LabAgreements.Add(agreement);
                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        OperationResult.Failure(ex.Message);
                    }
                }
            }

            var item = await GetLabAgreementById(agreement.Id);

            return OperationResult<LabAgreementCommand?>.Success(item);
        }

        public async Task<LabAgreementCommand?> GetLabAgreementById(long id)
        {
            var result = await _context.LabAgreements
                .AsNoTracking()
                .AsSplitQuery()
                .Where(x => x.Id == id)
                .Select(ToLabAgreement())
                .FirstOrDefaultAsync();

            return result;
        }

        public IReadOnlyList<LabAgreementCommand> GetLabAgreements(GetLabAgreementQuery query)
        {
            var dbQuery = _context.LabAgreements.Include(x => x.Children).AsQueryable();

            dbQuery = dbQuery.Where(x => x.StartDate.Date >= query.StartDateTime.Date && !x.ParentId.HasValue);

            if (!query.IsReceived)
                dbQuery = dbQuery.Where(x => x.PrimaryAgreementLabCodeNew == query.PrimaryLabCodeNew);
            else
                dbQuery = dbQuery.Where(x => x.ReceiverAgreementLabCodeNew == query.PrimaryLabCodeNew);

            if (query.LaboratoryAgreementStates != null && query.LaboratoryAgreementStates.Count > 0)
            {
                var statesToMatch = query.LaboratoryAgreementStates.ToArray();
                dbQuery = dbQuery.Where(x => statesToMatch.Any(c => c == x.LaboratoryAgreementState));
            }

            var result = dbQuery.Select(ToLabAgreementForList()).ToList();

            return result;
        }

        public void ReceiverActionSeen(LabAgreementCommand command)
        {
            _context.LabAgreements
                .Where(x => x.Id == command.Id)
                .ExecuteUpdate(x => x
                .SetProperty(c => c.ReceiverAction, (int)PartyActionType.Seen)
                .SetProperty(c => c.PrimaryAction, (int)PartyActionType.Seen)
                .SetProperty(c => c.ReceiverActionUserName, command.ReceiverActionUserName)
                .SetProperty(c => c.ReceiverActionDateTime, DateTime.Now)
                .SetProperty(c => c.PrimaryActionDateTime, DateTime.Now));
        }

        public void ReceiverReject(LabAgreementCommand command)
        {
            _context.LabAgreements
                .Where(x => x.Id == command.Id && string.IsNullOrEmpty(x.PrimaryAgreementSign))
                .ExecuteUpdate(x => x
                .SetProperty(c => c.ReceiverReturnCause, command.ReceiverReturnCause)
                .SetProperty(c => c.ReceiverAction, command.ReceiverAction)
                .SetProperty(c => c.ReceiverActionUserName, command.ReceiverActionUserName)
                .SetProperty(c => c.ReceiverAgreementSign, (string?)null)
                .SetProperty(c => c.ReceiverAgreementSignDateTime, (DateTime?)null)
                .SetProperty(c => c.ReceiverAgreementUserName, (string?)null)
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState));
        }

        public void PrimaryCanceledSuspendAgreement(LabAgreementCommand command)
        {
            _context.LabAgreements
               .Where(x => x.Id == command.Id)
               .ExecuteUpdate(x => x
               .SetProperty(c => c.PrimaryReturnCause, (string?)null)
               .SetProperty(c => c.PrimaryAction, command.PrimaryAction)
               .SetProperty(c => c.PrimaryActionDateTime, DateTime.Now)
               .SetProperty(c => c.PrimaryActionUserName, command.PrimaryActionUserName));
        }

        public void ReceiverCanceledSuspendAgreement(LabAgreementCommand command)
        {
            _context.LabAgreements
               .Where(x => x.Id == command.Id)
               .ExecuteUpdate(x => x
               .SetProperty(c => c.ReceiverReturnCause, (string?)null)
               .SetProperty(c => c.ReceiverAction, command.ReceiverAction)
               .SetProperty(c => c.ReceiverActionDateTime, DateTime.Now)
               .SetProperty(c => c.ReceiverActionUserName, command.ReceiverActionUserName));
        }

        public void PrimarySuspendAgreement(LabAgreementCommand command)
        {
            _context.LabAgreements
               .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
               .ExecuteUpdate(x => x
               .SetProperty(c => c.PrimaryReturnCause, command.PrimaryReturnCause)
               .SetProperty(c => c.PrimaryAction, command.PrimaryAction)
               .SetProperty(c => c.PrimaryActionDateTime, DateTime.Now)
               .SetProperty(c => c.PrimaryActionUserName, command.PrimaryActionUserName));
        }

        public void ReceiverSuspendAgreement(LabAgreementCommand command)
        {
            _context.LabAgreements
               .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
               .ExecuteUpdate(x => x
               .SetProperty(c => c.ReceiverReturnCause, command.ReceiverReturnCause)
               .SetProperty(c => c.ReceiverAction, command.ReceiverAction)
               .SetProperty(c => c.ReceiverActionDateTime, DateTime.Now)
               .SetProperty(c => c.ReceiverActionUserName, command.ReceiverActionUserName));
        }

        public void PrimaryTerminationAgreement(LabAgreementCommand command)
        {
            _context.LabAgreements
               .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
               .ExecuteUpdate(x => x
               .SetProperty(c => c.PrimaryReturnCause, command.PrimaryReturnCause)
               .SetProperty(c => c.PrimaryAction, command.PrimaryAction)
               .SetProperty(c => c.PrimaryActionDateTime, DateTime.Now)
               .SetProperty(c => c.PrimaryActionUserName, command.PrimaryActionUserName));
        }

        public void ReceiverTerminationAgreement(LabAgreementCommand command)
        {
            _context.LabAgreements
               .Where(x => x.Id == command.Id && !string.IsNullOrEmpty(x.PrimaryAgreementSign) && !string.IsNullOrEmpty(x.ReceiverAgreementSign))
               .ExecuteUpdate(x => x
               .SetProperty(c => c.ReceiverReturnCause, command.ReceiverReturnCause)
               .SetProperty(c => c.ReceiverAction, command.ReceiverAction)
               .SetProperty(c => c.ReceiverActionDateTime, DateTime.Now)
               .SetProperty(c => c.ReceiverActionUserName, command.ReceiverActionUserName));
        }

        public void ReceiverSign(LabAgreementCommand command)
        {
            _context.LabAgreements
                .Where(x => x.Id == command.Id && string.IsNullOrEmpty(x.ReceiverAgreementSign))
                .ExecuteUpdate(x => x
                .SetProperty(c => c.ReceiverAgreementSign, command.ReceiverAgreementSign)
                .SetProperty(c => c.ReceiverAgreementSignDateTime, DateTime.Now)
                .SetProperty(c => c.ReceiverAgreementUserName, command.ReceiverAgreementUsername)
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState)
                .SetProperty(c => c.ReceiverAction, command.ReceiverAction)
                .SetProperty(c => c.ReceiverActionDateTime, DateTime.Now)
                .SetProperty(c => c.ReceiverActionUserName, command.ReceiverActionUserName)
                .SetProperty(c => c.ReceiverReturnCause, string.Empty));
        }

        public void PrimarySign(LabAgreementCommand command)
        {
            _context.LabAgreements
                .Where(x => x.Id == command.Id && string.IsNullOrEmpty(x.PrimaryAgreementSign))
                .ExecuteUpdate(x => x
                .SetProperty(c => c.PrimaryAgreementSign, command.PrimaryAgreementSign)
                .SetProperty(c => c.PrimaryAgreementSignDateTime, DateTime.Now)
                .SetProperty(c => c.LaboratoryAgreementState, command.LaboratoryAgreementState)
                .SetProperty(c => c.PrimaryAction, command.PrimaryAction)
                .SetProperty(c => c.PrimaryActionDateTime, DateTime.Now)
                .SetProperty(c => c.PrimaryActionUserName, command.PrimaryActionUserName)
                .SetProperty(c => c.PrimaryReturnCause, string.Empty));
        }

        private static Expression<Func<LabAgreement, LabAgreementCommand>> ToLabAgreement()
        {
            return model => new LabAgreementCommand()
            {
                Id = model.Id,
                PrimaryAgreementId = model.PrimaryAgreementId,
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
                PrimaryReturnCause = model.PrimaryReturnCause,
                ReceiverReturnCause = model.ReceiverReturnCause,
                StartDate = model.StartDate,
                Text = model.Text,
                Title = model.Title,
                GetRecept = model.GetRecept,
                GetSampling = model.GetSampling,
                PrimaryAction = model.PrimaryAction,
                ReceiverAction = model.ReceiverAction,
                PrimaryActionDateTime = model.PrimaryActionDateTime,
                ReceiverActionDateTime = model.ReceiverActionDateTime,
                PrimaryActionUserName = model.PrimaryActionUserName,
                ReceiverActionUserName = model.ReceiverActionUserName,
                ChildrenCount = model.Children != null ? model.Children.Count : 0,
                AttachmentCount = model.Attachments != null ? model.Attachments.Count : 0,
                TestPriceCount = model.TestPrices != null ? model.TestPrices.Count : 0,
                Children = model.Children != null ? model.Children.Select(x => new LabAgreementCommand()
                {
                    Id = x.Id,
                    PrimaryAgreementId = x.PrimaryAgreementId,
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
                    PrimaryReturnCause = x.PrimaryReturnCause,
                    ReceiverReturnCause = x.ReceiverReturnCause,
                    StartDate = x.StartDate,
                    Text = x.Text,
                    Title = x.Title,
                    ParentId = x.ParentId,
                    GetRecept = x.GetRecept,
                    GetSampling = x.GetSampling,
                    PrimaryAction = x.PrimaryAction,
                    ReceiverAction = x.ReceiverAction,
                    PrimaryActionDateTime = x.PrimaryActionDateTime,
                    ReceiverActionDateTime = x.ReceiverActionDateTime,
                    PrimaryActionUserName = x.PrimaryActionUserName,
                    ReceiverActionUserName = x.ReceiverActionUserName,
                    Attachments = x.Attachments != null ? x.Attachments.Select(c => new LabAgreementAttachmentCommand()
                    {
                        Id = c.Id,
                        FileName = c.FileName,
                        Remark = c.Remark
                    }).ToList() : null,
                    TestPrices = x.TestPrices != null ? x.TestPrices.Select(c => new LabAgreementTestPriceCommand()
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
                        UrgentAmount = c.UrgentAmount
                    }).ToList() : null
                }).ToList() : null,
                Attachments = model.Attachments != null ? model.Attachments.Select(x => new LabAgreementAttachmentCommand()
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    Remark = x.Remark
                }).ToList() : null,
                TestPrices = model.TestPrices != null ? model.TestPrices.Select(x => new LabAgreementTestPriceCommand()
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
                    UrgentAmount = x.UrgentAmount
                }).ToList() : null
            };
        }

        private static Expression<Func<LabAgreement, LabAgreementCommand>> ToLabAgreementForList()
        {
            return model => new LabAgreementCommand()
            {
                Id = model.Id,
                PrimaryAgreementId = model.PrimaryAgreementId,
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
                PrimaryReturnCause = model.PrimaryReturnCause,
                ReceiverReturnCause = model.ReceiverReturnCause,
                StartDate = model.StartDate,
                Text = model.Text,
                Title = model.Title,
                GetRecept = model.GetRecept,
                GetSampling = model.GetSampling,
                PrimaryAction = model.PrimaryAction,
                ReceiverAction = model.ReceiverAction,
                PrimaryActionDateTime = model.PrimaryActionDateTime,
                ReceiverActionDateTime = model.ReceiverActionDateTime,
                PrimaryActionUserName = model.PrimaryActionUserName,
                ReceiverActionUserName = model.ReceiverActionUserName,
                ChildrenCount = model.Children != null ? model.Children.Count : 0,
                AttachmentCount = model.Attachments != null ? model.Attachments.Count : 0,
                TestPriceCount = model.TestPrices != null ? model.TestPrices.Count : 0,
            };
        }

        private static Expression<Func<LabAgreement, LabAgreementCommand>> ToLabAgreementMinimal()
        {
            return model => new LabAgreementCommand()
            {
                Id = model.Id,
                PrimaryAgreementId = model.PrimaryAgreementId,
                ContractNumber = model.ContractNumber,
                ExpDate = model.ExpDate,
                LaboratoryAgreementState = model.LaboratoryAgreementState,
                PrimaryAgreementLabCodeNew = model.PrimaryAgreementLabCodeNew,
                PrimaryAgreementSignDateTime = model.PrimaryAgreementSignDateTime,
                ReceiverAgreementLabCodeNew = model.ReceiverAgreementLabCodeNew,
                ReceiverAgreementSignDateTime = model.ReceiverAgreementSignDateTime,
                ReceiverAgreementUsername = model.ReceiverAgreementUserName,
                PrimaryReturnCause = model.PrimaryReturnCause,
                ReceiverReturnCause = model.ReceiverReturnCause,
                StartDate = model.StartDate,
                Title = model.Title,
                GetRecept = model.GetRecept,
                GetSampling = model.GetSampling,
                PrimaryAction = model.PrimaryAction,
                ReceiverAction = model.ReceiverAction,
                PrimaryActionDateTime = model.PrimaryActionDateTime,
                ReceiverActionDateTime = model.ReceiverActionDateTime,
                PrimaryActionUserName = model.PrimaryActionUserName,
                ReceiverActionUserName = model.ReceiverActionUserName,
            };
        }

        public void AddTestPrices(LabAgreementCommand command)
        {
            var labAgreement = _context.LabAgreements.FirstOrDefault(x => x.PrimaryAgreementLabCodeNew == command.PrimaryAgreementLabCodeNew && command.SamanehId.HasValue ? x.Id == command.SamanehId.Value : x.ContractNumber == command.ContractNumber);

            if (labAgreement != null)
            {
                labAgreement.GetSampling = command.GetSampling;
                labAgreement.GetRecept = command.GetRecept;

                _context.LabAgreements.Update(labAgreement);

                if (labAgreement.TestPrices != null)
                {
                    _context.LabAgreementTestPrices.RemoveRange(labAgreement.TestPrices);
                }

                var newTestPrices = command.TestPrices?.Select(x => new LabAgreementTestPrice()
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

                _context.LabAgreementTestPrices.AddRange(newTestPrices);
            }
        }

        public async Task<LabAgreementCommand?> GetLabAgreementByLabCode(int primaryLabCode, int receiverLabCode)
        {
            var now = DateTime.Now;

            var dbQuery = _context.LabAgreements
                .AsNoTracking()
                .Where(x =>
                    x.PrimaryAgreementLabCodeNew == primaryLabCode &&
                    x.ReceiverAgreementLabCodeNew == receiverLabCode &&
                    !x.ParentId.HasValue &&
                    x.StartDate.Date <= now.Date && x.ExpDate.Date >= now.Date &&
                    ((x.PrimaryAction == (int)PartyActionType.Signed && x.ReceiverAction == (int)PartyActionType.Signed) || (!x.PrimaryAction.HasValue || !x.ReceiverAction.HasValue))
                ).AsQueryable();

            var result = dbQuery
                .Select(ToLabAgreementMinimal())
                .FirstOrDefault();

            return result;
        }
    }
}
