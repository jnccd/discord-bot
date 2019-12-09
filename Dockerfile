FROM mcr.microsoft.com/dotnet/core/sdk:2.2

WORKDIR /app
COPY MEE7-Discord-Bot .

RUN mkdir dist
RUN dotnet restore
RUN dotnet build -c Release

CMD ["dotnet", "run", "-c", "Release"]
