using WithStringLocalizer;
using WithStringLocalizer.Swagger;

var builder = WebApplication.CreateBuilder(args);

var supportedCultures = new[] { "en", "ru" };
builder.Services.Configure<RequestLocalizationOptions>(options => {
  options.SetDefaultCulture(supportedCultures[0]);
  options.AddSupportedCultures(supportedCultures);
  options.AddSupportedUICultures(supportedCultures);
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services
  .AddControllers()
  .AddDataAnnotationsLocalization(options => {
    options.DataAnnotationLocalizerProvider = (type, factory) => {
      return factory.Create(typeof(SharedResources));
    };
  });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
  options.OperationFilter<SwaggerLanguageHeader>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseRequestLocalization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
