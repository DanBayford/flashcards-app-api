# SDK layer to compile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + csproj files first (better cache)
COPY *.sln ./
COPY src/Flashcards.Api/*.csproj src/Flashcards.Api/
COPY src/Data.Reset/*.csproj src/Data.Reset/

# Install required Nuget packages
RUN dotnet restore src/Flashcards.Api/Flashcards.Api.csproj

# Copy the rest of the source
COPY . .

# Publish the API project
RUN dotnet publish src/Flashcards.Api/Flashcards.Api.csproj \
  -c Release -o /out/api /p:UseAppHost=false

# Publish reset tool
RUN dotnet publish src/Data.Reset/Data.Reset.csproj \
  -c Release -o /out/reset /p:UseAppHost=false


# Runtime layer 
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Add SQLite CLI for debugging if required
RUN apt-get update \
 && apt-get install -y sqlite3 \
 && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# API binaries in /app
COPY --from=build /out/api ./
# Reset binaries in /app/reset
COPY --from=build /out/reset ./reset/

ENTRYPOINT ["dotnet", "Flashcards.Api.dll"]