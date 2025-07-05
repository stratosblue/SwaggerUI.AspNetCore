using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace SwaggerUI.AspNetCore.Test.TestBase;

public abstract class TestServerBaseTest
{
    #region Protected 字段

    protected IServiceProvider RootServiceProvider = null!;

    protected AsyncServiceScope ServiceScope;

    protected TestServer TestServer = null!;

    protected WebApplication WebApplication = null!;

    #endregion Protected 字段

    #region Protected 属性

    protected virtual string FallbackResponseContent { get; } = "Hello World";

    protected virtual string RoutePrefix { get; } = SwaggerUIOptions.DefaultRoutePrefix;

    protected IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

    #endregion Protected 属性

    #region Public 方法

    [TestCleanup]
    public async Task TestCleanupAsync()
    {
        await WebApplication.StopAsync();
        await ServiceScope.DisposeAsync();
    }

    [TestInitialize]
    public async Task TestInitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        ConfigureServices(builder.Services);

        WebApplication = builder.Build();

        ConfigureWebApplication(WebApplication);

        WebApplication.Use(async (HttpContext context, RequestDelegate _) =>
        {
            await context.Response.WriteAsync(FallbackResponseContent);
        });

        await WebApplication.StartAsync();

        TestServer = WebApplication.GetTestServer();

        RootServiceProvider = WebApplication.Services;
        ServiceScope = RootServiceProvider.CreateAsyncScope();
    }

    #endregion Public 方法

    #region Protected 方法

    protected virtual void ConfigureServices(IServiceCollection services)
    { }

    protected virtual void ConfigureWebApplication(WebApplication application)
    {
        application.MapSwaggerUI(routePrefix: RoutePrefix);
    }

    protected string GetResourceFullPath(string resourcePath) => $"{RoutePrefix}/{resourcePath}";

    protected HttpClient GetTestHttpClient() => TestServer.CreateClient();

    #endregion Protected 方法
}
