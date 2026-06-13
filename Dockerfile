# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY NetGuardGT.sln ./
COPY NetGuardGT.Api/NetGuardGT.Api.csproj NetGuardGT.Api/
COPY NetGuardGT.Tests/NetGuardGT.Tests.csproj NetGuardGT.Tests/

RUN dotnet restore

COPY . .
RUN dotnet publish NetGuardGT.Api/NetGuardGT.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "NetGuardGT.Api.dll"]
