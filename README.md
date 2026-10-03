# Лабораторная работа №1 — Person Service

Простейшее веб-приложение с CRUD-операциями над сущностью **Person**, написанное на
**.NET 10 (ASP.NET Core Web API)** с хранением в **PostgreSQL (EF Core / Npgsql)**.
Сборка, unit-тесты и деплой на Heroku автоматизированы через **GitHub Actions**.

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
.github/workflows/classroom.yml — CI/CD: build + test + deploy на Heroku
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

## Деплой на Heroku

Выполняется GitHub Actions workflow при пуше в `master`/`main`:

1. сборка и unit-тесты;
2. деплой Docker-образа на Heroku (`akhileshns/heroku-deploy` с `usedocker: true`);
3. подстановка реального адреса в Postman-окружение;
4. интеграционные тесты (Newman) против развёрнутого сервиса.

Для работы нужны секреты репозитория: `HEROKU_API_KEY`, `HEROKU_APP_NAME`,
`HEROKU_EMAIL`. На Heroku к приложению должен быть подключён аддон **Heroku Postgres**
(переменная `DATABASE_URL` заполняется автоматически, приложение читает её при старте).