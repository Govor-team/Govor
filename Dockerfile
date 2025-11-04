FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только .sln и .csproj
COPY *.sln ./
COPY Govor.API/*.csproj ./Govor.API/
COPY Govor.Application/*.csproj ./Govor.Application/
COPY Govor.Core/*.csproj ./Govor.Core/
COPY Govor.Data/*.csproj ./Govor.Data/
COPY Govor.Contracts/*.csproj ./Govor.Contracts/

RUN dotnet restore Govor.API/Govor.API.csproj

# Копируем весь код
COPY . .
WORKDIR /src/Govor.API
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Govor.API.dll"]