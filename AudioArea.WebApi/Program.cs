using AudioArea.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters; // IOutputFormatter, OutputFormatter
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Packt.Shared; // AddAudioContext extension method
using Microsoft.AspNetCore.HttpLogging; // HttpLoggingFields
using System.Runtime.Serialization;
using System.Xml;
using static System.Console;

var builder = WebApplication.CreateBuilder(args);

string? sqlServerConnection = builder.Configuration
  .GetConnectionString("DefaultConnection");

if (sqlServerConnection is null)
{
    Console.WriteLine("SQL Server database connection string is missing!");
}
else
{
    builder.Services.AddAudioContext(sqlServerConnection);
}

builder.Services.AddControllers()
.AddXmlDataContractSerializerFormatters()
.AddXmlSerializerFormatters()
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096; // default is 32k
    options.ResponseBodyLogLimit = 4096; // default is 32k
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
