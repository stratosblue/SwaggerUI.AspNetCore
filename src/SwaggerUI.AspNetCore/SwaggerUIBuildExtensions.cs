#pragma warning disable IDE0130

using System.ComponentModel;
using SwaggerUI.AspNetCore;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Swagger build extensions
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class SwaggerUIBuildExtensions
{
    #region Public 方法

    /// <summary>
    /// Map swagger-ui with setup callback <paramref name="optionsSetup"/>
    /// </summary>
    /// <param name="app"></param>
    /// <param name="optionsSetup"></param>
    /// <returns></returns>
    public static IApplicationBuilder MapSwaggerUI(this IApplicationBuilder app, Action<SwaggerUIOptions> optionsSetup)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(optionsSetup);

        var options = new SwaggerUIOptions();
        optionsSetup(options);

        var routePrefix = NormalizeRoutePrefix(options.RoutePrefix);

        app.Map(routePrefix, swaggerUIApp =>
        {
            swaggerUIApp.UseMiddleware<SwaggerUIMiddleware>(options);
        });

        return app;
    }

    /// <summary>
    /// Map swagger-ui with http access path <paramref name="routePrefix"/> and use openapi doc endpoint url <paramref name="openApiEndpoint"/>
    /// <br/><br/>see openapi document: <a href="https://aka.ms/aspnet/openapi" />
    /// <br/><br/>When use swagger. The <paramref name="openApiEndpoint"/> in default should be set as "/swagger/v1/swagger.json"
    /// </summary>
    /// <param name="app"></param>
    /// <param name="routePrefix">swagger-ui http access path</param>
    /// <param name="openApiEndpoint">
    /// openapi doc endpoint url<br/>
    /// <br/>Default value for Microsoft.AspNetCore.OpenApi is "/openapi/v1.json"
    /// <br/>Default value for swagger is "/swagger/v1/swagger.json"
    /// </param>
    /// <returns></returns>
    public static IApplicationBuilder MapSwaggerUI(this IApplicationBuilder app,
                                                   string routePrefix = SwaggerUIOptions.DefaultRoutePrefix,
                                                   string openApiEndpoint = SwaggerUIOptions.DefaultOpenApiEndpoint)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.MapSwaggerUI(options =>
        {
            options.RoutePrefix = NormalizeRoutePrefix(routePrefix);
            options.OpenApiEndpoints.Add(new(openApiEndpoint, null));
        });
    }

    #endregion Public 方法

    #region Private 方法

    private static string NormalizeRoutePrefix(string? routePrefix)
    {
        routePrefix = string.IsNullOrWhiteSpace(routePrefix)
                      ? SwaggerUIOptions.DefaultRoutePrefix
                      : routePrefix.Trim();

        if (!routePrefix.StartsWith('/'))
        {
            routePrefix = $"/{routePrefix}";
        }

        return routePrefix.Length == 1
               ? routePrefix
               : routePrefix.TrimEnd('/');
    }

    #endregion Private 方法
}
