#!/bin/bash

# Скрипт проверки здоровья сервисов

echo "🏥 Health Check"
echo "==============="

# Проверка PostgreSQL
echo -n "PostgreSQL: "
if docker-compose exec -T postgres pg_isready -U postgres > /dev/null 2>&1; then
    echo "✅ Healthy"
else
    echo "❌ Unhealthy"
fi

# Проверка MinIO
echo -n "MinIO: "
if curl -s http://localhost:9000/minio/health/live > /dev/null; then
    echo "✅ Healthy"
else
    echo "❌ Unhealthy"
fi

# Проверка API
echo -n "API: "
if curl -s http://localhost:5000/api/v1/health > /dev/null; then
    echo "✅ Healthy"
else
    echo "❌ Unhealthy"
fi

echo "==============="
