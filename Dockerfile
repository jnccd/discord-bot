FROM mcr.microsoft.com/dotnet/core/sdk:2.2

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && rm -rf /var/lib/apt/lists/*

RUN cd MEE7-Discord-Bot
RUN dotnet restore
RUN dotnet build -c Release
RUN ls -R

CMD ["dotnet", "run", "-c", "Release"]
