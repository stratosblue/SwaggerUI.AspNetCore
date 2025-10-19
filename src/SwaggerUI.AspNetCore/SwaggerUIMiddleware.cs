using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

using SwaggerUI.AspNetCore.Internal;

namespace SwaggerUI.AspNetCore;

internal sealed class SwaggerUIMiddleware
{
    #region Private 字段

    private const string EmbeddedFileNamespace = "SwaggerUI.AspNetCore.swagger_dist";

    private readonly CompressedEmbeddedFileResponder _compressedEmbeddedFileResponder;

    private readonly StringValues _javascriptContentsType = "text/javascript";

    private readonly RequestDelegate _next;

    private readonly ReadOnlyMemory<byte> _swaggerInitializerJSCodeData;

    #endregion Private 字段

    #region Public 构造函数

    public SwaggerUIMiddleware(RequestDelegate next, SwaggerUIOptions options, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(options);

        _next = next;
        var compressionEnabled = options.CompressionEnabled is null
                                 ? !hostEnvironment.IsDevelopment()
                                 : options.CompressionEnabled.Value;

        var assembly = typeof(SwaggerUIMiddleware).Assembly;
        _compressedEmbeddedFileResponder = new(assembly: assembly,
                                               resourceNamePrefix: EmbeddedFileNamespace,
                                               cacheLifetime: options.CacheLifetime,
                                               compressionEnabled: compressionEnabled);

        var initializerJSCode = GenerateInitializerJSCode(options, assembly);

        _swaggerInitializerJSCodeData = Encoding.UTF8.GetBytes(initializerJSCode);
    }

    #endregion Public 构造函数

    #region Public 方法

    public async Task Invoke(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value;

        //static resources
        if (!await _compressedEmbeddedFileResponder.TryRespondWithFileAsync(httpContext))
        {
            //Redirects similar to /swagger or /swagger/ to /swagger/index.html
            if (string.IsNullOrEmpty(path)
                || string.Equals(path, "/", StringComparison.Ordinal))
            {
                httpContext.Response.StatusCode = StatusCodes.Status301MovedPermanently;
                httpContext.Response.Headers.Location = $"{httpContext.Request.PathBase}/index.html";
                return;
            }

            //Response init script
            if (string.Equals("/swagger-initializer.js", path, StringComparison.Ordinal))
            {
                httpContext.Response.Headers.ContentLength = _swaggerInitializerJSCodeData.Length;
                httpContext.Response.Headers.ContentType = _javascriptContentsType;

                await httpContext.Response.BodyWriter.WriteAsync(_swaggerInitializerJSCodeData, httpContext.RequestAborted);
                return;
            }

            await _next(httpContext);
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static string GenerateInitializerJSCode(SwaggerUIOptions options, Assembly assembly)
    {
        //read templete in embedded resource
        using var stream = assembly.GetManifestResourceStream("SwaggerUI.AspNetCore.swagger-initializer.js")!;
        using var reader = new StreamReader(stream);
        var initializerScriptTemplate = reader.ReadToEnd();

        //configure code with options
        var openApiEndpoints = options.OpenApiEndpoints?.Count > 0
                               ? options.OpenApiEndpoints
                               : [OpenApiEndpointDescriptor.Default];

        string urlsSnippet;
        if (openApiEndpoints.Count > 1
            || !string.IsNullOrWhiteSpace(openApiEndpoints.First().Name))   //as urls
        {
            urlsSnippet = $"urls: [ {string.Join(", ", openApiEndpoints.Select((m, i) => $"{{ url: \"{m.Address}\", name: \"{m.Name ?? $"Unnamed_{i}"}\" }}"))} ]";
        }
        else //as url
        {
            urlsSnippet = $"url: \"{openApiEndpoints.First().Address}\"";
        }

        var optionsSetupSnippet = $$"""
                                  options = { ...options,
                                              {{urlsSnippet}}
                                            };
                                  """;

        var customCodeBeforeInitialization = options.CustomCodeBeforeInitialization ?? string.Empty;
        var customConfigurationSnippet = $"customOptions = {options.CustomConfigurationObject ?? "{}"} ?? {{}};";

        var initializerJSCode = initializerScriptTemplate.Replace("/** OptionsSetupSnippet */", optionsSetupSnippet, StringComparison.Ordinal)
                                                         .Replace("/** CustomCodeBeforeInitialization */", customCodeBeforeInitialization, StringComparison.Ordinal)
                                                         .Replace("/** CustomConfigurationSnippet */", customConfigurationSnippet, StringComparison.Ordinal);
        return initializerJSCode;
    }

    #endregion Private 方法
}
