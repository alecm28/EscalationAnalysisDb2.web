using EscalationAnalysisDb2.Application.ViewModels;
using EscalationAnalysisDb2.Domain.Entities;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EscalationAnalysisDb2.Application.Services
{
    public class CaseService
    {
        // contexto de base de datos para acceder a las tablas
        private readonly EscalationsDbContext _context;

        // constructor donde recibo el contexto por inyección de dependencias
        public CaseService(EscalationsDbContext context)
        {
            _context = context;
        }

        public async Task SaveData(List<UploadPreviewViewModel> data, int userId, string fileName)
        {
            // creo un registro del reporte que se está subiendo
            var report = new Report
            {
                UploadedByUserId = userId,
                UploadDate = DateTime.Now,
                FileName = fileName
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // agrupo los datos por número de caso porque un caso puede tener varias escalaciones
            var groupedCases = data.GroupBy(x => x.CaseNumber);

            foreach (var group in groupedCases)
            {
                // tomo el primer registro del grupo para datos generales del caso
                var first = group.First();

                // obtengo el nombre del account o asigno unknown si viene vacío
                var accountName = string.IsNullOrWhiteSpace(first.Account) ? "Unknown" : first.Account.Trim();

                // busco si el account ya existe
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountName.ToLower() == accountName.ToLower());

                // si no existe lo creo
                if (account == null)
                {
                    account = new Account { AccountName = accountName };
                    _context.Accounts.Add(account);
                    await _context.SaveChangesAsync();
                }

                // manejo del owner, igual que account
                var ownerName = string.IsNullOrWhiteSpace(first.Owner) ? "Unknown" : first.Owner.Trim();

                var owner = await _context.CaseOwners
                    .FirstOrDefaultAsync(o => o.CaseOwnerName.ToLower() == ownerName.ToLower());

                // si no existe el owner lo creo junto con la región
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

                // convierto los valores de texto a ids
                int severityId = MapSeverity(first.Severity);
                int statusId = MapStatus(first.Status);

                // reviso si el caso ya existe
                var existingCase = await _context.CaseRecords
                    .FirstOrDefaultAsync(c => c.CaseNumber == first.CaseNumber);

                // si no existe lo creo
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

                // recorro todas las escalaciones del caso
                foreach (var item in group)
                {
                    // si no hay escalation task lo ignoro
                    if (string.IsNullOrWhiteSpace(item.EscalationTask))
                        continue;

                    int escSeverityId = MapSeverity(item.Severity);
                    int escStatusId = MapStatus(item.Status);

                    // reviso si esa escalación ya existe
                    var exists = await _context.Escalations
                        .AnyAsync(e => e.EscalationTask == item.EscalationTask);

                    // si no existe la agrego
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

            // guardo todos los cambios al final
            await _context.SaveChangesAsync();
        }

        // convierte la severidad de texto a id
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

        // convierte el estado de texto a id
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

        // devuelve el total de casos
        public async Task<int> GetTotalCases()
        {
            return await _context.CaseRecords.CountAsync();
        }

        // devuelve el total de escalaciones
        public async Task<int> GetTotalEscalations()
        {
            return await _context.Escalations.CountAsync();
        }

        // agrupa las escalaciones por severidad para el gráfico
        public async Task<List<object>> GetEscalationsBySeverity()
        {
            return await _context.Escalations
                .Include(e => e.Severity)
                .GroupBy(e => e.Severity.SeverityName)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync<object>();
        }

        // agrupa las escalaciones por estado
        public async Task<List<object>> GetEscalationsByStatus()
        {
            return await _context.Escalations
                .Include(e => e.Status)
                .GroupBy(e => e.Status.StatusName)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync<object>();
        }

        // obtiene cantidad de escalaciones por mes para tendencia
        public async Task<List<int>> GetTrendValues()
        {
            return await _context.Escalations
                .GroupBy(e => e.EscalationDate.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToListAsync();
        }

        // etiquetas de meses para el gráfico
        public List<string> GetTrendLabels()
        {
            return new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        }

        // obtiene las cuentas con más casos
        public async Task<List<string>> GetTopAccounts()
        {
            return await _context.CaseRecords
                .Include(c => c.Account)
                .GroupBy(c => c.Account.AccountName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        // obtiene los owners con más casos
        public async Task<List<string>> GetTopOwners()
        {
            return await _context.CaseRecords
                .Include(c => c.CaseOwner)
                .GroupBy(c => c.CaseOwner.CaseOwnerName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToListAsync();
        }

        // calcula porcentaje de casos críticos
        public async Task<string> GetMainInsight1()
        {
            var total = await _context.Escalations.CountAsync();
            if (total == 0) return "No data available";

            var critical = await _context.Escalations.CountAsync(e => e.SeverityId == 1);
            var percent = Math.Round((double)critical / total * 100, 0);

            return $"Critical cases represent {percent}%";
        }

        // obtiene la severidad más común
        public async Task<string> GetMainInsight2()
        {
            var mostCommon = await _context.Escalations
                .GroupBy(e => e.SeverityId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            return $"Most common severity is {mostCommon}";
        }

        // obtiene el último mes con actividad
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