# Real State API

A **.NET** Web API that demonstrates CRUD operations and authentication for real estate properties and owners. Built with **C#**, **.NET**, **MongoDB**, and documented with **Swagger** (OpenAPI).

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Running Locally](#running-locally)
- [Authentication Flow](#authentication-flow)
- [API Endpoints Overview](#api-endpoints-overview)
- [Database Backup / Restore](#database-backup--restore)
- [License](#license)

---

## Features

- **Owners (Users)**
  - Register (Hash + Salt password)
  - Login (JWT token generation)
- **Properties**
  - Filter properties by name, address, price range, etc.
  - Create, update, delete properties (must be authenticated and owner of the property)
  - Image handling (stores images in a MongoDB collection)
- **Swagger (OpenAPI)** documentation for endpoints
- **JWT-based** authentication

---

## Prerequisites

- **.NET 6.0** or later
- **MongoDB** installed locally or accessible remotely
- **Git** (optional, for cloning the repo)

---

## Configuration

1. **Environment Variables** (in `.env` or system environment):
   - `CONNECTION_STRING` = `"mongodb://localhost:27017"` (or another URI)
   - `MONGO_DATABASE_NAME` = `"RealStateDb"`
   - `JWT_KEY` = `"YourJWTSecretKey"`
   - `JWT_ISSUER` = `"default-issuer"`
   - `JWT_AUDIENCE` = `"default-audience"`

2. **`appsettings.json`**:
   - Contains logging, swagger, and other .NET configurations.
   - A snippet for the `MongoDbSettings` section is recommended, e.g.:

     ```json
     {
       "MongoDbSettings": {
         "DatabaseName": "RealStateDb"
       }
     }
     ```

3. **MongoDbContext**:
   - Reads your environment variable `CONNECTION_STRING`.
   - Reads the `DatabaseName` from `IOptions<MongoDbSettings>`.

---

## Running Locally

1. **Clone** the repository:

   ```bash
   git clone https://github.com/YourUsername/real-state-api.git
   cd real-state-api
   ```
   - Also ensure your `MongoDbSettings` in `appsettings.json` or environment variables contain the DatabaseName key, e.g. `"RealStateDb"`.

2. **Create/Veryfy** `.env`:

    ```bash
    CONNECTION_STRING=mongodb://localhost:27017
    JWT_KEY=YourJWTSecretKey
    JWT_ISSUER=default-issuer
    JWT_AUDIENCE=default-audience
    ```

3. **Build & Run**

    ```bash
    dotnet build
    dotnet run
    ```
    - The API will typically start at `http://localhost:5025` (depending on your `launchSettings.json`).

4. **Swagger UI**
    - visit: `http://localhost:5025/swagger`

---

## Authentication Flow

1. Register: `POST /api/auth/register`
    - Provide Email and Password.
    - Returns 200 OK upon success.

2. Login: `POST /api/auth/login`
    - Provide Email and Password.
    - Returns 200 OK with Token (JWT) if valid.
    - Add "Authorization: Bearer <Token>" to future requests.

---

## API Endpoints Overview
- **`AuthController`**:
    - `POST /api/auth/login` – Logs in a user, returning JWT.
    - `POST /api/auth/register` – Registers a new owner.

- **`PropertyController`**:
    - `GET /api/property` – Lists/filter properties.
    - `GET /api/property/{id}` – Gets a property by ID.
    - `POST /api/property` – Creates a property (authorized).
    - `PUT /api/property/{id}` – Updates a property (authorized & must own it).
    - `DELETE /api/property/{id}` – Deletes a property (authorized & must own it).

---
