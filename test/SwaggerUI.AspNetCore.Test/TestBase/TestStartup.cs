using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace SwaggerUI.AspNetCore.Test.TestBase;

#region Base

public interface ITestStartup
{
    #region Public 方法

    WebApplication Build(params string[] args);

    #endregion Public 方法
}

public abstract class TestStartup : ITestStartup
{
    #region Protected 方法

    protected virtual WebApplication ConfigureWebApplication(WebApplication app)
    {
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            await context.Response.WriteAsync("Hello World");
        });

        return app;
    }

    protected abstract WebApplicationBuilder ConfigureWebApplicationBuilder(WebApplicationBuilder builder);

    protected virtual WebApplicationBuilder CreateWebApplicationBuilder(params string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseTestServer();

        return builder;
    }

    #endregion Protected 方法

    #region Public 方法

    public virtual WebApplication Build(params string[] args)
    {
        var builder = CreateWebApplicationBuilder(args);
        builder = ConfigureWebApplicationBuilder(builder);
        var app = builder.Build();
        return ConfigureWebApplication(app);
    }

    #endregion Public 方法
}

#endregion Base
