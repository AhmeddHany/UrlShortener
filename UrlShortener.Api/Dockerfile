# 1. مرحلة التشغيل (Runtime) - Linux
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 2. مرحلة البناء (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# نسخ ملفات الـ csproj لكل المشاريع عشان الـ Restore يشتغل صح
COPY ["UrlShortener.Api/UrlShortener.Api.csproj", "UrlShortener.Api/"]
COPY ["UrlShortener.Core/UrlShortener.Core.csproj", "UrlShortener.Core/"]
COPY ["UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj", "UrlShortener.Infrastructure/"]

# عمل Restore للمكتبات
RUN dotnet restore "UrlShortener.Api/UrlShortener.Api.csproj"

# نسخ كل الكود وبناء المشروع
COPY . .
WORKDIR "/src/UrlShortener.Api"
RUN dotnet build "UrlShortener.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3. مرحلة النشر (Publish)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UrlShortener.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. المرحلة النهائية (Final)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]