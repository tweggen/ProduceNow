FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5245

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DemoSignalServer/DemoSignalServer.csproj", "DemoSignalServer/"]
RUN dotnet restore "DemoSignalServer/DemoSignalServer.csproj"
COPY . .
WORKDIR "/src/DemoSignalServer"
RUN dotnet build "DemoSignalServer.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "DemoSignalServer.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoSignalServer.dll"]
