FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/e-commerce-platform/e-commerce-platform.csproj", "src/e-commerce-platform/"]
RUN dotnet restore "./src/e-commerce-platform/e-commerce-platform.csproj"
COPY . .
WORKDIR "/src/src/e-commerce-platform"
RUN dotnet build "./e-commerce-platform.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./e-commerce-platform.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "e-commerce-platform.dll"]
