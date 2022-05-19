using AutoMapper;
using CorrelationId;
using CorrelationId.DependencyInjection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json;
using $safeprojectname$.Orchestration;
using $safeprojectname$.Repositories;
using $safeprojectname$.Services;$if$ ($userql$ == True)
using Tense.Rql;$endif$$if$ ($usehal$ == True)
using Tense.Hal;
using $safeprojectname$.Configuration;$endif$

var builder = WebApplication.CreateBuilder(args);

//  Configure the host
builder.Host

    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .UseSerilog((hostingcontext, config) =>
    {
        config.ReadFrom.Configuration(hostingcontext.Configuration);
    });

// Add services to the container.

//  Add versioning
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderVersionReader();
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

//  Add correlation id support
builder.Services.AddDefaultCorrelationId(options =>
{
    options.IncludeInResponse = true;
    options.AddToLoggingScope = true;
});

//  Add CORS support
builder.Services.AddCors();

builder.Services.AddHealthChecks();

builder.Services.AddControllers(config =>
{$if$ ($usehal$ == True )
    config.Filters.Add(new HalFilter());
$endif$$if$ ($userql$ == True )
    config.Filters.Add(new RqlFilter());
$endif$}).AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase); 

$if$ ($useauth$ == True)
builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme,
                    jwtOptions =>
                    {
                        jwtOptions.Authority = builder.Configuration.GetSection("OAuth2").GetValue<string>("AuthorityUrl");
                        jwtOptions.Audience = builder.Configuration.GetSection("OAuth2").GetValue<string>("Audience");
                    },
                    referenceOptions =>
                    {
                        referenceOptions.Authority = builder.Configuration.GetSection("OAuth2").GetValue<string>("AuthorityUrl");
                        referenceOptions.ClientId = builder.Configuration.GetSection("OAuth2").GetValue<string>("IntrospectionClient");
                        referenceOptions.ClientSecret = builder.Configuration.GetSection("OAuth2").GetValue<string>("IntrospectionSecret");
                    });

builder.Services.AddAuthorization(options =>
{
    foreach (var policy in builder.Configuration.GetSection("OAuth2").GetSection("Policies").GetChildren())
    {
        var scopes = new List<string>();

        foreach (var scope in policy.GetSection("Scopes").GetChildren())
            scopes.Add(scope.Value);

        options.AddPolicy(policy.GetValue<string>("Policy"), builder =>
        {
            builder.RequireAuthenticatedUser();
            builder.RequireClaim("scope", scopes.ToArray());
        });
    }
});$endif$

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IOrchestrator, Orchestrator>();
builder.Services.AddSingleton<IMapper>(new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly())).CreateMapper());$if$ ($usehal$ == True)
builder.Services.AddSingleton<IHalConfiguration>(new HalConfiguration());$endif$

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
c.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "$safeprojectname$",
    Description = "<description here>",
    Version = "v1",
    Contact = new OpenApiContact
    {
        Name = "$authorname$"$if$ ($emailaddress$ != none),
        Email = "$emailaddress$"$endif$$if$ ($website$ != none),
        Url = new Uri("$website$")$endif$
    }
});

c.DescribeAllParametersInCamelCase();

    $if$ ($useauth$ == True )
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "IDS Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Name= "Bearer",
                In = ParameterLocation.Header,
            },
            Array.Empty<string>()
        }
    });$endif$

#pragma warning disable CS8604 // Possible null reference argument.
    c.IncludeXmlComments(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "$safeprojectname$.xml"));
#pragma warning restore CS8604 // Possible null reference argument.
});

var app = builder.Build();

app.UseCorrelationId();
app.UseCors(builder => builder.AllowAnyMethod()
                              .AllowAnyHeader()
                              .SetIsOriginAllowed(origin => true)
                              .AllowCredentials()
                              .WithExposedHeaders(new string[] { "location" }));

$if$ ($userql$ == True)app.UseRql();
$endif$app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "$safeprojectname$ V1");
    options.RoutePrefix = string.Empty;
});

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();$if$ ($useauth$ == True )
app.UseAuthorization();$endif$

app.MapControllers();
app.UseHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => false,    //  Prevents this endpoint from checking teh StayAliveHealthCheck
    ResponseWriter = HealthResponseWriter.Format
});
app.Run();
