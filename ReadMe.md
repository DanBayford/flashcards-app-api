# FlashCards API

A minimal API for a study flashcards application

## Entities
- Users
- Questions
- Categories

## Endpoints
- GET `/api/category`
- GET `/api/category/:id`
- POST `/api/category`
- PUT `/api/category/:id`
- DELETE `/api/category/:id`
- GET `/api/question`
- GET `/api/question/:id`
- POST `/api/question`
- PUT `/api/question/:id`
- DELETE `/api/question/:id`
- POST `/api/quiz/generate`
- POST `/api/quiz/update`
- POST '/api/user/register'
- POST '/api/user/login'
- GET '/api/user/me'
- POST '/api/user/refresh'
- POST '/api/user/logout'
- POST '/api/user/password'

## Authentication
- JWT auth
- Refresh tokens

## Persistence

There is an overall ``ApplicationDbContext`` file and then individual entity config files. This allows the individual entity constraints be be defined in their own files.

A ``Persistence/Migrations`` directory holda all the migrations. It can be specified as the migration aoutput as:

```shell
dotnet ef migrations add <MigrationName> --output-dir Persistence/Migrations
```

The database can then be updated via:

```shell
dotnet ef database update
```

## Jobs

There is an extra console app ``Data.Reset`` that can be used to reset the test user `dan@flashcards.com` 

## DB Schema

```dbml
Table users {
  Id guid [pk]
  Email varchar
  PasswordHash varchar
  PasswordSalt varchar
  RefreshToken varchar
  RefreshTokenExpiresAtUtc datetime
  CreatedAtUtc datetime
  UpdatedAtUtc datetime
}

Table questions {
  Id guid [pk]
  UserId guid [ref: > users.Id]
  Prompt varchar
  Hint varchar [null]
  Answer varchar
  Confidencelevel ConfidenceLevel
  CreatedAtUtc datetime
  UpdatedAtUtc datetime
}

Table categories {
  Id guid [pk]
  UserId guid [ref: > users.Id]
  Name varchar
  CreatedAtUtc datetime
  UpdatedAtUtc datetime
}

Table question_categories {
  QuestionId guid [ref: > questions.Id]
  CategoryId guid [ref: > categories.Id]
}

Enum ConfidenceLevel {
  None
  VeryLow
  Low
  Medium
  High
  VeryHigh
}
```
