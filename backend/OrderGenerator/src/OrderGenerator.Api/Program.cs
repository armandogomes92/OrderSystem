using System.Text.Json.Serialization;
using OrderGenerator.Api.Endpoints;
using OrderGenerator.Api.Fix;
using OrderGenerator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<PendingOrderRegistry>();
builder.Services.AddSingleton<GeneratorFixApp>();
builder.Services.AddSingleton<IOrderSender, FixOrderSender>();
builder.Services.AddHostedService<FixInitiatorService>();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:5173").AllowAnyHeader().WithMethods("POST")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.MapOrderEndpoints();

app.Run();