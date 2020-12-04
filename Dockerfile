#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Webscan.ProductStatusProcessor/Webscan.ProductStatusProcessor.csproj", "Webscan.ProductStatusProcessor/"]
COPY nuget.config .
RUN dotnet restore "Webscan.ProductStatusProcessor/Webscan.ProductStatusProcessor.csproj" --configfile ./nuget.config
COPY . .
WORKDIR "/src/Webscan.ProductStatusProcessor"
RUN dotnet build "Webscan.ProductStatusProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Webscan.ProductStatusProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Webscan.ProductStatusProcessor.dll"]