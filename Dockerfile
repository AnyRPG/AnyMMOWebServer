FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["app/AnyMMOWebServer/AnyMMOWebServer/AnyMMOWebServer.csproj", "AnyMMOWebServer/"]

RUN dotnet nuget locals all --clear
RUN dotnet restore "./AnyMMOWebServer/AnyMMOWebServer.csproj"

RUN dotnet list "./AnyMMOWebServer/AnyMMOWebServer.csproj" package --include-transitive

COPY app/AnyMMOWebServer .
WORKDIR "/src/AnyMMOWebServer"
RUN dotnet build "./AnyMMOWebServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AnyMMOWebServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/keys
RUN chown -R app:app /app/keys 

ENTRYPOINT ["dotnet", "AnyMMOWebServer.dll"]