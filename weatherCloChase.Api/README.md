# Weather Clothing API

Backend для системы подбора одежды по погоде с использованием ML.

## Требования

- .NET 7.0 SDK
- Docker и Docker Compose
- PostgreSQL (через Docker)
- MinIO (через Docker)
- Ключ API OpenWeatherMap

## Быстрый старт

1. Клонируйте репозиторий
2. Создайте файл `.env` в корне проекта:
   env OPENWEATHERMAP_API_KEY=your-api-key JWT_SECRET=your-very-long-secret-key-at-least-32-characters


3. Запустите зависимости через Docker:
   bash docker-compose up -d postgres minio


4. Примените миграции:
   bash cd WeatherClothing.API dotnet ef database update


5. Запустите API:
   bash dotnet run


API будет доступен по адресу: http://localhost:5000

## Эндпоинты

### Аутентификация
- `POST /api/v1/auth/register` - Регистрация пользователя
- `POST /api/v1/auth/login` - Вход в систему

### Одежда
- `POST /api/v1/clothing/upload` - Загрузка фото одежды (требует авторизации)
- `GET /api/v1/clothing/my-wardrobe` - Получение гардероба пользователя
- `DELETE /api/v1/clothing/{id}` - Удаление элемента одежды

### Рекомендации
- `POST /api/v1/recommendation/get-recommendations` - Получение рекомендаций по погоде

## Конфигурация MinIO

1. Откройте http://localhost:9001
2. Войдите с логином/паролем: minioadmin/minioadmin
3. Создайте bucket "clothing-images"
4. Установите политику доступа "public" для чтения

## Размещение ONNX модели

Поместите файл `clothing_classifier.onnx` в папку:
WeatherClothing.ML/models/clothing_classifier.onnx


## Тестирование через Swagger

Откройте http://localhost:5000/swagger для доступа к Swagger UI.

## Примеры запросов

### Регистрация:
json POST /api/v1/auth/register { "email": "user@example.com", "password": "password123" }


### Получение рекомендаций:
json POST /api/v1/recommendation/get-recommendations Authorization: Bearer {token} { "latitude": 55.7558, "longitude": 37.6173 }


### Загрузка фото одежды:
bash curl -X POST http://localhost:5000/api/v1/clo...
-H "Authorization: Bearer {token}"
-F "image=@/path/to/image.jpg"


## Структура ответа рекомендаций

json { "currentWeather": { "temperature": 15.5, "feelsLike": 14.2, "description": "облачно", "humidity": 65, "windSpeed": 3.5 }, "recommendation": { "description": "Прохладно. Возьмите с собой куртку.", "recommendedCategories": ["sweater", "pants", "sneakers", "jacket"], "specificItems": [ { "id": "guid", "category": "sweater", "imageUrl": "http://localhost:9000/clothing-images/..." } ], "additionalItems": [] }, "forecast": { "minTemperature": 10.2, "maxTemperature": 18.7, "willRain": false, "hourlyForecast": [...] }, "additionalRecommendations": [ "Температура будет сильно меняться в течение дня. Возьмите дополнительную одежду." ] }


## Оценка потребления памяти MinIO

MinIO в минимальной конфигурации потребляет:
- **RAM**: 512MB - 1GB (зависит от нагрузки)
- **CPU**: минимальные требования
- **Disk**: зависит от объема хранимых данных

Для тестирования девочками на локальных машинах MinIO подходит отлично.

### Альтернативы для production:
- **AWS S3** - если нужна высокая доступность
- **Яндекс Object Storage** - российский аналог S3
- **Cloudflare R2** - дешевле S3, без платы за исходящий трафик

## Troubleshooting

### Ошибка подключения к PostgreSQL
bash

Проверьте статус контейнера

docker ps

Проверьте логи

docker logs weather_clothing_postgres


### Ошибка загрузки изображений
- Убедитесь, что MinIO запущен
- Проверьте, создан ли bucket "clothing-images"
- Проверьте права доступа к bucket

### Ошибка классификации
- Убедитесь, что ONNX модель находится в правильной папке
- Проверьте, что модель совместима с ML.NET

## Производительность

- Классификация изображения: ~100-500ms
- Получение погоды: ~200-500ms (зависит от OpenWeatherMap)
- Загрузка изображения: зависит от размера и скорости сети

## Безопасность

- Пароли хешируются с использованием SHA256
- JWT токены подписываются секретным ключом
- Изображения доступны только по уникальным URL
- Пользователи могут видеть только свой гардероб

## Дальнейшие улучшения

1. **Кэширование**:
    - Redis для кэширования погоды
    - Кэширование результатов классификации

2. **Оптимизация ML**:
    - Батчевая обработка изображений
    - Использование GPU для ускорения

3. **Функциональность**:
    - Добавление тегов к одежде
    - История рекомендаций
    - Избранные комплекты

4. **Масштабирование**:
    - Горизонтальное масштабирование API
    - Очередь для обработки изображений
    - CDN для раздачи изображений