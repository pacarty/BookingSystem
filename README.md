# BookingSystem — starter solution

A generic multi-provider appointment booking system: clients book time with
providers for a given service; providers manage their schedule and mark
attendance. Built to demonstrate clean layering, EF Core, and ASP.NET Core
Web API patterns for a .NET developer portfolio.

## Solution layout

```
BookingSystem.sln
src/
  BookingSystem.Domain          - entities, enums. No dependencies on anything.
  BookingSystem.Application     - DTOs, interfaces, business logic (booking rules).
                                   Depends only on Domain.
  BookingSystem.Infrastructure  - EF Core DbContext, entity configurations,
                                   repositories, notification implementations.
                                   Depends on Application (implements its interfaces).
  BookingSystem.Api             - ASP.NET Core Web API. Controllers, DI wiring,
                                   Swagger. Depends on Application + Infrastructure.
tests/
  BookingSystem.UnitTests       - xUnit + Moq tests for the booking service.
clients/
  public-site                   - React (Vite) client-facing booking flow.
                                   No login required, by design - see below.
```

This is a simplified clean/onion architecture: dependencies point inward.
`Domain` knows nothing about EF Core or HTTP. `Application` defines
interfaces (`IAppointmentRepository`, `INotificationService`) that
`Infrastructure` implements — so swapping SQL Server for Postgres, or the
console notification stub for real SendGrid/Twilio calls, never touches
business logic or the API layer.

## Why this shape

- **`Provider` / `Client` / `Service` / `Appointment`** are deliberately
  generic names — the domain is a services-booking system (consultants,
  tutors, trainers, whoever), not tied to any one industry.
- **`ProviderService`** is a many-to-many join because not every provider
  offers every service, and pricing can vary by provider.
- **`Availability`** stores recurring weekly hours only. Bookable slots are
  *computed* at query time (weekly hours minus existing appointments) rather
  than stored — see `AppointmentBookingService.GetAvailableSlotsAsync`.
- **`NotificationLog`** and `INotificationService` exist from day one, but
  the shipped implementation (`ConsoleNotificationService`) just logs what
  it would send. This proves out the whole flow — booking triggers a
  notification, which is tracked — without needing a paid Twilio/SendGrid
  account. Wiring up a real provider later is a one-file change plus a DI
  registration; see the comments in `Program.cs` and
  `ConsoleNotificationService.cs`.
- **Conflict checking happens twice**: once when displaying available slots,
  and again at the moment of booking (`HasOverlapAsync`). This closes the
  race condition where two clients view the same open slot simultaneously —
  a detail worth mentioning if it comes up in an interview.
- **No login on the public site.** `POST /api/appointments` takes the
  client's name/email/phone directly rather than requiring a pre-existing
  `ClientId`. `AppointmentBookingService.BookAsync` finds an existing
  `Client` by email or creates one on the fly. This matches how the original
  system worked (patients never signed up for accounts) and keeps auth
  scoped to where it actually matters — see "what's next" below.

## Getting started

You'll need:
- .NET 10 SDK
- SQL Server LocalDB (ships with Visual Studio) or a SQL Server/Azure SQL instance
- Node.js 20+ (for the React frontends, once you build them)

### Visual Studio
1. Open `BookingSystem.sln`.
2. Right-click the solution → **Restore NuGet Packages**.
3. Set `BookingSystem.Api` as the startup project.
4. Open the Package Manager Console, select `BookingSystem.Infrastructure` as
   the default project, and run:
   ```
   Add-Migration InitialCreate -StartupProject BookingSystem.Api
   Update-Database -StartupProject BookingSystem.Api
   ```
5. Press F5. Swagger UI opens automatically in Development.

### CLI / VS Code
```bash
cd BookingSystem
dotnet restore
dotnet tool install --global dotnet-ef   # if you don't already have it
dotnet ef migrations add InitialCreate --project src/BookingSystem.Infrastructure --startup-project src/BookingSystem.Api
dotnet ef database update --project src/BookingSystem.Infrastructure --startup-project src/BookingSystem.Api
dotnet run --project src/BookingSystem.Api
```
Then browse to the printed `https://localhost:xxxx/swagger`.

### Running the tests
```bash
dotnet test
```

### Running the public booking site
```bash
cd clients/public-site
npm install
cp .env.example .env    # points VITE_API_BASE_URL at the API - edit if your port differs
npm run dev
```
Then open the printed `localhost` URL. Make sure the API is running first
(and check its actual HTTPS port in Visual Studio / `launchSettings.json` —
`.env.example` assumes `7100`, yours may differ), and that `Clients:PublicSiteUrl`
in `appsettings.json` matches the Vite dev server's URL so CORS allows it.

The flow: pick a service → pick a provider who offers it → pick a date and
time from the availability board → enter your details → confirmed. No
account or login needed, matching how the original patient-facing site
worked.

## What's here vs. what's next

**Implemented:**
- Full entity model + EF Core configuration
- Availability → slot calculation
- Booking creation with double-booking prevention and find-or-create client
- Status update endpoint (Confirmed/Attended/NoShow/Cancelled)
- Notification abstraction (console-logged by default)
- Unit tests for the booking rules
- Public booking site (React) — the full client-facing flow, no auth

**Deliberately left for the next phase** (see the project plan):
- ASP.NET Core Identity / JWT auth — needed for the *provider* admin site,
  not the public one (see the "no login" note above)
- The provider/admin React dashboard (view schedule, mark attendance)
- Real email sending (SendGrid — free tier, cheap to add)
- Real SMS (Twilio — costs a few cents per message; wire up only if/when
  you want a live demo)
- Recurring cancellations / time-off exceptions on top of weekly `Availability`
- Deployment to Azure App Service + Azure SQL

## A note on scaffolding

`dotnet ef migrations add` needs to run in an environment with the .NET SDK
and NuGet access, which this sandbox didn't have — so the migration itself
isn't included here. The model above is what `Add-Migration InitialCreate`
will generate tables from; running it locally is the very first thing to do
once you open this in Visual Studio or VS Code.
