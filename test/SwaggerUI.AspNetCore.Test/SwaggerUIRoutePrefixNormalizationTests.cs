using System.Net;
using Microsoft.AspNetCore.Builder;
using SwaggerUI.AspNetCore.Test.TestBase;

namespace SwaggerUI.AspNetCore.Test;

[TestClass]
public class SwaggerUIRoutePrefixNormalizationTests : TestServerBaseTest
{
    #region Public 方法

    [TestMethod]
    public async Task Should_MapSwaggerUI_WhenRoutePrefixHasNoLeadingSlash()
    {
        using var client = GetTestHttpClient();

        using var response = await client.GetAsync("/custom/index.html", TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Should_MapSwaggerUI_WhenRoutePrefixHasTrailingSlash()
    {
        using var client = GetTestHttpClient();

        using var response = await client.GetAsync("/custom/index.html", TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Should_MapSwaggerUI_WhenRoutePrefixIsWhitespace()
    {
        using var client = GetTestHttpClient();

        using var response = await client.GetAsync($"{SwaggerUIOptions.DefaultRoutePrefix}/index.html", TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureWebApplication(WebApplication application)
    {
        application.MapSwaggerUI(options =>
        {
            options.OpenApiEndpoints.Add("/openapi/v1.json");
        });

        application.MapSwaggerUI(routePrefix: "custom", openApiEndpoint: "/openapi/v1.json");
        application.MapSwaggerUI(routePrefix: "/custom/", openApiEndpoint: "/openapi/v1.json");
        application.MapSwaggerUI(routePrefix: "   ", openApiEndpoint: "/openapi/v1.json");
    }


    #endregion Protected 方法
}
