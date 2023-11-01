#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:7.0 AS base
WORKDIR /app
RUN apt-get update
RUN apt-get install -y --no-install-recommends libgdiplus libc6-dev ffmpeg youtube-dl
RUN apt-get install -y tesseract-ocr tesseract-ocr-eng tesseract-ocr-chi-sim tesseract-ocr-chi-tra tesseract-ocr-jpn
Run rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/core/sdk:7.0 AS build
WORKDIR /src
COPY ["MEE7-Discord-Bot/MEE7-Discord-Bot.csproj", "MEE7-Discord-Bot/"]
RUN dotnet restore "MEE7-Discord-Bot/MEE7-Discord-Bot.csproj"
COPY . .
WORKDIR "/src/MEE7-Discord-Bot"
RUN dotnet build "MEE7-Discord-Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MEE7-Discord-Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MEE7.dll"]
