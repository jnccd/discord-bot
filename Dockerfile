FROM mcr.microsoft.com/dotnet/core/sdk:2.2

WORKDIR /app
COPY MEE7-Discord-Bot .

RUN apt-get update
RUN apt-get install -y apt-utils
RUN apt-get install -y libgdiplus
RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll

RUN dotnet restore
RUN dotnet build -c Release

CMD ["dotnet", "run", "-c", "Release"]
