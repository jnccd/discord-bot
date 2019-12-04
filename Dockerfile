FROM mcr.microsoft.com/dotnet/core/sdk:2.2
WORKDIR /app

RUN dotnet restore

COPY app/bin/Release/netcoreapp2.2/publish/ app/
RUN dotnet publish -c Release -o out

ENTRYPOINT ["dotnet", "app/MEE7.dll"]