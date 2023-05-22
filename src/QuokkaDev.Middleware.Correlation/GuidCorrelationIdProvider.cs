namespace QuokkaDev.Middleware.Correlation
{
    internal class GuidCorrelationIdProvider : ICorrelationIdProvider
    {
        public string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
