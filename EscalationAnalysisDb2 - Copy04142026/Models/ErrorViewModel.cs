namespace EscalationAnalysisDb2.Models
{
    public class ErrorViewModel
    {
        // id de la solicitud actual, sirve para rastrear errores
        public string? RequestId { get; set; }

        // indica si se debe mostrar el request id en la vista
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}