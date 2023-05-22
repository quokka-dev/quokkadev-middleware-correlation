namespace QuokkaDev.Middleware.Correlation
{
    public class CorrelationOptions
    {
        public bool TryToUseRequestHeader { get; init; }
        public IEnumerable<string>? ValidRequestHeaders { get; init; }
        public bool EnrichLog { get; init; }
        public string LogPropertyName { get; init; } = Constants.DEFAULT_CORRELATION_LOG_PROPERTY;
        public bool WriteCorrelationIDToResponse { get; init; }
        public string DefaultHeaderName { get; set; } = Constants.DEFAULT_CORRELATION_HEADER_NAME;
    }
}
