namespace QuokkaDev.Middleware.Correlation
{
    public interface ICorrelationIdProvider
    {
        string GenerateCorrelationId();
    }
}
