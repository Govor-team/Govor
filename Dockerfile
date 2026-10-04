FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ./
COPY Govor.API/*.csproj ./Govor.API/
COPY Govor.Application/*.csproj ./Govor.Application/
COPY Govor.Domain/*.csproj ./Govor.Domain/
COPY Govor.Contracts/*.csproj ./Govor.Contracts/
COPY libs/ ./libs/
RUN dotnet restore Govor.API/Govor.API.csproj
COPY . .
RUN dotnet publish Govor.API/Govor.API.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
    CMD curl --fail --silent --show-error --max-time 5 http://127.0.0.1:8080/server/ping || exit 1
ENTRYPOINT ["dotnet", "Govor.API.dll"]
