using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using SwaggerUI.AspNetCore.Test.TestBase;

namespace SwaggerUI.AspNetCore.Test;

[TestClass]
public class SwaggerUIResourceNonCompressionTests : TestServerBaseTest
{
    #region Public 方法

    [TestMethod]
    public async Task Should_Returns_ExpectedAssetContentsNonGZip_GZipDirectly()
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
            Assert.IsEmpty(htmlResponse.Content.Headers.ContentEncoding);

            using var stream = await htmlResponse.Content.ReadAsStreamAsync();
            using var diskFileStream = typeof(SwaggerUIResourcesTests).Assembly.GetManifestResourceStream(resourceName);

            Assert.IsNotNull(diskFileStream);
            CollectionAssert.AreEqual(MD5.HashData(diskFileStream), MD5.HashData(stream));
        }
    }

    [TestMethod]
    public async Task Should_Returns_ExpectedAssetContentsNonGZip_NotModified()
    {
        using var client = GetTestHttpClient();

        var embeddedUIFiles = GetEmbeddedUIFiles();
        Assert.IsNotEmpty(embeddedUIFiles);

        foreach (var (_, fileName) in embeddedUIFiles)
        {
            using var requestMessage1 = new HttpRequestMessage(HttpMethod.Get, GetResourceFullPath(fileName));
            requestMessage1.Headers.AcceptEncoding.Add(new("gzip"));

            using var htmlResponse = await client.SendAsync(requestMessage1);
            Assert.AreEqual(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.IsEmpty(htmlResponse.Content.Headers.ContentEncoding);
            Assert.IsNotNull(htmlResponse.Headers.ETag?.Tag);

            using var requestMessage2 = new HttpRequestMessage(HttpMethod.Get, GetResourceFullPath(fileName));
            requestMessage2.Headers.IfNoneMatch.Add(new(htmlResponse.Headers.ETag.Tag));

            using var secondHtmlResponse = await client.SendAsync(requestMessage2);
            Assert.AreEqual(HttpStatusCode.NotModified, secondHtmlResponse.StatusCode);
            Assert.AreEqual(0, secondHtmlResponse.Content.ReadAsStream().Length);

            using var stream = await secondHtmlResponse.Content.ReadAsStreamAsync();
            Assert.AreEqual(0, stream.Length);
        }
    }

    #endregion Public 方法

    #region Private 方法

    protected override void ConfigureWebApplication(WebApplication application)
    {
        application.MapSwaggerUI(options =>
        {
            options.RoutePrefix = RoutePrefix;
            options.CompressionEnabled = false;
        });
    }

    #endregion Private 方法
}
