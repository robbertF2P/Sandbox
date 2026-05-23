FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY server/AkkaSignalRVuePoc.Api/AkkaSignalRVuePoc.Api.csproj server/AkkaSignalRVuePoc.Api/
RUN dotnet restore server/AkkaSignalRVuePoc.Api/AkkaSignalRVuePoc.Api.csproj

COPY server/AkkaSignalRVuePoc.Api/ server/AkkaSignalRVuePoc.Api/
RUN dotnet publish server/AkkaSignalRVuePoc.Api/AkkaSignalRVuePoc.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV Kestrel__Endpoints__Http__Url=http://0.0.0.0:5000

COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "AkkaSignalRVuePoc.Api.dll"]
