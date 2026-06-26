FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY build/Platform.Logging.Versions.props build/
COPY build/Platform.Logging.Host.props build/
COPY build/Platform.Logging.Library.props build/
COPY build/Platform.Logging.Akka.props build/
COPY local-feed/ local-feed/

COPY Platform.ControlPlane.Contracts/Platform.ControlPlane.Contracts.csproj Platform.ControlPlane.Contracts/
COPY F2pPlatform/Directory.Build.props F2pPlatform/
COPY F2pPlatform/Directory.Packages.props F2pPlatform/
COPY F2pPlatform/NuGet.config F2pPlatform/
COPY F2pPlatform/host/F2pPlatform.Host/F2pPlatform.Host.csproj F2pPlatform/host/F2pPlatform.Host/
COPY F2pPlatform/host/F2pPlatform.Host.Contracts/F2pPlatform.Host.Contracts.csproj F2pPlatform/host/F2pPlatform.Host.Contracts/
COPY F2pPlatform/host/F2pPlatform.Host.Core/F2pPlatform.Host.Core.csproj F2pPlatform/host/F2pPlatform.Host.Core/
COPY F2pPlatform/src/Modules/Identity/Identity.Api/Identity.Api.csproj F2pPlatform/src/Modules/Identity/Identity.Api/
COPY F2pPlatform/src/Modules/Identity/Identity.Application/Identity.Application.csproj F2pPlatform/src/Modules/Identity/Identity.Application/
COPY F2pPlatform/src/Modules/Identity/Identity.Domain/Identity.Domain.csproj F2pPlatform/src/Modules/Identity/Identity.Domain/
COPY F2pPlatform/src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj F2pPlatform/src/Modules/Identity/Identity.Infrastructure/
COPY F2pPlatform/src/Modules/PlatformConfig/PlatformConfig.Api/PlatformConfig.Api.csproj F2pPlatform/src/Modules/PlatformConfig/PlatformConfig.Api/
COPY F2pPlatform/src/Modules/PlatformConfig/PlatformConfig.Application/PlatformConfig.Application.csproj F2pPlatform/src/Modules/PlatformConfig/PlatformConfig.Application/
COPY F2pPlatform/src/Modules/PlatformConfig/PlatformConfig.Infrastructure/PlatformConfig.Infrastructure.csproj F2pPlatform/src/Modules/PlatformConfig/PlatformConfig.Infrastructure/
COPY F2pPlatform/src/Modules/Reference/Reference.Api/Reference.Api.csproj F2pPlatform/src/Modules/Reference/Reference.Api/
COPY F2pPlatform/src/Modules/Reference/Reference.Application/Reference.Application.csproj F2pPlatform/src/Modules/Reference/Reference.Application/
COPY F2pPlatform/src/Modules/Reference/Reference.Domain/Reference.Domain.csproj F2pPlatform/src/Modules/Reference/Reference.Domain/
COPY F2pPlatform/src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure.csproj F2pPlatform/src/Modules/Reference/Reference.Infrastructure/
COPY F2pPlatform/src/Modules/HourApprovals/HourApprovals.Api/HourApprovals.Api.csproj F2pPlatform/src/Modules/HourApprovals/HourApprovals.Api/
COPY F2pPlatform/src/Modules/HourApprovals/HourApprovals.Application/HourApprovals.Application.csproj F2pPlatform/src/Modules/HourApprovals/HourApprovals.Application/
COPY F2pPlatform/src/Modules/HourApprovals/HourApprovals.Domain/HourApprovals.Domain.csproj F2pPlatform/src/Modules/HourApprovals/HourApprovals.Domain/
COPY F2pPlatform/src/Modules/HourApprovals/HourApprovals.Infrastructure/HourApprovals.Infrastructure.csproj F2pPlatform/src/Modules/HourApprovals/HourApprovals.Infrastructure/
COPY F2pPlatform/src/Packs/HourApprovals.Packs.Acme/HourApprovals.Packs.Acme.csproj F2pPlatform/src/Packs/HourApprovals.Packs.Acme/
COPY F2pPlatform/src/Shared/Platform.Shared.Domain/Platform.Shared.Domain.csproj F2pPlatform/src/Shared/Platform.Shared.Domain/
COPY F2pPlatform/src/Modules/ControlPlane/ControlPlane.Api/ControlPlane.Api.csproj F2pPlatform/src/Modules/ControlPlane/ControlPlane.Api/
COPY F2pPlatform/src/Modules/ControlPlane/ControlPlane.Application/ControlPlane.Application.csproj F2pPlatform/src/Modules/ControlPlane/ControlPlane.Application/
COPY F2pPlatform/src/Modules/ControlPlane/ControlPlane.Domain/ControlPlane.Domain.csproj F2pPlatform/src/Modules/ControlPlane/ControlPlane.Domain/
COPY F2pPlatform/src/Modules/ControlPlane/ControlPlane.Infrastructure/ControlPlane.Infrastructure.csproj F2pPlatform/src/Modules/ControlPlane/ControlPlane.Infrastructure/

WORKDIR /src/F2pPlatform
RUN dotnet restore host/F2pPlatform.Host/F2pPlatform.Host.csproj

COPY Platform.ControlPlane.Contracts/ /src/Platform.ControlPlane.Contracts/
COPY F2pPlatform/ /src/F2pPlatform/

RUN dotnet publish host/F2pPlatform.Host/F2pPlatform.Host.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV Kestrel__Endpoints__Http__Url=http://0.0.0.0:5080

COPY --from=build /app/publish .

EXPOSE 5080
ENTRYPOINT ["dotnet", "F2pPlatform.Host.dll"]
