# FlashCards API

A minimal API for a study flashcards application

## Entities

### Users

### Questions

### Categories

## Endpoints

## Services

## Authentication
- JWT auth
- Refresh tokens

##  Directory Structure
```
api
| Flashcards.sln
| - src
|   |- Flashcards.Api
|       |- Flashcards.Api.csproj
|       |- Program.cs (entrypoint)
|       |- appsettings.json
|       |- appsettings.Development.json
|       |- Features
|       |   |- Users
|       |   |   |- User.cs, UserDtos.cs, UserMappings.cs, UserEndpoints.cs, etc
|       |   |- Categories
|       |   |   |- Category.cs, CategoryDtos.cs, CategoryMapping.cs, CategoryEndpoints.cs, etc
|       |   |- Questions
|       |   |   |- Question.cs, QuestionDtos.cs, QuestionMappings.cs, ConfidenceLevel.cs, QuesiontsEndpont.cs, etc
|       |   |- Quizzes
|       |   |   |- QuizDtos.cs, QuizEndpoints.cs, QuizService.cs, etc       
|       |- Auth
|       |   |- JWTTokenService.cs, etc
|       |- Persistence
|       |   |- ApplicationDbContext.cs
|       |   |- EntityConfigurations
|       |   |   |- UserConfiguration.cs, CategoryConfiguration.cs, QuestionConfiguration.cs, RefreshTokenConfiguration.cs
|       |   |- Migrations
|       |- Common
|           |- Errors
|           |   |- ApiError.cs, ErrorMiddleware.cs 
|           |- Logging
|               |- LoggingExtensions.cs
| - tests
|   |- tests.csproj
|   |- Users
|   |   |- UserEndpointsTest.cs
|   |   |- UserServiceTests.cs
|   |- Quesions
|       |- QuesionEndpointsTest.cs
...etc
```

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

