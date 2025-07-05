using System.Net;
using SwaggerUI.AspNetCore.Test.TestBase;

namespace SwaggerUI.AspNetCore.Test;

[TestClass]
public class CustomRoutePrefix1SwaggerUIEntryPointTests : SwaggerUIEntryPointTests
{
    #region Protected 属性

    protected override string RoutePrefix => "/hello";

    #endregion Protected 属性
}

[TestClass]
public class CustomRoutePrefix2SwaggerUIEntryPointTests : SwaggerUIEntryPointTests
{
    #region Protected 属性

    protected override string RoutePrefix => "/hello/world";

    #endregion Protected 属性
}

[TestClass]
public class SwaggerUIEntryPointTests : TestServerBaseTest
{
    #region Public 方法

    [TestMethod]
    public async Task Should_Passing_Through_Middleware()
    {
        using var client = GetTestHttpClient();

        using var response = await client.GetAsync($"/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(FallbackResponseContent, await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    public async Task Should_Redirect_Root_To_IndexHtml(string requestPathSuffix)
    {
        using var client = GetTestHttpClient();

        using var response = await client.GetAsync($"{RoutePrefix}{requestPathSuffix}");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.AreEqual($"{RoutePrefix}/index.html", response.Headers.Location?.ToString());
    }

    #endregion Public 方法
}
