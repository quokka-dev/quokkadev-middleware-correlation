namespace QuokkaDev.Middleware.Correlation
{
    public interface ICorrelationForwarder
    {
        void Forward(string correlationId);
    }
}
