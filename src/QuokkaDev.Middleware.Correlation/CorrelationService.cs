namespace QuokkaDev.Middleware.Correlation
{
    public class CorrelationService : ICorrelationService
    {
        private string? correlationID;

        public CorrelationService()
        {
        }

        public void SetCorrelationID(string id)
        {
            if (string.IsNullOrWhiteSpace(correlationID))
            {
                correlationID = id;
            }
        }

        public string? GetCurrentCorrelationID()
        {
            return correlationID;
        }
    }
}
