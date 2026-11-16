# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Web API for Contacts 2.0.csproj", "./"]
RUN dotnet restore "Web API for Contacts 2.0.csproj"

COPY . .
RUN dotnet publish "Web API for Contacts 2.0.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web API for Contacts 2.0.dll"]
