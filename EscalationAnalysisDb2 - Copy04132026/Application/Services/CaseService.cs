using EscalationAnalysisDb2.Application.ViewModels;
using EscalationAnalysisDb2.Domain.Entities;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EscalationAnalysisDb2.Application.Services
{
    public class CaseService
    {
        private readonly EscalationsDbContext _context;

        public CaseService(EscalationsDbContext context)
        {
            _context = context;
        }

        public async Task SaveData(List<UploadPreviewViewModel> data, int userId, string fileName)
        {
            var report = new Report
            {
                UploadedByUserId = userId,
                UploadDate = DateTime.Now,
                FileName = fileName
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var groupedCases = data.GroupBy(x => x.CaseNumber);

            foreach (var group in groupedCases)
            {
                var first = group.First();

                var accountName = string.IsNullOrWhiteSpace(first.Account) ? "Unknown" : first.Account.Trim();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountName.ToLower() == accountName.ToLower());

                if (account == null)
                {
                    account = new Account { AccountName = accountName };
                    _context.Accounts.Add(account);
                    await _context.SaveChangesAsync();
                }

                var ownerName = string.IsNullOrWhiteSpace(first.Owner) ? "Unknown" : first.Owner.Trim();

                var owner = await _context.CaseOwners
                    .FirstOrDefaultAsync(o => o.CaseOwnerName.ToLower() == ownerName.ToLower());

                if (owner == null)
                {
                    owner = new CaseOwner
                    {
                        CaseOwnerName = ownerName,
                        Region = string.IsNullOrWhiteSpace(first.Region) ? "Unknown" : first.Region.Trim()
                    };

                    _context.CaseOwners.Add(owner);
                    await _context.SaveChangesAsync();
                }

                int severityId = MapSeverity(first.Severity);
                int statusId = MapStatus(first.Status);

                var existingCase = await _context.CaseRecords
                    .FirstOrDefaultAsync(c => c.CaseNumber == first.CaseNumber);

                if (existingCase == null)
                {
                    existingCase = new CaseRecord
                    {
                        CaseNumber = first.CaseNumber,
                        AccountId = account.AccountId,
                        CaseOwnerId = owner.CaseOwnerId,
                        ReportId = report.ReportId,
                        SeverityId = severityId,
                        StatusId = statusId,
                        ProductVersion = string.IsNullOrWhiteSpace(first.ProductVersion)
                            ? "Unknown"
                            : first.ProductVersion.Trim()
                    };

                    _context.CaseRecords.Add(existingCase);
                    await _context.SaveChangesAsync();
                }

                foreach (var item in group)
                {
                    if (string.IsNullOrWhiteSpace(item.EscalationTask))
                        continue;

                    int escSeverityId = MapSeverity(item.Severity);
                    int escStatusId = MapStatus(item.Status);

                    var exists = await _context.Escalations
                        .AnyAsync(e => e.EscalationTask == item.EscalationTask);

                    if (!exists)
                    {
                        var escalation = new Escalation
                        {
                            CaseRecordId = existingCase.CaseRecordId,
                            EscalationTask = item.EscalationTask.Trim(),
                            EscalationDate = item.EscalationDate ?? DateTime.Now,
                            SeverityId = escSeverityId,
                            StatusId = escStatusId
                        };

                        _context.Escalations.Add(escalation);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private int MapSeverity(string severity)
        {
            return severity?.Trim() switch
            {
                "Critical" => 1,
                "High" => 2,
                "Moderate" => 3,
                "Low" => 4,
                _ => 3
            };
        }

        private int MapStatus(string status)
        {
            return status?.Trim() switch
            {
                "IBM is working" => 1,
                "Awaiting your feedback" => 2,
                "Waiting for IBM" => 3,
                "Waiting for Development" => 4,
                _ => 1
            };
        }

        // 🔥 NUEVOS MÉTODOS (FIX DEL ERROR)

        public async Task<List<object>> GetEscalationsBySeverity()
        {
            return await _context.Escalations
                .Include(e => e.Severity)
                .GroupBy(e => e.Severity.SeverityName)
                .Select(g => new
                {
                    Severity = g.Key,
                    Count = g.Count()
                })
                .ToListAsync<object>();
        }

        public async Task<List<object>> GetEscalationsByStatus()
        {
            return await _context.Escalations
                .Include(e => e.Status)
                .GroupBy(e => e.Status.StatusName)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync<object>();
        }

        // 🔥 FILTROS

        public async Task<int> GetTotalCases(int? month, List<int> severity, string region, string version)
        {
            var query = _context.CaseRecords
                .Include(c => c.CaseOwner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(region))
                query = query.Where(c => c.CaseOwner.Region == region);

            if (!string.IsNullOrEmpty(version))
                query = query.Where(c => c.ProductVersion == version);

            return await query.CountAsync();
        }

        public async Task<int> GetTotalEscalations(int? month, List<int> severity, string region, string version)
        {
            var query = _context.Escalations
                .Include(e => e.CaseRecord)
                .ThenInclude(c => c.CaseOwner)
                .AsQueryable();

            if (month.HasValue)
                query = query.Where(e => e.EscalationDate.Month == month);

            if (severity != null && severity.Any())
                query = query.Where(e => severity.Contains(e.SeverityId));

            if (!string.IsNullOrEmpty(region))
                query = query.Where(e => e.CaseRecord.CaseOwner.Region == region);

            if (!string.IsNullOrEmpty(version))
                query = query.Where(e => e.CaseRecord.ProductVersion == version);

            return await query.CountAsync();
        }

        public async Task<List<int>> GetTrendValues(int? month, List<int> severity, string region, string version)
        {
            var query = _context.Escalations
                .Include(e => e.CaseRecord)
                .ThenInclude(c => c.CaseOwner)
                .AsQueryable();

            if (severity != null && severity.Any())
                query = query.Where(e => severity.Contains(e.SeverityId));

            if (!string.IsNullOrEmpty(region))
                query = query.Where(e => e.CaseRecord.CaseOwner.Region == region);

            if (!string.IsNullOrEmpty(version))
                query = query.Where(e => e.CaseRecord.ProductVersion == version);

            var trends = await query
                .GroupBy(e => e.EscalationDate.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToListAsync();

            return trends;
        }

        public List<string> GetTrendLabels()
        {
            return new List<string>
            {
                "Jan","Feb","Mar","Apr","May","Jun",
                "Jul","Aug","Sep","Oct","Nov","Dec"
            };
        }

        public async Task<List<string>> GetTopAccounts(int? month, List<int> severity, string region, string version)
        {
            var query = _context.CaseRecords
                .Include(c => c.Account)
                .Include(c => c.CaseOwner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(region))
                query = query.Where(c => c.CaseOwner.Region == region);

            if (!string.IsNullOrEmpty(version))
                query = query.Where(c => c.ProductVersion == version);

            return await query
                .GroupBy(c => c.Account.AccountName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        public async Task<List<string>> GetTopOwners(int? month, List<int> severity, string region, string version)
        {
            var query = _context.CaseRecords
                .Include(c => c.CaseOwner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(region))
                query = query.Where(c => c.CaseOwner.Region == region);

            if (!string.IsNullOrEmpty(version))
                query = query.Where(c => c.ProductVersion == version);

            return await query
                .GroupBy(c => c.CaseOwner.CaseOwnerName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        public async Task<string> GetMainInsight1()
        {
            var total = await _context.Escalations.CountAsync();
            if (total == 0) return "No data available";

            var critical = await _context.Escalations.CountAsync(e => e.SeverityId == 1);
            var percent = Math.Round((double)critical / total * 100, 0);

            return $"Critical cases represent {percent}%";
        }

        public async Task<string> GetMainInsight2()
        {
            var mostCommon = await _context.Escalations
                .GroupBy(e => e.SeverityId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            return $"Most common severity is {mostCommon}";
        }

        public async Task<string> GetMainInsight3()
        {
            var latestMonth = await _context.Escalations
                .OrderByDescending(e => e.EscalationDate)
                .Select(e => e.EscalationDate.Month)
                .FirstOrDefaultAsync();

            return $"Latest activity month: {latestMonth}";
        }
    }
}