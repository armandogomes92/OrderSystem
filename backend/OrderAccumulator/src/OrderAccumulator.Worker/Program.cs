using OrderAccumulator.Worker.Services;
using OrderAccumulator.Worker.Fix;
using OrderAccumulator.Core.Exposure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ExposureCalculator>();
builder.Services.AddSingleton<AccumulatorFixApp>();
builder.Services.AddHostedService<FixAcceptorService>();

var host = builder.Build();
host.Run();
