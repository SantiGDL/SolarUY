FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# .NET 10 detecta assets de Blazor al restaurar: los .razor deben existir aquí.
COPY SolarUy/ SolarUy/
COPY Sensor/ Sensor/
RUN dotnet restore SolarUy/SolarUy.csproj
RUN dotnet publish SolarUy/SolarUy.csproj -c Release --no-restore -o /publicado

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends fonts-dejavu-core curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /publicado .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "SolarUy.dll"]
