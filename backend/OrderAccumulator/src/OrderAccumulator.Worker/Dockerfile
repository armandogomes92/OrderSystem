FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY OrderAccumulator/src/OrderAccumulator.Core/OrderAccumulator.Core.csproj OrderAccumulator/src/OrderAccumulator.Core/
COPY OrderAccumulator/src/OrderAccumulator.Worker/OrderAccumulator.Worker.csproj OrderAccumulator/src/OrderAccumulator.Worker/
RUN dotnet restore OrderAccumulator/src/OrderAccumulator.Worker/OrderAccumulator.Worker.csproj
COPY OrderAccumulator/src/ OrderAccumulator/src/
RUN dotnet publish OrderAccumulator/src/OrderAccumulator.Worker/OrderAccumulator.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .
USER root
RUN mkdir store log && chown app:app store log
USER app
ENTRYPOINT ["dotnet", "OrderAccumulator.Worker.dll"]
