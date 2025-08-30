using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using SwaggerUI.AspNetCore.Test.TestBase;

namespace SwaggerUI.AspNetCore.Test;

[TestClass]
public class CustomRoutePrefix1SwaggerUIResourcesTests : SwaggerUIResourcesTests
{
    #region Protected 属性

    protected override string RoutePrefix => "/hello";

    #endregion Protected 属性
}

[TestClass]
public class CustomRoutePrefix2SwaggerUIResourcesTests : SwaggerUIResourcesTests
{
    #region Protected 属性

    protected override string RoutePrefix => "/hello/world";

    #endregion Protected 属性
}

[TestClass]
public class SwaggerUIResourcesTests : TestServerBaseTest
{
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
        Assert.AreEqual(TimeSpan.FromDays(1), response.Headers.CacheControl.MaxAge);
    }

    [TestMethod]
    public async Task Should_Returns_ExpectedAssetContents()
    {
        using var client = GetTestHttpClient();

        var embeddedUIFiles = GetEmbeddedUIFiles();
        Assert.IsNotEmpty(embeddedUIFiles);

        foreach (var (resourceName, fileName) in embeddedUIFiles)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetResourceFullPath(fileName));
            using var htmlResponse = await client.SendAsync(requestMessage);
            Assert.AreEqual(HttpStatusCode.OK, htmlResponse.StatusCode);

            using var stream = await htmlResponse.Content.ReadAsStreamAsync();
            using var diskFileStream = typeof(SwaggerUIResourcesTests).Assembly.GetManifestResourceStream(resourceName);

            Assert.IsNotNull(diskFileStream);
            CollectionAssert.AreEqual(MD5.HashData(diskFileStream), MD5.HashData(stream));
        }
    }

    [TestMethod]
    public async Task Should_Returns_ExpectedAssetContents_GZipDirectly()
    {
        using var client = GetTestHttpClient();

        var embeddedUIFiles = GetEmbeddedUIFiles();
        Assert.IsNotEmpty(embeddedUIFiles);

        foreach (var (resourceName, fileName) in embeddedUIFiles)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetResourceFullPath(fileName));
            requestMessage.Headers.AcceptEncoding.Add(new("gzip"));

            using var htmlResponse = await client.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.AreEqual("gzip", htmlResponse.Content.Headers.ContentEncoding.Single());

            using var stream = await htmlResponse.Content.ReadAsStreamAsync();
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            using var diskFileStream = typeof(SwaggerUIResourcesTests).Assembly.GetManifestResourceStream(resourceName);

            Assert.IsNotNull(diskFileStream);
            CollectionAssert.AreEqual(MD5.HashData(diskFileStream), MD5.HashData(gzipStream));
        }
    }

    [TestMethod]
    public async Task Should_Returns_ExpectedAssetContents_NotModified()
    {
        using var client = GetTestHttpClient();

        var embeddedUIFiles = GetEmbeddedUIFiles();
        Assert.IsNotEmpty(embeddedUIFiles);

        foreach (var (_, fileName) in embeddedUIFiles)
        {
            using var htmlResponse = await client.GetAsync(GetResourceFullPath(fileName));
            Assert.AreEqual(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.IsNotNull(htmlResponse.Headers.ETag?.Tag);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetResourceFullPath(fileName));
            requestMessage.Headers.IfNoneMatch.Add(new(htmlResponse.Headers.ETag.Tag));

            using var secondHtmlResponse = await client.SendAsync(requestMessage);
            Assert.AreEqual(HttpStatusCode.NotModified, secondHtmlResponse.StatusCode);
            Assert.AreEqual(0, secondHtmlResponse.Content.ReadAsStream().Length);

            using var stream = await secondHtmlResponse.Content.ReadAsStreamAsync();
            Assert.AreEqual(0, stream.Length);
        }
    }

    #endregion Public 方法
}
