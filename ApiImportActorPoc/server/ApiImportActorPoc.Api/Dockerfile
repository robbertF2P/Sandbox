FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY server/ApiImportActorPoc.Api/ApiImportActorPoc.Api.csproj server/ApiImportActorPoc.Api/
COPY server/ApiImportActorPoc.Contracts/ApiImportActorPoc.Contracts.csproj server/ApiImportActorPoc.Contracts/
COPY server/ApiImportActorPoc.Core/ApiImportActorPoc.Core.csproj server/ApiImportActorPoc.Core/
COPY server/ApiImportActorPoc.Data/ApiImportActorPoc.Data.csproj server/ApiImportActorPoc.Data/
COPY server/ApiImportActorPoc.Data.Migrations/ApiImportActorPoc.Data.Migrations.csproj server/ApiImportActorPoc.Data.Migrations/
RUN dotnet restore server/ApiImportActorPoc.Api/ApiImportActorPoc.Api.csproj

COPY server/ApiImportActorPoc.Api/ server/ApiImportActorPoc.Api/
COPY server/ApiImportActorPoc.Contracts/ server/ApiImportActorPoc.Contracts/
COPY server/ApiImportActorPoc.Core/ server/ApiImportActorPoc.Core/
COPY server/ApiImportActorPoc.Data/ server/ApiImportActorPoc.Data/
COPY server/ApiImportActorPoc.Data.Migrations/ server/ApiImportActorPoc.Data.Migrations/
RUN dotnet publish server/ApiImportActorPoc.Api/ApiImportActorPoc.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV Kestrel__Endpoints__Http__Url=http://0.0.0.0:5001

COPY --from=build /app/publish .

EXPOSE 5001
ENTRYPOINT ["dotnet", "ApiImportActorPoc.Api.dll"]
