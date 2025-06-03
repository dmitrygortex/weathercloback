#!/bin/bash

# Скрипт для развертывания приложения

echo "🚀 Starting deployment..."

# Проверка наличия .env файла
if [ ! -f .env ]; then
    echo "❌ .env file not found! Please create it from .env.example"
    exit 1
fi

# Загрузка переменных окружения
#export $(cat .env | xargs)


# Остановка старых контейнеров
echo "🛑 Stopping old containers..."
docker compose down

# Сборка новых образов
echo "🔨 Building new images..."
docker compose build

# Запуск PostgreSQL и MinIO
echo "🗄️ Starting PostgreSQL and MinIO..."
docker compose up -d postgres minio

# Ожидание готовности PostgreSQL
echo "⏳ Waiting for PostgreSQL to be ready..."
until docker compose exec -T postgres pg_isready -U postgres
do
    echo "Waiting for PostgreSQL..."
    sleep 2
done

# Применение миграций
echo "🔄 Applying database migrations..."
docker compose run --rm api dotnet ef database update

# Создание bucket в MinIO
echo "📦 Creating MinIO bucket..."
docker compose exec minio mc alias set myminio http://localhost:9000 minioadmin minioadmin
docker compose exec minio mc mb myminio/clothing-images --ignore-existing
docker compose exec minio mc anonymous set public myminio/clothing-images

# Запуск API
echo "🚀 Starting API..."
docker compose up -d api

echo "✅ Deployment completed!"
echo "📍 API is available at: http://localhost:5001"
echo "📍 MinIO console: http://localhost:9001"
echo "📍 Swagger: http://localhost:5001/swagger"
