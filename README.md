[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=quokka-dev_quokkadev-middleware-correlation&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=quokka-dev_quokkadev-middleware-correlation) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=quokka-dev_quokkadev-middleware-correlation&metric=coverage)](https://sonarcloud.io/summary/new_code?id=quokka-dev_quokkadev-middleware-correlation) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=quokka-dev_quokkadev-middleware-correlation&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=quokka-dev_quokkadev-middleware-correlation) [![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=quokka-dev_quokkadev-middleware-correlation&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=quokka-dev_quokkadev-middleware-correlation) [![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=quokka-dev_quokkadev-middleware-correlation&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=quokka-dev_quokkadev-middleware-correlation) ![publish workflow](https://github.com/quokka-dev/quokkadev-middleware-correlation/actions/workflows/publish.yml/badge.svg)

# QuokkaDev.Middleware.Correlation

QuokkaDev.Middleware.Correlation is a .NET middleware for correlating HTTP request using a correlationId. It can read a correlationId from the incoming request searching in a set of valid headers; if correlationId is not found in the incoming request it automatically generate a new one.
You can enrich your log using using the generated id as a property and you can write the id in the HTTP response. You can also forward the correlationId to other services called through HttpClient.

## Installing QuokkaDev.Middleware.Correlation

You should install the package via the .NET command line interface

    Install-Package QuokkaDev.Middleware.Correlation

## Using QuokkaDev.Middleware.Correlation
For using the middleware you must configure the services and then add it to the pipeline.

#### **`startup.cs`**
```csharp
//Add default services
services.AddCorrelation();

//Add custom services
services.AddCorrelation<MyCustomCorrelationService, MyCustomCorrelationIdProvider>();

//Add custom ICorrelationService service
services.AddCorrelationWithService<MyCustomCorrelationService>();

//Add custom ICorrelationIdProvider service
services.AddCorrelationWithProvider< MyCustomCorrelationIdProvider>();

//Register the correlation middleware in the pipeline
app.UseCorrelation(options => {
    options.TryToUseRequestHeader = true;
    options.ValidRequestHeaders = new string[] { "X-Correlation-Id" };
    options.EnrichLog = true;
    options.LogPropertyName = "CorrelationId";
    options.WriteCorrelationIDToResponse = true,
    options.DefaultHeaderName = "";
});
```

## Configure middleware

### TryToUseRequestHeader (default: true)
Set this property to *true* if you want to inspect incoming request for searching a correlationId set by the client. middleware search between all request headers comparing them to a list of valid header (see *ValidRequestHeaders* below). The first matched header value is used as correlationId; if not matches was found a new CorrelationId is generated.

Set this property to *false* for always generate the correlationId.

### ValidRequestHeaders (default: ["X-Correlation-Id"])
An array of valid headers name for searching a correlationId in the incoming request. This property take effect only if *TryToUseRequestHeader* is set to *true*.

### EnrichLog (default: true)
Setting this property to *true* add the correlationId to your logger.

### LogPropertyName (default: "CorrelationId")
Set the property name used for enrich your log. This property take effect only if *EnrichLog* is set to *true*.

### WriteCorrelationIDToResponse (default: true)
Set this property to *true* if you want correlationId to be write in HTTP response. If the correlationId was found in the incoming request it will be write to the response using the same header name used by client in the original request else *DefaultHeaderName* will be used.

### DefaultHeaderName (default: "X-Correlation-Id")
This property is used as header name both when the correlationId is write to the response or is forwarded to other http clients.

### Custom services
You can customize the middleware using custom implementations for some interfaces.

#### ICorrelationService
This service is used for setting and retrieving the correlationId. the default implementation store the correlationId in memory.

#### ICorrelationIdProvider
This service is used for generating a new correlationId. The default implementation generate use Guid as correlationIds.

### Forward correlationId to other services
You can forward your correlationId to other services called using HttpClient. This can help you to trace the same request from the original client through all the services involved.

#### **`startup.cs`**
```csharp

//Forward the correlationId to MyClient using DefaultHeaderName property
services.AddHttpClient("MyClient").ForwardCorrelationId();

//Forward the correlationId to MyOtherClient using another header name
services.AddHttpClient("MyOtherClient").ForwardCorrelationId("X-Another-Header-Name");

```