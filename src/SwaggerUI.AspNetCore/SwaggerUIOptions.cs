namespace SwaggerUI.AspNetCore;

/// <summary>
/// swagger ui options
/// </summary>
public class SwaggerUIOptions
{
    #region Public 字段

    /// <summary>
    /// default openapi endpoint
    /// </summary>
    public const string DefaultOpenApiEndpoint = "/openapi/v1.json";

    /// <summary>
    /// default swagger entrypoint path
    /// </summary>
    public const string DefaultRoutePrefix = "/swagger";

    #endregion Public 字段

    #region Public 属性

    /// <summary>
    /// Static resource HTTP cache time
    /// </summary>
    public TimeSpan? CacheLifetime { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// Custom javascript code snippet run before create swagger-ui.
    /// <br/>It will be embedded into 'swagger-initializer.js'
    /// </summary>
    public string? CustomCodeBeforeInitialization { get; set; }

    /// <summary>
    /// A string of a javascript object to custom swagger-ui configuration.
    /// <br/>It will be embedded into 'swagger-initializer.js'
    /// <br/>If it is different from other previous options, it will overwrite the previous options.
    /// <br/><br/>see: <a href="https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/"/>
    /// <br/><br/>example:
    /// <br/>{
    /// <br/>  urls: [
    /// <br/>    {
    /// <br/>      url: "http://127.0.0.1:8080/openapi/v1.json", name: "doc1"
    /// <br/>    },
    /// <br/>    {
    /// <br/>      url: "http://127.0.0.1:5000/openapi/v1.json", name: "doc2"
    /// <br/>    }
    /// <br/>  ]
    /// <br/>}
    /// </summary>
    public string? CustomConfigurationObject { get; set; }

    /// <summary>
    /// openapi doc endpoints.
    /// <br/><br/>see openapi document: <a href="https://aka.ms/aspnet/openapi" /> to generate the doc.
    /// <br/><br/>based on the content, it will be set as the 'url' or 'urls' for swagger-ui
    /// <br/><br/>when not set, use <see cref="DefaultOpenApiEndpoint"/> as default item internally
    /// </summary>
    public HashSet<OpenApiEndpointDescriptor> OpenApiEndpoints { get; set; } = [];

    /// <summary>
    /// swagger entrypoint path
    /// <br/>default with <see cref="DefaultRoutePrefix"/>
    /// </summary>
    public string RoutePrefix { get; set; } = DefaultRoutePrefix;

    #endregion Public 属性
}

/// <summary>
/// openapi endpoint descriptor
/// </summary>
/// <param name="Address">doc address</param>
/// <param name="Name">name to display</param>
public record class OpenApiEndpointDescriptor(string Address, string? Name)
{
    /// <inheritdoc cref="OpenApiEndpointDescriptor"/>
    public OpenApiEndpointDescriptor(string Address) : this(Address, null) { }

    /// <summary>
    /// The default endpoint <see cref="SwaggerUIOptions.DefaultOpenApiEndpoint"/>
    /// </summary>
    public static OpenApiEndpointDescriptor Default { get; } = new(SwaggerUIOptions.DefaultOpenApiEndpoint, null);

    /// <summary>
    /// implicit convert
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator OpenApiEndpointDescriptor((string Address, string? Name) value) => new(value.Address, value.Name);

    /// <summary>
    /// implicit convert
    /// </summary>
    /// <param name="address"></param>
    public static implicit operator OpenApiEndpointDescriptor(string address) => new(address);
}
