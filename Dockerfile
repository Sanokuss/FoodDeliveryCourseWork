# Етап 1: Збірка (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо файл проєкту
# Оскільки Dockerfile лежить поруч з папкою CourseWork, шлях правильний:
COPY ["CourseWork/CourseWork.csproj", "CourseWork/"]

# Відновлюємо залежності
RUN dotnet restore "CourseWork/CourseWork.csproj"

# Копіюємо всі інші файли з поточної директорії (де лежить Dockerfile) в контейнер
COPY . .

# Переходимо в папку з проєктом для збірки
WORKDIR "/src/CourseWork"

# Збираємо проєкт
RUN dotnet build "CourseWork.csproj" -c Release -o /app/build

# Публікуємо проєкт
FROM build AS publish
RUN dotnet publish "CourseWork.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Етап 2: Запуск (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Налаштування порту для Render (важливо!)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CourseWork.dll"]