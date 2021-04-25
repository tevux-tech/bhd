FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim-arm64v8 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim-arm64v8 AS build
WORKDIR /src
COPY ["BlazorHomieDashboard.Server/BlazorHomieDashboard.Server.csproj", "BlazorHomieDashboard.Server/"]
RUN dotnet restore "BlazorHomieDashboard.Server/BlazorHomieDashboard.Server.csproj"
COPY . .
WORKDIR "/src/BlazorHomieDashboard.Server"
RUN dotnet build "BlazorHomieDashboard.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorHomieDashboard.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorHomieDashboard.Server.dll"]

ENV MQTT_SERVER 127.0.0.1
ENV MQTT_SERVER_PORT 9001
ENV BASE_TOPIC homie