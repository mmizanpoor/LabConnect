using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations
{
    public class SRLabRepository : ISRLabRepository
    {
        private readonly SamanehDbContext _context;

        public SRLabRepository(SamanehDbContext context)
        {
            _context = context;
        }


        public async Task<SRLabNameViewModel?> GetSRLabName(int labcode)
        {
            var dbQuery = _context.SRLabName.AsNoTracking();

            if ((labcode >= 1000 && labcode <= 9999) || (labcode >= 100000 && labcode <= 999999))
                dbQuery = dbQuery.Where(x => x.intLabId == labcode);
            else if (labcode >= 10000 && labcode <= 99999)
                dbQuery = dbQuery.Where(x => x.intLabIdNew == labcode || x.intLabId == labcode);
            else
                return null;

            return await dbQuery.Select(x => new SRLabNameViewModel()
            {
                intLabId = x.intLabId,
                intLabIdNew = x.intLabIdNew.HasValue ? x.intLabIdNew : x.intLabId,
                vchLabName = x.vchLabName
            }).FirstOrDefaultAsync();
        }

        public async Task<List<SRLabNameViewModel>> GetSRLabNameList(List<int> labcodes)
        {
            if (labcodes?.Any() != true)
                return new List<SRLabNameViewModel>();

            var labcodeFourOrSix = new List<int>();
            var labcodeFive = new List<int>();

            foreach (var code in labcodes)
            {
                if ((code >= 1000 && code <= 9999) || (code >= 100000 && code <= 999999))
                    labcodeFourOrSix.Add(code);
                else if (code >= 10000 && code <= 99999)
                    labcodeFive.Add(code);
            }

            var dbQuery = _context.SRLabName.AsQueryable().AsNoTracking();

            var list = new List<SRLabNameViewModel>();

            if (labcodeFourOrSix.Count > 0)
            {
                var labs = dbQuery.Where(x => labcodeFourOrSix.Contains(x.intLabId))
                    .Select(x => new SRLabNameViewModel()
                    {
                        intLabId = x.intLabId,
                        intLabIdNew = x.intLabIdNew.HasValue ? x.intLabIdNew : x.intLabId,
                        vchLabName = x.vchLabName
                    }).ToList();

                list.AddRange(labs);
            }
            if (labcodeFive.Count > 0)
            {
                var labs = dbQuery.Where(x =>
                        (x.intLabIdNew.HasValue && labcodeFive.Contains(x.intLabIdNew.Value)) ||
                        labcodeFive.Contains(x.intLabId))
                    .Select(x => new SRLabNameViewModel()
                    {
                        intLabId = x.intLabId,
                        intLabIdNew = x.intLabIdNew.HasValue ? x.intLabIdNew : x.intLabId,
                        vchLabName = x.vchLabName
                    }).ToList();

                list.AddRange(labs);
            }


            return list;
        }

        public async Task<List<SRLabNameViewModel>> GetSourceSenderLabName(int targetLabCode)
        {
            var sourceLabIds = _context.ReceptionNew
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.chrSourceReceptId) && string.IsNullOrEmpty(x.chrTargetReceptId) && x.intTargetLabId == targetLabCode)
                .GroupBy(x => x.intSourceLabId)
                .Select(x => x.Key)
                .ToList();

            return await _context.SRLabName
                .AsNoTracking()
                .Where(x => sourceLabIds.Any(c => c == x.intLabId))
                .Select(x => new SRLabNameViewModel()
                {
                    intLabId = x.intLabId,
                    intLabIdNew = x.intLabIdNew.HasValue ? x.intLabIdNew : x.intLabId,
                    vchLabName = x.vchLabName
                }).ToListAsync();
        }

        public async Task<List<SRLabNameViewModel>> GetTargetLabName(int sourceLabCode)
        {
            var sourceLabIds = _context.ReceptTestNew
                .AsNoTracking()
                .Where(x => string.IsNullOrEmpty(x.chrSourceReceiveResultDate) && !string.IsNullOrEmpty(x.vchResult) && x.intSourceLabId == sourceLabCode)
                .GroupBy(x => x.intTargetLabId)
                .Select(x => x.Key)
                .ToList();

            return await _context.SRLabName
                .AsNoTracking()
                .Where(x => sourceLabIds.Any(c => c == x.intLabId))
                .Select(x => new SRLabNameViewModel()
                {
                    intLabId = x.intLabId,
                    intLabIdNew = x.intLabIdNew,
                    vchLabName = x.vchLabName
                }).ToListAsync();
        }

        public async Task<List<SRLabNameViewModel>> GetAllSRLabs(int labCode, bool incoming)
        {
            List<int> labIds = new List<int>();

            if (incoming)
            {
                labIds = _context.ReceptionNew
                .AsNoTracking()
                .Where(x => x.intSourceLabId == labCode)
                .GroupBy(x => x.intTargetLabId)
                .Select(x => x.Key)
                .ToList();
            }
            else
            {
                labIds = _context.ReceptionNew
                .AsNoTracking()
                .Where(x => x.intTargetLabId == labCode)
                .GroupBy(x => x.intSourceLabId)
                .Select(x => x.Key)
                .ToList();
            }

            return await _context.SRLabName
                .AsNoTracking()
                .Where(x => labIds.Any(c => c == x.intLabId))
                .Select(x => new SRLabNameViewModel()
                {
                    intLabId = x.intLabId,
                    intLabIdNew = x.intLabIdNew,
                    vchLabName = x.vchLabName
                }).ToListAsync();
        }
    }
}

