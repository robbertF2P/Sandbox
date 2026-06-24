FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY build/Platform.Logging.Versions.props build/
COPY build/Platform.Logging.Host.props build/
COPY build/Platform.Logging.Library.props build/
COPY build/Platform.Logging.Akka.props build/
COPY local-feed/ local-feed/

COPY Platform.ControlPlane.Contracts/Platform.ControlPlane.Contracts.csproj Platform.ControlPlane.Contracts/
COPY AdminBackoffice/Directory.Build.props AdminBackoffice/
COPY AdminBackoffice/Directory.Packages.props AdminBackoffice/
COPY AdminBackoffice/NuGet.config AdminBackoffice/
COPY AdminBackoffice/host/AdminBackoffice.Host/AdminBackoffice.Host.csproj AdminBackoffice/host/AdminBackoffice.Host/
COPY AdminBackoffice/src/ControlPlane.Api/ControlPlane.Api.csproj AdminBackoffice/src/ControlPlane.Api/
COPY AdminBackoffice/src/ControlPlane.Application/ControlPlane.Application.csproj AdminBackoffice/src/ControlPlane.Application/
COPY AdminBackoffice/src/ControlPlane.Contracts/ControlPlane.Contracts.csproj AdminBackoffice/src/ControlPlane.Contracts/
COPY AdminBackoffice/src/ControlPlane.Core/ControlPlane.Core.csproj AdminBackoffice/src/ControlPlane.Core/
COPY AdminBackoffice/src/ControlPlane.Data.Migrations/ControlPlane.Data.Migrations.csproj AdminBackoffice/src/ControlPlane.Data.Migrations/
COPY AdminBackoffice/src/ControlPlane.Infrastructure/ControlPlane.Infrastructure.csproj AdminBackoffice/src/ControlPlane.Infrastructure/

WORKDIR /src/AdminBackoffice
RUN dotnet restore host/AdminBackoffice.Host/AdminBackoffice.Host.csproj

COPY Platform.ControlPlane.Contracts/ /src/Platform.ControlPlane.Contracts/
COPY AdminBackoffice/ /src/AdminBackoffice/

RUN dotnet publish host/AdminBackoffice.Host/AdminBackoffice.Host.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV Kestrel__Endpoints__Http__Url=http://0.0.0.0:5090

COPY --from=build /app/publish .

EXPOSE 5090
ENTRYPOINT ["dotnet", "AdminBackoffice.Host.dll"]
