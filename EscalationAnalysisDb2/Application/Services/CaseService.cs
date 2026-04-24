using EscalationAnalysisDb2.Application.ViewModels;
using EscalationAnalysisDb2.Domain.Entities;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EscalationAnalysisDb2.Application.Services
{
    public class CaseService
    {
        // contexto para trabajar con la bd
        private readonly EscalationsDbContext _context;

        public CaseService(EscalationsDbContext context)
        {
            _context = context;
        }

        // guarda la informacion del archivo cargado
        public async Task SaveData(
            List<UploadPreviewViewModel> data,
            int userId,
            string fileName)
        {
            // crea historial del reporte subido
            var report = new Report
            {
                UploadedByUserId = userId,
                UploadDate = DateTime.Now,
                FileName = fileName
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            foreach (var item in data)
            {
                // si falta info importante no guarda esa fila
                if (string.IsNullOrWhiteSpace(item.CaseNumber) ||
                    string.IsNullOrWhiteSpace(item.EscalationTask) ||
                    string.IsNullOrWhiteSpace(item.Severity) ||
                    string.IsNullOrWhiteSpace(item.Owner) ||
                    string.IsNullOrWhiteSpace(item.Account) ||
                    string.IsNullOrWhiteSpace(item.Status))
                {
                    continue;
                }

                var account = await GetOrCreateAccount(item.Account);
                var owner = await GetOrCreateOwner(item.Owner, item.Region);

                var severityId = MapSeverity(item.Severity);
                var statusId = MapStatus(item.Status);

                // guarda caso principal
                var caseRecord = new CaseRecord
                {
                    CaseNumber = item.CaseNumber.Trim(),
                    AccountId = account.AccountId,
                    CaseOwnerId = owner.CaseOwnerId,
                    SeverityId = severityId,
                    StatusId = statusId,
                    ProductVersion = item.ProductVersion ?? "Unknown",
                    ReportId = report.ReportId
                };

                _context.CaseRecords.Add(caseRecord);
                await _context.SaveChangesAsync();

                // guarda escalacion ligada al caso
                var escalation = new Escalation
                {
                    CaseRecordId = caseRecord.CaseRecordId,
                    EscalationTask = item.EscalationTask.Trim(),
                    EscalationDate = item.EscalationDate ?? DateTime.Now,
                    SeverityId = severityId,
                    StatusId = statusId
                };

                _context.Escalations.Add(escalation);
            }

            await _context.SaveChangesAsync();
        }

        // busca cuenta y si no existe la crea
        private async Task<Account> GetOrCreateAccount(string name)
        {
            name ??= "Unknown";

            var account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.AccountName == name);

            if (account == null)
            {
                account = new Account
                {
                    AccountName = name
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
            }

            return account;
        }

        // busca owner y si no existe lo crea
        private async Task<CaseOwner> GetOrCreateOwner(string name, string region)
        {
            name ??= "Unknown";

            var owner = await _context.CaseOwners
                .FirstOrDefaultAsync(x => x.CaseOwnerName == name);

            if (owner == null)
            {
                owner = new CaseOwner
                {
                    CaseOwnerName = name,
                    Region = region ?? "Unknown"
                };

                _context.CaseOwners.Add(owner);
                await _context.SaveChangesAsync();
            }

            return owner;
        }

        // convierte severidad texto a id
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

        // convierte estado texto a id
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

        // aplica filtros generales para reutilizar consultas
        private IQueryable<Escalation> ApplyFilters(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            var query = _context.Escalations
                .Include(e => e.CaseRecord)
                    .ThenInclude(c => c.CaseOwner)
                .Include(e => e.CaseRecord)
                    .ThenInclude(c => c.Account)
                .Include(e => e.CaseRecord)
                    .ThenInclude(c => c.Report)
                .AsQueryable();

            // filtra por usuario
            if (userId.HasValue)
            {
                query = query.Where(e =>
                    e.CaseRecord.Report.UploadedByUserId == userId.Value);
            }

            // filtra por mes
            if (month.HasValue)
            {
                query = query.Where(e =>
                    e.EscalationDate.Month == month.Value);
            }

            // filtra por severidad
            if (severity != null && severity.Any())
            {
                query = query.Where(e =>
                    severity.Contains(e.SeverityId));
            }

            // filtra por region
            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(e =>
                    e.CaseRecord.CaseOwner.Region != null &&
                    e.CaseRecord.CaseOwner.Region.ToLower() == region.ToLower());
            }

            // filtra por version
            if (!string.IsNullOrWhiteSpace(version))
            {
                query = query.Where(e =>
                    e.CaseRecord.ProductVersion != null &&
                    e.CaseRecord.ProductVersion.ToLower() == version.ToLower());
            }

            return query;
        }

        // total de escalaciones
        public async Task<int> GetTotalEscalations(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            return await ApplyFilters(month, severity, region, version, userId)
                .CountAsync();
        }

        // total de casos unicos
        public async Task<int> GetTotalCases(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            return await ApplyFilters(month, severity, region, version, userId)
                .Select(e => e.CaseRecord.CaseNumber)
                .Distinct()
                .CountAsync();
        }

        // version con mas impacto
        public async Task<string> GetMostImpactedVersion(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            var result = await ApplyFilters(month, severity, region, version, userId)
                .Where(e => e.CaseRecord.ProductVersion != null &&
                            e.CaseRecord.ProductVersion != "")
                .GroupBy(e => e.CaseRecord.ProductVersion)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(result)
                ? "No data"
                : result;
        }

        // valores para grafico mensual
        public async Task<List<int>> GetTrendValues(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            var query = ApplyFilters(month, severity, region, version, userId);

            var grouped = await query
                .GroupBy(e => e.EscalationDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new List<int>();

            for (int i = 1; i <= 12; i++)
            {
                var item = grouped.FirstOrDefault(x => x.Month == i);
                result.Add(item?.Count ?? 0);
            }

            return result;
        }

        // nombres de meses
        public List<string> GetTrendLabels()
        {
            return new List<string>
            {
                "Jan","Feb","Mar","Apr","May","Jun",
                "Jul","Aug","Sep","Oct","Nov","Dec"
            };
        }

        // top clientes
        public async Task<List<string>> GetTopAccounts(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            return await ApplyFilters(month, severity, region, version, userId)
                .GroupBy(e => e.CaseRecord.Account.AccountName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        // cantidades top clientes
        public async Task<List<int>> GetTopAccountValues(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            return await ApplyFilters(month, severity, region, version, userId)
                .GroupBy(e => e.CaseRecord.Account.AccountName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Count())
                .ToListAsync();
        }

        // top owners
        public async Task<List<string>> GetTopOwners(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            return await ApplyFilters(month, severity, region, version, userId)
                .GroupBy(e => e.CaseRecord.CaseOwner.CaseOwnerName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        // cantidades top owners
        public async Task<List<int>> GetTopOwnerValues(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            return await ApplyFilters(month, severity, region, version, userId)
                .GroupBy(e => e.CaseRecord.CaseOwner.CaseOwnerName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Count())
                .ToListAsync();
        }

        // insight porcentaje criticos
        public async Task<string> GetMainInsight1(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            var total = await GetTotalEscalations(month, severity, region, version, userId);

            if (total == 0)
                return "No data available";

            var critical = await ApplyFilters(month, severity, region, version, userId)
                .CountAsync(e => e.SeverityId == 1);

            var percent = Math.Round((double)critical / total * 100, 0);

            return $"Critical cases represent {percent}%";
        }

        // insight severidad mas comun
        public async Task<string> GetMainInsight2(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            var mostCommon = await ApplyFilters(month, severity, region, version, userId)
                .GroupBy(e => e.SeverityId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            return $"Most common severity is {mostCommon}";
        }

        // insight tendencia entre meses
        public async Task<string> GetMainInsight3(
            int? month,
            List<int> severity,
            string region,
            string version,
            int? userId)
        {
            var data = await ApplyFilters(month, severity, region, version, userId)
                .GroupBy(e => e.EscalationDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            if (data.Count < 2)
                return "Not enough trend data";

            var last = data.Last();
            var previous = data[data.Count - 2];

            string monthName = System.Globalization.CultureInfo
                .CurrentCulture
                .DateTimeFormat
                .GetAbbreviatedMonthName(previous.Month);

            if (last.Count > previous.Count)
                return $"Escalations increased after {monthName}";

            if (last.Count < previous.Count)
                return $"Escalations dropped after {monthName}";

            return $"Escalations remained stable after {monthName}";
        }
    }
}