FROM mcr.microsoft.com/dotnet/sdk:11.0-rc.1 AS build
WORKDIR /src
COPY . .
RUN dotnet publish HybridCacheValidation/HybridCacheValidation.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:11.0-rc.1
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HybridCacheValidation.dll"]