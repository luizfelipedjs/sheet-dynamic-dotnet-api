#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Playground.Api/Playground.Api.csproj", "Playground.Api/"]
RUN dotnet restore "Playground.Api/Playground.Api.csproj"
COPY . .
WORKDIR "/src/Playground.Api"
RUN dotnet build "Playground.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Playground.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN useradd -m myappuser
USER myappuser

CMD ASPNETCORE_URLS=http://*:$PORT dotnet Playground.Api.dll