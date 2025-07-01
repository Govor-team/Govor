# Используем официальный образ с SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем csproj и восстанавливаем зависимости
COPY *.sln .
COPY Govor.API/*.csproj ./Govor.API/
COPY Govor.Application/*.csproj ./Govor.Application/
COPY Govor.Core/*.csproj ./Govor.Core/
COPY Govor.Data/*.csproj ./Govor.Data/
RUN dotnet restore

# Копируем все исходники и билдим проект в Release режиме
COPY . .
WORKDIR /app/Govor.API
RUN dotnet publish -c Release -o out

# Используем лёгкий runtime образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем билд из предыдущего этапа
COPY --from=build /app/Govor.API/out ./

# Указываем порт (если используется нестандартный, замени 80)
EXPOSE 8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "Govor.API.dll"]
