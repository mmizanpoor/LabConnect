FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LabConnectPortal.Api/LabConnectPortal.Api.csproj", "LabConnectPortal.Api/"]
RUN dotnet restore "LabConnectPortal.Api/LabConnectPortal.Api.csproj"

COPY . .
WORKDIR "/src/LabConnectPortal.Api"
RUN dotnet publish "LabConnectPortal.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LabConnectPortal.Api.dll"]