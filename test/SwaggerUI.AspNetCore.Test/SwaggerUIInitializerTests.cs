using System.Net;
using Microsoft.AspNetCore.Builder;
using SwaggerUI.AspNetCore.Test.TestBase;

namespace SwaggerUI.AspNetCore.Test;

[TestClass]
public class CustomRoutePrefix1SwaggerUIInitializerTests : SwaggerUIInitializerTests
{
    #region Protected 属性

    protected override string RoutePrefix => "/hello";

    #endregion Protected 属性
}

[TestClass]
public class CustomRoutePrefix2SwaggerUIInitializerTests : SwaggerUIInitializerTests
{
    #region Protected 属性

    protected override string RoutePrefix => "/hello/world";

    #endregion Protected 属性
}

[TestClass]
public class SwaggerUIInitializerTests : TestServerBaseTest
{
    #region Public 方法

    [TestMethod]
    public async Task Should_Response_Custom_Data()
    {
        using var client = GetTestHttpClient();

        using var response = await client.GetAsync(GetResourceFullPath("swagger-initializer.js"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        Assert.IsTrue(content.Contains("http://test.com/api.json"));
        Assert.IsTrue(content.Contains("//TestCustomConfigurationObject"));
        Assert.IsTrue(content.Contains("//CustomCodeBeforeInitialization"));
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureWebApplication(WebApplication application)
    {
        application.MapSwaggerUI(options =>
        {
            options.RoutePrefix = RoutePrefix;
            options.OpenApiEndpoints.Add("http://test.com/api.json");
            options.CustomConfigurationObject =
            """
            {
            //TestCustomConfigurationObject
            }
            """;

            options.CustomCodeBeforeInitialization =
            """
            //CustomCodeBeforeInitialization
            """;
        });
    }

    #endregion Protected 方法
}
