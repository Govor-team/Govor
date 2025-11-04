FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Govor.API/*.csproj Govor.API/
COPY Govor.Application/*.csproj Govor.Application/
COPY Govor.Core/*.csproj Govor.Core/
COPY Govor.Data/*.csproj Govor.Data/
COPY Govor.Contracts/*.csproj Govor.Contracts/
RUN dotnet restore Govor.API/Govor.API.csproj

COPY . .
WORKDIR /src/Govor.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Govor.API.dll"]
