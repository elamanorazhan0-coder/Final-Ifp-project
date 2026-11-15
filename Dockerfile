# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AutoCare.csproj ./
RUN dotnet restore AutoCare.csproj
COPY . .
RUN dotnet publish AutoCare.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# SQLite database lives in /app/Data (created and seeded on first start)
RUN mkdir -p /app/Data && chown -R app:app /app/Data
USER app
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
# Hosts like Render/Railway pass the port in $PORT; default to 8080 otherwise
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet AutoCare.dll"]
