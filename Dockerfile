# SDK layer to compile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + csproj files first (better cache)
COPY *.sln ./
COPY src/Flashcards.Api/*.csproj src/Flashcards.Api/
COPY src/Data.Reset/*.csproj src/Data.Reset/

RUN dotnet restore src/Flashcards.Api/Flashcards.Api.csproj

# Copy the rest of the source
COPY . .

# Publish ONLY the API project
RUN dotnet publish src/Flashcards.Api/Flashcards.Api.csproj \
  -c Release -o /out /p:UseAppHost=false

# Runtime layer 
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Add SQLite CLI for debugging if required
RUN apt-get update \
 && apt-get install -y sqlite3 \
 && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

COPY --from=build /out ./
ENTRYPOINT ["dotnet", "Flashcards.Api.dll"]