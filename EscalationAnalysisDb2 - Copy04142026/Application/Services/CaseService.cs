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

        // =========================
        // GUARDAR DATA DESDE CSV
        // =========================

        public async Task SaveData(List<UploadPreviewViewModel> data, int userId, string fileName)
        {
            foreach (var item in data)
            {
                if (string.IsNullOrWhiteSpace(item.CaseNumber))
                    continue;

                // Buscar o crear CaseRecord
                var existingCase = await _context.CaseRecords
                    .FirstOrDefaultAsync(c => c.CaseNumber == item.CaseNumber);

                if (existingCase == null)
                {
                    existingCase = new CaseRecord
                    {
                        CaseNumber = item.CaseNumber,
                        ProductVersion = item.ProductVersion ?? "Unknown"
                    };

                    _context.CaseRecords.Add(existingCase);
                    await _context.SaveChangesAsync();
                }

                // Crear escalación
                var escalation = new Escalation
                {
                    CaseRecordId = existingCase.CaseRecordId,
                    EscalationTask = item.EscalationTask ?? "",
                    EscalationDate = item.EscalationDate ?? DateTime.Now,
                    SeverityId = MapSeverity(item.Severity),
                    StatusId = MapStatus(item.Status)
                };

                _context.Escalations.Add(escalation);
            }

            await _context.SaveChangesAsync();
        }

        // =========================
        // MAPEO
        // =========================

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

        // =========================
        // FILTROS BASE
        // =========================

        private IQueryable<Escalation> ApplyFilters(
            int? month,
            List<int> severity,
            string region,
            string version)
        {
            var query = _context.Escalations
                .Include(e => e.CaseRecord)
                .ThenInclude(c => c.CaseOwner)
                .Include(e => e.CaseRecord)
                .ThenInclude(c => c.Account)
                .AsQueryable();

            if (month.HasValue)
                query = query.Where(e => e.EscalationDate.Month == month.Value);

            if (severity != null && severity.Any())
                query = query.Where(e => severity.Contains(e.SeverityId));

            if (!string.IsNullOrWhiteSpace(region))
                query = query.Where(e =>
                    e.CaseRecord.CaseOwner.Region != null &&
                    e.CaseRecord.CaseOwner.Region.ToLower() == region.ToLower());

            if (!string.IsNullOrWhiteSpace(version))
                query = query.Where(e =>
                    e.CaseRecord.ProductVersion != null &&
                    e.CaseRecord.ProductVersion.ToLower() == version.ToLower());

            return query;
        }

        // =========================
        // MÉTRICAS
        // =========================

        public async Task<int> GetTotalEscalations(int? month, List<int> severity, string region, string version)
        {
            return await ApplyFilters(month, severity, region, version).CountAsync();
        }

        public async Task<int> GetTotalCases(int? month, List<int> severity, string region, string version)
        {
            return await ApplyFilters(month, severity, region, version)
                .Select(e => e.CaseRecordId)
                .Distinct()
                .CountAsync();
        }

        // =========================
        // TENDENCIA
        // =========================

        public async Task<List<int>> GetTrendValues(int? month, List<int> severity, string region, string version)
        {
            var query = ApplyFilters(month, severity, region, version);

            var grouped = await query
                .GroupBy(e => e.EscalationDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new List<int>();

            for (int i = 1; i <= 12; i++)
            {
                var item = grouped.FirstOrDefault(x => x.Month == i);
                result.Add(item?.Count ?? 0);
            }

            return result;
        }

        public List<string> GetTrendLabels()
        {
            return new List<string>
            {
                "Jan","Feb","Mar","Apr","May","Jun",
                "Jul","Aug","Sep","Oct","Nov","Dec"
            };
        }

        // =========================
        // TOPS
        // =========================

        public async Task<List<string>> GetTopAccounts(int? month, List<int> severity, string region, string version)
        {
            return await ApplyFilters(month, severity, region, version)
                .GroupBy(e => e.CaseRecord.Account.AccountName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        public async Task<List<string>> GetTopOwners(int? month, List<int> severity, string region, string version)
        {
            return await ApplyFilters(month, severity, region, version)
                .GroupBy(e => e.CaseRecord.CaseOwner.CaseOwnerName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        // =========================
        // INSIGHTS
        // =========================

        public async Task<string> GetMainInsight1(int? month, List<int> severity, string region, string version)
        {
            var total = await GetTotalEscalations(month, severity, region, version);

            if (total == 0)
                return "No data available";

            var critical = await ApplyFilters(month, severity, region, version)
                .CountAsync(e => e.SeverityId == 1);

            var percent = Math.Round((double)critical / total * 100, 0);

            return $"Critical cases represent {percent}%";
        }

        public async Task<string> GetMainInsight2(int? month, List<int> severity, string region, string version)
        {
            var mostCommon = await ApplyFilters(month, severity, region, version)
                .GroupBy(e => e.SeverityId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            return $"Most common severity is {mostCommon}";
        }

        public async Task<string> GetMainInsight3(int? month, List<int> severity, string region, string version)
        {
            var latestMonth = await ApplyFilters(month, severity, region, version)
                .OrderByDescending(e => e.EscalationDate)
                .Select(e => e.EscalationDate.Month)
                .FirstOrDefaultAsync();

            return $"Latest activity month: {latestMonth}";
        }
    }
}