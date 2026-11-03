# ATLAS – Digital Onboarding & Compliance

ATLAS is a digital onboarding application designed to manage customer applications and compliance checks across six markets.

It provides a structured workflow for collecting customer information, validating data, assessing compliance requirements, and managing application progress.

## Features

- Customer onboarding and application management.
- Market-specific validation and compliance rules.
- Compliance assessment and manual review foundation.
- Application status management.
- Staff authentication and application tracking.

## Tech Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- C#

## Architecture

The application follows a **Modular Monolith** approach, using Clean Architecture and Domain-Driven Design principles.

- **Domain:** Business entities, rules, and policies.
- **Application:** Use cases and workflows.
- **Infrastructure:** Database access and external integrations.
- **API:** HTTP endpoints and application entry point.

This keeps business logic independent from infrastructure and makes the system easier to maintain and extend.

## Getting Started

**Prerequisites:** .NET 10 SDK and SQL Server.

```bash
git clone <repository-url>
cd <repository-folder>
dotnet restore
dotnet build
```

Configure the database connection string, then run migrations:

```bash
dotnet ef database update
```

Start the API:

```bash
dotnet run --project src/ATLAS.API
```

Adjust the project path if necessary.

## Limitations & Future Improvements

The current implementation provides the architectural foundation and initial onboarding workflows. With more time, the main priorities would be:

- Complete and test compliance rules across all six markets.
- Finish sanctions/PEP screening, manual reviews, and the full audit trail.
- Add a frontend, role-based permissions, and stronger security.
- Expand automated testing and prepare production deployment, monitoring, and backups.
- Integrate real identity and screening providers; the supplied `.example` endpoints are placeholders.
- Revisit country-specific data residency and clarify retention versus erasure requirements.
- Treat the three-minute target as digital submission time, not guaranteed final activation where manual review or physical attendance is required.

A modular monolith is sufficient for this scope; separate microservices per market would add unnecessary complexity. Customer identifiers should remain business data rather than database primary keys, and potential compliance matches must never be automatically approved.

**Status:** A working backend foundation, not yet a complete production-ready onboarding solution.
