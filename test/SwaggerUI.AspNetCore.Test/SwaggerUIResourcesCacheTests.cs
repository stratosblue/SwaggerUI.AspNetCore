using System.Net;
using Microsoft.AspNetCore.Builder;
using SwaggerUI.AspNetCore.Test.TestBase;

namespace SwaggerUI.AspNetCore.Test;

[TestClass]
public class CustomCacheLifetime1SwaggerUIResourcesCacheTests : SwaggerUIResourcesCacheTests
{
    #region Protected 属性

    protected override TimeSpan CacheLifetime { get; } = TimeSpan.FromDays(3);

    protected override string RoutePrefix => "/hello";

    #endregion Protected 属性
}

[TestClass]
public class CustomCacheLifetime2SwaggerUIResourcesCacheTests : SwaggerUIResourcesCacheTests
{
    #region Protected 属性

    protected override TimeSpan CacheLifetime { get; } = TimeSpan.FromDays(2);

    protected override string RoutePrefix => "/hello/world";

    #endregion Protected 属性
}

[TestClass]
public class SwaggerUIResourcesCacheTests : TestServerBaseTest
{
    #region Public 属性

    protected virtual TimeSpan CacheLifetime { get; } = TimeSpan.FromDays(7);

    #endregion Public 属性

    #region Public 方法

    [TestMethod]
    [DataRow("favicon-16x16.png")]
    [DataRow("favicon-32x32.png")]
    [DataRow("index.css")]
    [DataRow("index.html")]
    [DataRow("oauth2-redirect.html")]
    [DataRow("swagger-ui.css")]
    [DataRow("swagger-ui.js")]
    [DataRow("swagger-ui-bundle.js")]
    [DataRow("swagger-ui-standalone-preset.js")]
    public async Task Should_Access_Embedded_Resource_Success(string resourcePath)
    {
        using var client = GetTestHttpClient();

        using var response = await client.GetAsync(GetResourceFullPath(resourcePath));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(response.Headers.ETag);
        Assert.IsFalse(response.Headers.ETag.IsWeak);
        Assert.IsNotEmpty(response.Headers.ETag.Tag);
        Assert.IsNotNull(response.Headers.CacheControl);
        Assert.IsTrue(response.Headers.CacheControl.Private);
        Assert.AreEqual(CacheLifetime, response.Headers.CacheControl.MaxAge);
    }

    #endregion Public 方法

    #region Protected 方法

    protected override void ConfigureWebApplication(WebApplication application)
    {
        application.MapSwaggerUI(options =>
        {
            options.RoutePrefix = RoutePrefix;
            options.CacheLifetime = CacheLifetime;
        });
    }

    #endregion Protected 方法
}
