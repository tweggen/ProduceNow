FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

RUN apt-get -y update
RUN apt-get -y upgrade
RUN apt-get install -y ffmpeg

WORKDIR /app
EXPOSE 5401

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DemoContent/DemoContent.csproj", "DemoContent/"]
RUN dotnet restore "DemoContent/DemoContent.csproj"
COPY . .
WORKDIR "/src/DemoContent"
RUN dotnet build "DemoContent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoContent.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoContent.dll"]
