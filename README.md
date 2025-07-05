# SwaggerUI.AspNetCore

A [swagger-ui](https://github.com/swagger-api/swagger-ui) [dist](https://www.npmjs.com/package/swagger-ui-dist) package for aspnetcore. Let the aspnetcore program provide swagger-ui endpoint.

- Provides minimizing and original swagger-ui
- Lightweight and fast (Pre gzip and simple mapping)
- Pre configured for `Microsoft.AspNetCore.OpenApi`

## How to use
- Install package
  ```shell
  dotnet add package SwaggerUI.AspNetCore
  ```
- Setup with code

  Add `MapSwaggerUI` into request pipeline.
  ```
  app.MapSwaggerUI();
  ```

Now it will work with `Microsoft.AspNetCore.OpenApi` (`aspnetcore api` template version of `.net9` or higher). Now access `/swagger`, you will see swagger-ui.

## Configure

### simple configuration
```C#
app.MapSwaggerUI(routePrefix: "/swagger",
                 openApiEndpoint: "/openapi/v1.json");
```
- `routePrefix` set the swagger-ui access path
- `openApiEndpoint` set the openapi doc access path
  - `aspnetcore api` template in `.net9` default is `/openapi/v1.json`
  - Support `relative path` or `absolute path`
    - If use `swagger`. Normally, the path is `/swagger/v1/swagger.json`
    - If set to a non-same-origin `absolute path`, `CORS` is required

### full configuration
```C#
app.MapSwaggerUI(options =>
{
    options.RoutePrefix = "/swagger";

    //support named and multi openapi endpoint
    options.OpenApiEndpoints.Add("/openapi/v1.json");

    //Javascript code run before swagger-ui initialization
    options.CustomCodeBeforeInitialization =
"""
alert("Hello");
""";

    //Javascript object to set and override all configuration before
    options.CustomConfigurationObject =
"""
{
  url: null,
  urls: [ { url: "/openapi/v1.json", name: "default"}]
}
""";
});
```

Support full swagger-ui configuration by set `Javascript object code` for `CustomConfigurationObject`

More configuration references: [swagger-ui configuration doc](https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/)
