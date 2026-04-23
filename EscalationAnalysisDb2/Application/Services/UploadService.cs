using Microsoft.VisualBasic.FileIO;
using EscalationAnalysisDb2.Application.ViewModels;

namespace EscalationAnalysisDb2.Application.Services
{
    public class UploadService
    {
        public List<UploadPreviewViewModel> ProcessFile(IFormFile file)
        {
            var result = new List<UploadPreviewViewModel>();

            // validar archivo seleccionado
            if (file == null)
                throw new Exception("Please select a file.");

            // validar archivo vacío real
            if (file.Length == 0)
                throw new Exception("The selected file is empty.");

            // validar formato permitido
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only CSV files are allowed.");

            using var stream = file.OpenReadStream();
            using var parser = new TextFieldParser(stream);

            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TrimWhiteSpace = true;

            // buscar primera fila útil para headers
            string[]? headers = null;

            while (!parser.EndOfData)
            {
                var row = parser.ReadFields();

                if (row != null &&
                    row.Any(x => !string.IsNullOrWhiteSpace(x)))
                {
                    headers = row;
                    break;
                }
            }

            // si no encontró nada útil
            if (headers == null)
                throw new Exception("The selected file is empty.");

            var columnMap = headers
                .Select((h, i) => new
                {
                    Name = h.Trim(),
                    Index = i
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToDictionary(
                    x => x.Name,
                    x => x.Index,
                    StringComparer.OrdinalIgnoreCase);

            // columnas obligatorias del reporte
            var requiredColumns = new[]
            {
                "CaseNumber",
                "EscalationTask",
                "Severity",
                "Status",
                "Owner",
                "Account"
            };

            var missingColumns = requiredColumns
                .Where(x => !columnMap.ContainsKey(x))
                .ToList();

            if (missingColumns.Any())
                throw new Exception(
                    "The uploaded file does not match the required report template.");

            // obtener valor por nombre de columna
            string? GetValue(string column, string[] values)
            {
                if (!columnMap.ContainsKey(column))
                    return null;

                var index = columnMap[column];

                if (index >= values.Length)
                    return null;

                return values[index]?.Trim();
            }

            int rowNumber = 1; // empieza en header
            var invalidRows = new List<int>();

            // leer filas del archivo
            while (!parser.EndOfData)
            {
                rowNumber++;

                string[]? values;

                try
                {
                    values = parser.ReadFields();
                }
                catch
                {
                    invalidRows.Add(rowNumber);
                    continue;
                }

                // fila completamente vacía
                if (values == null || values.All(x => string.IsNullOrWhiteSpace(x)))
                {
                    invalidRows.Add(rowNumber);
                    continue;
                }

                var caseNumber = GetValue("CaseNumber", values);
                var escalationTask = GetValue("EscalationTask", values);
                var severityRaw = GetValue("Severity", values);
                var status = GetValue("Status", values);
                var owner = GetValue("Owner", values);
                var account = GetValue("Account", values);
                var region = GetValue("Region", values);
                var version = GetValue("ProductVersion", values);
                var dateRaw = GetValue("EscalationDate", values);

                // validar campos requeridos
                if (string.IsNullOrWhiteSpace(caseNumber) ||
                    string.IsNullOrWhiteSpace(escalationTask) ||
                    string.IsNullOrWhiteSpace(severityRaw) ||
                    string.IsNullOrWhiteSpace(status) ||
                    string.IsNullOrWhiteSpace(owner) ||
                    string.IsNullOrWhiteSpace(account))
                {
                    invalidRows.Add(rowNumber);
                    continue;
                }

                // limpiar severidad
                var severity = severityRaw
                    .Replace("1 (", "")
                    .Replace("2 (", "")
                    .Replace("3 (", "")
                    .Replace("4 (", "")
                    .Replace(")", "")
                    .Trim();

                DateTime? escalationDate = null;

                if (!string.IsNullOrWhiteSpace(dateRaw) &&
                    DateTime.TryParse(dateRaw, out var parsedDate))
                {
                    escalationDate = parsedDate;
                }

                result.Add(new UploadPreviewViewModel
                {
                    CaseNumber = caseNumber,
                    EscalationTask = escalationTask,
                    Severity = severity,
                    Status = status,
                    Owner = owner,
                    Account = account,
                    Region = region,
                    ProductVersion = version,
                    EscalationDate = escalationDate
                });
            }

            // si hubo filas inválidas, rechazar todo el archivo
            if (invalidRows.Any())
            {
                throw new Exception(
                    $"The file contains empty or incomplete rows. Please review row(s): {string.Join(", ", invalidRows)}.");
            }

            // si no quedó ninguna fila válida
            if (!result.Any())
            {
                throw new Exception("No valid rows were found in the selected file.");
            }

            return result;
        }
    }
}