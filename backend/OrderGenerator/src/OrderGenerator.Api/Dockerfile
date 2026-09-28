FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY OrderGenerator/src/OrderGenerator.Core/OrderGenerator.Core.csproj OrderGenerator/src/OrderGenerator.Core/
COPY OrderGenerator/src/OrderGenerator.Api/OrderGenerator.Api.csproj OrderGenerator/src/OrderGenerator.Api/
RUN dotnet restore OrderGenerator/src/OrderGenerator.Api/OrderGenerator.Api.csproj
COPY OrderGenerator/src/ OrderGenerator/src/
RUN dotnet publish OrderGenerator/src/OrderGenerator.Api/OrderGenerator.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
USER root
RUN mkdir store log && chown app:app store log
USER app
ENTRYPOINT ["dotnet", "OrderGenerator.Api.dll"]
