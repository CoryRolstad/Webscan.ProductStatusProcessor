#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

RUN apt-get update && apt-get install -y gconf-service libasound2 libatk1.0-0 libc6 libcairo2 libcups2 libdbus-1-3 libexpat1 libfontconfig1 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libglib2.0-0 libgtk-3-0 libnspr4 libpango-1.0-0 libpangocairo-1.0-0 libstdc++6 libx11-6 libx11-xcb1 libxcb1 libxcomposite1 libxcursor1 libxdamage1 libxext6 libxfixes3 libxi6 libxrandr2 libxrender1 libxss1 libxtst6 ca-certificates fonts-liberation libappindicator1 libnss3 lsb-release xdg-utils wget

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

RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get install -y iputils-ping && \
    apt-get install -y tzdata && \
    ln -fs /usr/share/zoneinfo/America/Chicago /etc/localtime && \
    dpkg-reconfigure --frontend noninteractive tzdata

# Setup the sandbox for headless chrome - this also requires --cap-add=SYS_ADMIN when running the container
RUN echo 'kernel.unprivileged_userns_clone=1' > /etc/sysctl.d/userns.conf

COPY --from=publish /app/publish .

# Do not run as root user
RUN chown -R www-data:www-data /app
USER www-data

ENTRYPOINT ["dotnet", "Webscan.ProductStatusProcessor.dll"]