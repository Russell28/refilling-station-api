# Water Refilling Station API

RESTful API powering a Water Refilling Station Management System for managing daily business operations such as deliveries, expenses, payroll, customer debts, and business reporting.

## Overview

This project is a RESTful API built with ASP.NET Core 8 for a Water Refilling Station Management System. It provides secure and scalable endpoints for managing day-to-day business operations while following a layered architecture that promotes maintainability, separation of concerns, and testability.

## Features

- Dashboard and daily business summary
- Trip and delivery management
- Expense tracking
- Customer debt management
- Payroll processing
- User and employee management
- Secure authentication and authorization

## Tech Stack

### Backend
- ASP.NET Core 8
- Entity Framework Core
- PostgreSQL

### Authentication & Security
- JWT Authentication
- Refresh Tokens
- Role-based Authorization

### Architecture & Design
- Layered Architecture
- Dependency Injection
- RESTful API Design

### Performance
- In-Memory Caching

### Testing
- xUnit

### Documentation
- Swagger / OpenAPI

## Architecture

The project follows a layered architecture to separate business logic from infrastructure and presentation concerns.

```text
API
├── Controllers
├── Middleware

Application
├── DTOs
├── Interfaces
├── Services
├── Validators

Domain
├── Entities
├── Enums
├── ErrorCodes
├── Exceptions

Infrastructure
├── Authentication
├── Imports
├── Migrations
├── Persistence

Tests
├── Domain
├── Services
```

## API Capabilities

- RESTful endpoint design
- JWT authentication
- Refresh token support
- Role-based authorization
- Global exception handling
- Request validation
- Dependency injection
- In-memory caching
- Swagger API documentation

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL

### Installation

Clone the repository.

```bash
git clone https://github.com/Russell28/refilling-station-api.git
```

Navigate to the project.

```bash
cd refilling-station-api
```

Restore dependencies.

```bash
dotnet restore
```

Configure your PostgreSQL connection string in `appsettings.json`.

Run database migrations.

```bash
dotnet ef database update
```

Start the API.

```bash
dotnet run
```

Once the application is running, access the Swagger UI to explore the available endpoints.

## Testing

Run the unit tests.

```bash
dotnet test
```

Unit tests are implemented using xUnit to verify business logic and ensure application reliability.

## Related Repository

Frontend Application:
https://github.com/Russell28/refilling-station-app.git

## License

This project is provided for portfolio and educational purposes.