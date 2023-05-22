namespace QuokkaDev.Middleware.Correlation
{
    public interface ICorrelationService
    {
        string? GetCurrentCorrelationID();
        void SetCorrelationID(string id);
    }
}