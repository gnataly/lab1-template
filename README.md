# Лабораторная работа №1 — Person Service

Простейшее веб-приложение с CRUD-операциями над сущностью **Person**, написанное на
**.NET 10 (ASP.NET Core Web API)** с хранением в **PostgreSQL (EF Core / Npgsql)**.
Сборка, unit-тесты и деплой на Render автоматизированы через **GitHub Actions**.

## REST API

Базовый путь: `/api/v1/persons`

| Метод   | Путь                  | Описание                                   | Успешный ответ      |
|---------|-----------------------|--------------------------------------------|---------------------|
| `GET`   | `/persons`            | Список всех людей                          | `200` + JSON-массив |
| `GET`   | `/persons/{id}`       | Информация о человеке                      | `200` + JSON         |
| `POST`  | `/persons`            | Создание человека                          | `201` + `Location`   |
| `PATCH` | `/persons/{id}`       | Обновление человека (частичное)            | `200` + JSON         |
| `DELETE`| `/persons/{id}`       | Удаление человека                          | `204`                |

Формат данных — JSON (camelCase). Если запись по `id` не найдена — `404 Not Found`.

Модель `Person`:

```json
{
  "id": 1,
  "name": "Alice",
  "age": 31,
  "address": "Moscow",
  "work": "T-Bank"
}
```

## Структура проекта

```
PersonService/            — Web API приложение
  Controllers/PersonsController.cs
  Contracts/              — PersonRequest / PersonResponse / ошибки
  Data/AppDbContext.cs
  Models/Person.cs
  Migrations/
PersonService.Tests/       — unit-тесты (xUnit, EF Core InMemory)
Dockerfile                 — multi-stage образ
docker-compose.yml         — локальный PostgreSQL 13
.github/workflows/classroom.yml — CI/CD: build + test + deploy на Render
postman/                   — коллекция и окружения для интеграционных тестов
person-service.yaml        — OpenAPI-спецификация
```

## Локальный запуск

Требуется .NET SDK 10 и Docker.

```bash
# 1. Поднять PostgreSQL (порт 5433, чтобы не конфликтовать с занятым 5432)
docker compose up -d

# 2. Запустить приложение (слушает http://localhost:8080)
dotnet run --project PersonService
```

Миграции применяются автоматически при старте приложения.

Интеграционные тесты локально (Newman/Postman) — коллекция
`postman/[inst] Lab1.postman_collection.json` и окружение
`postman/[inst][local] Lab1.postman_environment.json` (baseUrl `http://localhost:8080`).

## Unit-тесты

```bash
dotnet test
```

## Docker-образ

```bash
docker build -t person-service .
docker run --rm -p 8080:8080 -e DATABASE_URL="postgres://program:test@host.docker.internal:5433/persons" person-service
```

## Деплой на Render

Выполняется GitHub Actions workflow при пуше в `master`/`main`:

1. сборка и unit-тесты;
2. деплой на Render через **Deploy Hook** (POST триггерит пересборку/редисплой);
3. ожидание, пока сервис поднимется (free-план после сна просыпается ~30–60 c);
4. подстановка реального адреса в Postman-окружение;
5. интеграционные тесты (Newman) против развёрнутого сервиса.

Для работы нужны секреты репозитория:

- `RENDER_DEPLOY_HOOK_URL` — адрес Deploy Hook из Render (Settings → Deploy Hook);
- `RENDER_URL` — адрес сервиса, например `https://person-service.onrender.com`.

На Render к сервису должна быть подключена **Render PostgreSQL**; переменная окружения
`DATABASE_URL` (внутренний адрес БД) задаётся в Render (Environment). Приложение читает
`DATABASE_URL` и `PORT` при старте.

> Примечание: free-тариф Render выключает сервис после ~15 минут простоя (следующий
> запрос просыпает его ~30–60 c), а free-PostgreSQL живёт ~90 дней.