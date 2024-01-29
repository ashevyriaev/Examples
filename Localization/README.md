# Localization

## Globalization settings

By default application setup [`System.Globalization.CultureInfo`](https://learn.microsoft.com/ru-ru/dotnet/api/system.globalization.cultureinfo.currentculture), [`System.Globalization.CultureUIInfo`](https://learn.microsoft.com/ru-ru/dotnet/api/system.globalization.cultureinfo.currentuiculture), [`Thread.CurrentThread.CurrentCulture`](https://learn.microsoft.com/ru-ru/dotnet/api/system.threading.thread.currentthread) and [`Thread.CurrentThread.CurrentUICulture`](https://learn.microsoft.com/ru-ru/dotnet/api/system.threading.thread.currentuiculture) values based on current system settings. It cant be changed programmically by setting values for [`System.Globalization.DefaultThreadCurrentCulture`](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.defaultthreadcurrentculture) and [`System.Globalization.DefaultThreadCurrentUICulture`](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.defaultthreadcurrentuiculture).

When [`ApplicationBuilderExtensions.UseRequestLocalization`](https://learn.microsoft.com/ru-ru/dotnet/api/microsoft.aspnetcore.builder.applicationbuilderextensions.userequestlocalization) is used, default localization settings and settings made with [`System.Globalization.DefaultThreadCurrentCulture`](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.defaultthreadcurrentculture) and [`System.Globalization.DefaultThreadCurrentUICulture`](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.defaultthreadcurrentuiculture) will be overriden with request's *Accept-Language* header value.

## Resource file naming

> Resources are named for the full type name of their class minus the assembly name. For example, a French resource in a project whose main assembly is `LocalizationWebsite.Web.dll` for the class `LocalizationWebsite.Web.Startup` would be named *Startup.fr.resx*. A resource for the class `LocalizationWebsite.Web.Controllers.HomeController` would be named *Controllers.HomeController.fr.resx*. If your targeted class's namespace isn't the same as the assembly name you will need the full type name. For example, in the sample project a resource for the type `ExtraNamespace.Tools` would be named *ExtraNamespace.Tools.fr.resx*. [[1]](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/provide-resources?#resource-file-naming)

If resource file for language not found, ResourceManager will try to find ***default*** resource file. Naming for default resources files are the same as for localized resource file mius language string specification. If name for French resource file is *Controllers.HomeController.fr.resx* name for ***default*** resource file would be *Controllers.HomeController.resx*. If ***default*** resource can't be found either than ResourceManager will use ***resource name*** as localized value.

<image src="Readme/resource_name.png" alt="resource name">

***Resource name*** will be used as localized value if resource with given name can't be found in existing resource files.

## Setup request localization

```csharp
// Program.cs

...
var supportedCultures = new[] { "en", "ru" };
builder.Services.Configure<RequestLocalizationOptions>(options => {
  options.SetDefaultCulture(supportedCultures[0]);
  options.AddSupportedCultures(supportedCultures);
  options.AddSupportedUICultures(supportedCultures);
});
...
app.UseRequestLocalization();
...
```
> [!NOTE]
> `DefaultCulture` will be used when *Accept-Language* header not set or set with value that not present in `SupportedCultures` list. If request is submitted via ***Swagger UI***, browser will automatically add *Accept-Language* header value, according to current browser settings.

## Disable project's Invariant Globalization
If Invariant Globalization is enabled in project settings, an exception will be raised: 

<image src="Readme/globalization_exception.png" alt="globalization exception">

To disable project's Invariant Globalization we need to set *InvariantGlobalization* option in project file to false:

<image src="Readme/project_settings.png" alt="project settings">

## Add Accept-Language header to Swagger UI

```csharp
// Swagger/SwaggerLanguageHeader.cs

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WithResourceManager.Swagger {
  public class SwaggerLanguageHeader : IOperationFilter {
    private readonly IServiceProvider _serviceProvider;

    public SwaggerLanguageHeader(IServiceProvider serviceProvider) {
      _serviceProvider = serviceProvider;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
      operation.Parameters ??= new List<OpenApiParameter>();

      operation.Parameters.Add(new OpenApiParameter {
        Name = "Accept-Language",
        Description = "Supported languages",
        In = ParameterLocation.Header,
        Required = false,
        Schema = new OpenApiSchema {
          Type = "string",
          Enum = (
            _serviceProvider
            .GetService(typeof(IOptions<RequestLocalizationOptions>)) as IOptions<RequestLocalizationOptions>)
            ?.Value
            ?.SupportedCultures
            ?.Select(c => new OpenApiString(c.TwoLetterISOLanguageName))
            .ToList<IOpenApiAny>(),
        }
      });
    }
  }
}

```
```csharp
// Program.cs
...
builder.Services.AddSwaggerGen(options => {
  options.OperationFilter<SwaggerLanguageHeader>();
});
...
```

<image src="Readme/swagger_language_header.png" alt="swagger language header">

## Localizatioin with [`System.Resources.ResourceManager`](https://learn.microsoft.com/ru-ru/dotnet/api/system.resources.resourcemanager)

Add ***default***(*Resources/SharedResources.resx* and *Resources/Controllers/AuthController.resx*) resource files:

<image src="Readme/add_resource_files.png" alt="add default resource files">

Add localized(*Resources/SharedResources.ru.resx* and *Resources/Controllers/AuthController.ru.resx*) resource files:

<image src="Readme/add_resource_files2.png" alt="add localized resource files">

Make ***default***(*Resources/SharedResources.resx* and *Resources/Controllers/AuthController.resx*) resource files public:

<image src="Readme/add_resource_files3.png" alt="make default files public">

Now we can use resource localization for [`System.ComponentModel.DataAnnotations`](https://learn.microsoft.com/ru-ru/dotnet/api/system.componentmodel.dataannotations):

```csharp
// DTO/LoginDTO.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WithResourceManager.DTO {
  public class LoginDTO {
    [Required(
      ErrorMessageResourceType = typeof(Resources.SharedResources),
      ErrorMessageResourceName = "errorRequired")]
    [EmailAddress(
      ErrorMessageResourceType = typeof(Resources.SharedResources),
      ErrorMessageResourceName = "errorEmail")]
    [DefaultValue("")]
    [Display(
      Name = "email",
      ResourceType = typeof(Resources.SharedResources))]
    public string Email { get; set; } = "";

    [Required(
      ErrorMessageResourceType = typeof(Resources.SharedResources),
      ErrorMessageResourceName = "errorRequired")]
    [Length(8, 32,
      ErrorMessageResourceType = typeof(Resources.SharedResources),
      ErrorMessageResourceName = "errorStringLength")]
    [RegularExpression("^[a-zA-Z0-9]*$",
      ErrorMessageResourceType = typeof(Resources.SharedResources),
      ErrorMessageResourceName = "errorRegularExpression")]
    [DefaultValue("")]
    [Display(
      Name = "password",
      ResourceType = typeof(Resources.SharedResources))]
    public string Password { get; set; } = "";
  }
}
```
and for localize controller's strings:
```csharp
// Controllers/AuthController.cs

using Microsoft.AspNetCore.Mvc;
using WithResourceManager.DTO;

namespace WithResourceManager.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase {
    public AuthController() { }

    [HttpPost("login")]
    public string Login(LoginDTO loginDTO) {
      return Resources
        .Controllers
        .AuthController
        .messageUserLoggedin
        .Replace("{0}", loginDTO.Email);
    }
  }
}


```

## Localizatioin with [`Microsoft.Extensions.Localization.IStringLocalizer`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.localization.istringlocalizer)

Add ***default***(*Resources/SharedResources.resx* and *Resources/Controllers/AuthController.resx*) resource files:

<image src="Readme/add_resource_files.png" alt="add default resource files">

Add localized (*Resources/SharedResources.ru.resx* and *Resources/Controllers/AuthController.ru.resx*) resource files:

<image src="Readme/add_resource_files2.png" alt="add localized resource files">

To use *Resources/SharedResources.resx* and *Resources/SharedResources.ru.resx* files we need to add the marker class `SharedResources`(this class will be used only for setup [`Microsoft.Extensions.Localization.IStringLocalizer`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.localization.istringlocalizer)):

```csharp
// SharedResources.cs

namespace WithStringLocalizer {
  public class SharedResources {
  }
}
```

Setup [`Microsoft.Extensions.Localization.IStringLocalizer`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.localization.istringlocalizer) for Dependency Injection with [`Microsoft.Extensions.DependencyInjection.LocalizationServiceCollectionExtensions.AddLocalization`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.localizationservicecollectionextensions.addlocalization) extension method:

```csharp
// Program.cs

...
builder.Services.AddLocalization(options => options.ResourcesPath = 
"Resources");
...
```
> [!NOTE]
> IStringLocalizer will search resource files according to `ResourcesPath`. For instence for class `WithStringLocalizer.Controllers.AuthController` IStringLocalzer search files *Resources/Controllers/AuthComtroller.resx* and *Resources/Controllers.AuthController.resx* etc.  If `ResourcesPath` is not set, then IStringLocalzer search files in root project directory *Controllers/AuthComtroller.resx* and *Controllers.AuthController.resx* etc.. See [[resource file naming]](#resource-file-naming) for resources naming details.

> [!WARNING]
>  If `ResourcesPath` is set then resource files have to be placed separate form class definition: <br><br> <image src="Readme/wrong_placing.png" alt="wrong class file placing"> <br><br> <image src="Readme/right_placing.png" alt="right class file placing">

Setup [`Microsoft.Extensions.Localization.IStringLocalizer`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.localization.istringlocalizer) for [`System.ComponentModel.DataAnnotations`](https://learn.microsoft.com/ru-ru/dotnet/api/system.componentmodel.dataannotations) with [`Microsoft.Extensions.DependencyInjection.AddDataAnnotationsLocalization`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.mvcdataannotationsmvcbuilderextensions.adddataannotationslocalization) extension method:

```csharp
// Program.cs

...
builder.Services
  .AddControllers()
  .AddDataAnnotationsLocalization(options => {
    options.DataAnnotationLocalizerProvider = (type, factory) => {
      return factory.Create(typeof(SharedResources));
    };
  });
...  
```
> [!NOTE] 
> `ResourcesPath` that is set with [`System.ComponentModel.DataAnnotations`](https://learn.microsoft.com/ru-ru/dotnet/api/system.componentmodel.dataannotations) extension method also affects [`System.ComponentModel.DataAnnotations`](https://learn.microsoft.com/ru-ru/dotnet/api/system.componentmodel.dataannotations) localization.

Now we can use resource localization for [`System.ComponentModel.DataAnnotations`](https://learn.microsoft.com/ru-ru/dotnet/api/system.componentmodel.dataannotations):

```csharp
// DTO/LoginDTO.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WithStringLocalizer.DTO {
  public class LoginDTO {
    [Required(ErrorMessage = "errorRequired")]
    [EmailAddress(ErrorMessage = "errorEmail")]
    [DefaultValue("")]
    [Display(Name = "email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "errorRequired")]
    [Length(8, 32, ErrorMessage = "errorStringLength")]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "errorRegularExpression")]
    [DefaultValue("")]
    [Display(Name = "password")]
    public string Password { get; set; } = "";
  }
}
```

and for localize controller's strings:

```csharp
// Controllers/AuthController.cs

namespace WithStringLocalizer.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase {
    private IStringLocalizer<AuthController> _stringLocalizer;

    public AuthController(IStringLocalizer<AuthController> stringLocalizer) =>
      _stringLocalizer = stringLocalizer;

    [HttpPost("login")]
    public string Login(LoginDTO loginDTO) {
      return _stringLocalizer["messageUserLoggedin"].ToString().Replace("{0}", loginDTO.Email);
    }
  }
}
```

> [!WARNING]
> In .Net 8.0 [`System.ComponentModel.DataAnnotation`](https://learn.microsoft.com/ru-ru/dotnet/api/system.componentmodel.dataannotations) `[Lenght]` attribute not localize properly with [`Microsoft.Extensions.Localization.IStringLocalizer`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.localization.istringlocalizer) method:<br> <image src="Readme/length_attribute_error.png" alt="length attribute"> <br> But it work just fine with [`System.Resources.ResourceManager`](#localizatioin-with-systemresourcesresourcemanager) localization method.

## External links:
1. [Globalization and localization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization)
1. [What every ASP.NET Core Web API project needs - Part 4 - Error Message Reusability and Localization](https://dev.to/moesmp/what-every-asp-net-core-web-api-project-needs-part-4-error-message-reusability-and-localization-229i)
1. [Localization of the DTOs in a separate assembly in ASP.NET Core](https://dejanstojanovic.net/aspnet/2019/april/localization-of-the-dtos-in-a-separate-assembly-in-aspnet-core/)
