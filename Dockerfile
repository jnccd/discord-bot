FROM mcr.microsoft.com/dotnet/core/sdk:2.2

WORKDIR /app
COPY MEE7-Discord-Bot .

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && rm -rf /var/lib/apt/lists/*

RUN dotnet restore
RUN dotnet build -c Release

CMD ["dotnet", "run", "-c", "Release"]
