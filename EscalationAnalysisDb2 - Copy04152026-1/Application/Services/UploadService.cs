using EscalationAnalysisDb2.Application.ViewModels;

namespace EscalationAnalysisDb2.Application.Services
{
    public class UploadService
    {
        public List<UploadPreviewViewModel> ProcessFile(IFormFile file)
        {
            // lista donde voy a guardar los datos procesados del archivo
            var result = new List<UploadPreviewViewModel>();

            // valido que el archivo exista y sea csv
            if (file == null || !file.FileName.EndsWith(".csv"))
                throw new Exception("El archivo debe estar en formato CSV (.csv)");

            using var reader = new StreamReader(file.OpenReadStream());

            // leo la primera línea que corresponde a los encabezados
            var headerLine = reader.ReadLine();

            // valido que el archivo tenga encabezados
            if (string.IsNullOrWhiteSpace(headerLine))
                throw new Exception("El archivo no contiene encabezados");

            var headers = headerLine.Split(',');

            // creo un diccionario para mapear el nombre de la columna con su posición
            var columnMap = headers
                .Select((h, i) => new { Name = h.Trim(), Index = i })
                .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

            // columnas obligatorias que el archivo debe tener
            var requiredColumns = new[]
            {
                "CaseNumber",
                "EscalationTask",
                "Severity",
                "Status",
                "Owner",
                "Account"
            };

            // valido que todas las columnas requeridas existan
            foreach (var col in requiredColumns)
            {
                if (!columnMap.ContainsKey(col))
                    throw new Exception($"Falta la columna obligatoria: {col}");
            }

            // función para obtener valores de forma segura desde cada fila
            string GetValue(string column, string[] values)
            {
                if (!columnMap.ContainsKey(column))
                    return null;

                var index = columnMap[column];

                return values.Length > index ? values[index]?.Trim() : null;
            }

            // empiezo a leer cada fila del archivo
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                // ignoro líneas vacías
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split(',');

                // obtengo los valores de cada columna
                var caseNumber = GetValue("CaseNumber", values);
                var escalationTask = GetValue("EscalationTask", values);
                var severityRaw = GetValue("Severity", values);
                var status = GetValue("Status", values);
                var owner = GetValue("Owner", values);
                var account = GetValue("Account", values);
                var region = GetValue("Region", values);
                var version = GetValue("ProductVersion", values);
                var dateRaw = GetValue("EscalationDate", values);

                // limpio el formato de severidad (ej: "1 (Critical)" -> "Critical")
                var severity = severityRaw?
                    .Replace("1 (", "")
                    .Replace("2 (", "")
                    .Replace("3 (", "")
                    .Replace("4 (", "")
                    .Replace(")", "")
                    .Trim();

                // intento convertir la fecha a formato DateTime
                DateTime? escalationDate = null;

                if (!string.IsNullOrWhiteSpace(dateRaw) &&
                    DateTime.TryParse(dateRaw, out var parsedDate))
                {
                    escalationDate = parsedDate;
                }

                // valido que la fila tenga los datos mínimos necesarios
                if (string.IsNullOrWhiteSpace(caseNumber) ||
                    string.IsNullOrWhiteSpace(escalationTask))
                {
                    continue;
                }

                // agrego el registro procesado a la lista
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

            // retorno la lista con los datos ya procesados
            return result;
        }
    }
}