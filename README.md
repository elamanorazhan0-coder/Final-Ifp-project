# AutoCare — Car Service Platform

AutoCare is a student-built workshop management app for customer vehicles, service appointments, repair orders, and spare-parts inventory. The team is Orazkhan Yelaman and Zharkyn Lukpan.

## Technology

- .NET 10 and C#
- Blazor Web App with Interactive Server rendering for operational pages
- ASP.NET Core Identity for password hashing and cookie-based sign-in
- Entity Framework Core 10 and SQLite
- xUnit for unit and SQLite integration tests

The server render mode keeps business rules and database access on the server while providing interactive forms without a separate JavaScript client.

## Requirements and run

Install the .NET 10 SDK. From the project folder:

```bash
dotnet restore
dotnet run
```

Open the localhost URL printed by `dotnet run`. At startup, AutoCare applies EF Core migrations and adds sample records if the database is empty. The SQLite database is `Data/app.db`; it can be removed while the app is stopped to recreate the demo data on the next launch.

### Demo accounts

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@autocare.local` | `AutoCare123!` |
| Mechanic | `mechanic@autocare.local` | `AutoCare123!` |


## Features

- Role-aware dashboards with database-backed counts and upcoming work
- Administrator appointment scheduling, editing, cancellation, and mechanic conflict checks
- Customer vehicle records and per-vehicle repair history
- Repair orders with mechanic assignment, status updates, saved parts prices, and labor/parts totals
- Inventory CRUD, unique SKU, low-stock indicators, and confirmation before deleting a part
- Atomic parts usage and stock reduction in a database transaction; stock cannot be overdrawn
- Form validation, clear success/error feedback, search, filters, and responsive layouts

Mechanics can see and update their assigned orders, and can record stock use on those orders. Administrators can manage all records and inventory. Demo registration does not grant a role; staff accounts should be provisioned by an administrator before use.

## Data model and structure

- `Customer` owns one or more `Vehicle` records.
- `Appointment` references a customer, vehicle, and optionally an assigned mechanic.
- `RepairOrder` references a vehicle, optionally an appointment and mechanic.
- `RepairOrderPart` joins repair orders and parts and snapshots each part's unit price when used.
- `ApplicationUser` and Identity roles provide account and role data.

```text
Components/Pages   Blazor pages and workflows
Components/Layout  Navigation and application shell
Domain             Entities and status enums
Data               Identity/EF Core context, migrations, demo seeder
Services           Scheduling, repair status, stock and dashboard rules
tests/AutoCare.Tests  Unit and SQLite workflow tests
docs/PRESENTATION.md  English presentation outline and diagrams
```

## Limits

- The project seeds two demo users; it does not yet include administrator user/role management or an email delivery service.
- An appointment can be created or edited, and repair orders can be created directly; converting an appointment into a repair order is not yet a dedicated action.
- Conflict checks cover assigned mechanics, not service bays.
- There is no external payment, customer notification, or public customer portal.
- Manual visual checks are not a substitute for full end-to-end automation or a deployment security review.

## Suggested student responsibilities

- Orazkhan Yelaman: data model, EF Core, scheduling/repair/inventory services, and related tests.
- Zharkyn Lukpan: Blazor shell, reusable page patterns, forms, dashboard, responsive styling, and UI tests.
- Shared: agree on design and data relationships, integrate changes, review each other's work, run workflows, document the project, and prepare to explain the full application.
