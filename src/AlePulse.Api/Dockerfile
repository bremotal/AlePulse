FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AlePulse.Api/AlePulse.Api.csproj", "AlePulse.Api/"]
COPY ["AlePulse.Application/AlePulse.Application.csproj", "AlePulse.Application/"]
COPY ["AlePulse.Domain/AlePulse.Domain.csproj", "AlePulse.Domain/"]
COPY ["AlePulse.Infrastructure/AlePulse.Infrastructure.csproj", "AlePulse.Infrastructure/"]
COPY ["AlePulse.Shared/AlePulse.Shared.csproj", "AlePulse.Shared/"]
RUN dotnet restore "AlePulse.Api/AlePulse.Api.csproj"
COPY . .
WORKDIR "/src/AlePulse.Api"
RUN dotnet build "AlePulse.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlePulse.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlePulse.Api.dll"]