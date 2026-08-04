# BookingSystem

A generic multi-provider appointment booking system: clients book time with
providers for a given service; providers manage their schedule and mark
attendance. Built to demonstrate clean layering, EF Core, and ASP.NET Core
Web API patterns for a .NET developer portfolio.

## Live demo

- **Public booking site:** https://blue-ground-046726800-preview.eastasia.7.azurestaticapps.net
- **Staff dashboard:** https://wonderful-meadow-08b0a6900-preview.eastasia.7.azurestaticapps.net
- **API (Swagger):** https://bookingsystem-api-pat-cyavgwezf6decwhk.australiaeast-01.azurewebsites.net/swagger

Demo staff logins for the dashboard:
- Admin: `admin@bookingsystem.local` / `Passw0rd!123`
- Provider ("Jordan Blake"): `provider@bookingsystem.local` / `Passw0rd!123`

**Note on first load:** the API runs on Azure's free App Service tier, which
spins down after ~20 minutes of inactivity to keep hosting cost at $0. The
first request after a period of idle time can take 10–30 seconds to respond
while it cold-starts back up — an intentional cost trade-off for a
portfolio project, not a bug. A refresh (or just waiting it out) resolves it.

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
  admin-site                    - React (Vite) staff dashboard. Login, view
                                    appointments, and update their status.
                                    JWT-authenticated - see "Auth" below.
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
  scoped to where it actually matters — see "Auth" below.

## Auth

Only staff (Providers and Admins) authenticate — clients never do. ASP.NET
Core Identity tables live in the same database as the domain tables
(`BookingSystemDbContext` is now an `IdentityDbContext`), and login issues a
JWT rather than a cookie, since the admin site is a separate SPA calling the
API cross-origin.

- **Roles:** `Admin`, `Provider`.
- **`ApplicationUser.ProviderId`** links a Provider-role account to its
  `Provider` row. It's how `GET /api/appointments/mine` and the ownership
  check in `PATCH /api/appointments/{id}/status` know whose data is whose,
  straight from a claim on the token — no extra database lookup.
- **No self-registration.** Staff accounts are created by an admin (or, for
  now, by `DevelopmentSeeder`) — there's deliberately no
  `POST /api/auth/register`.
- **Demo credentials** (created automatically on startup, Development only):
  - Admin: `admin@bookingsystem.local` / `Passw0rd!123`
  - Provider (linked to the seeded demo provider "Jordan Blake"):
    `provider@bookingsystem.local` / `Passw0rd!123`

  None of this seeding runs outside `Development` — a real deployment
  creates its first admin through a proper (out-of-band) process, not a
  hardcoded password shipped in source control.
- Try it in Swagger: `POST /api/auth/login` with one of the demo logins,
  copy the returned token, click **Authorize** at the top of the Swagger
  page, paste it in as `Bearer <token>`, and the `[Authorize]`-protected
  endpoints (`GET /api/appointments/mine`, `PATCH /api/appointments/{id}/status`)
  will work.

## Getting started

You'll need:
- .NET 10 SDK
- SQL Server LocalDB (ships with Visual Studio) or a SQL Server/Azure SQL instance
- Node.js 20+ (for the React frontends)

### Visual Studio
1. Open `BookingSystem.sln`.
2. Right-click the solution → **Restore NuGet Packages**.
3. Set `BookingSystem.Api` as the startup project.
4. Open the Package Manager Console, select `BookingSystem.Infrastructure` as
   the default project, and run:
   ```
   Update-Database -StartupProject BookingSystem.Api
   ```
5. Press F5. Swagger UI opens automatically in Development.

### CLI / VS Code
```bash
cd BookingSystem
dotnet restore
dotnet tool install --global dotnet-ef   # if you don't already have it
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
Then open the printed `localhost` URL (defaults to port 5173). Make sure the API is running first (and check its actual HTTPS port in Visual Studio / `launchSettings.json` —
`.env.example` assumes `7100`, yours may differ), and that `Clients:PublicSiteUrl`
in `appsettings.json` matches the Vite dev server's URL so CORS allows it.

The flow: pick a service → pick a provider who offers it → pick a date and
time from the availability board → enter your details → confirmed. No
account or login needed, matching how the original patient-facing site
worked.

### Running the admin dashboard
```bash
cd clients/admin-site
npm install
cp .env.example .env    # points VITE_API_BASE_URL at the API - edit if your port differs
npm run dev
```

Runs on a fixed port, 5174 (not Vite's default 5173), so it doesn't collide
with the public site if you're running both at once, and so it matches
`Clients:AdminSiteUrl` in `appsettings.json`. Log in with either demo
account above — Admin sees every appointment across all providers, Provider
sees only their own — and confirm/mark attendance/cancel from there.

## What's here vs. what's next

**Implemented:**
- Full entity model + EF Core configuration
- Availability → slot calculation
- Booking creation with double-booking prevention and find-or-create client
- Status update endpoint (Confirmed/Attended/NoShow/Cancelled), with role- and ownership-based authorization, and validated status transitions
- ASP.NET Core Identity + JWT auth for Provider/Admin accounts
- Notification abstraction (console-logged by default)
- Unit tests for the booking rules
- Public booking site (React) — the full client-facing flow, no auth
- Provider/admin React dashboard — login, view appointments (scoped to own schedule for Providers, all appointments for Admins), and confirm/attend/no-show/cancel actions with server-side transition validation
- Live deployment: API on App Service, database on Azure SQL, both
  frontends on Static Web Apps (see "Live demo" above)

**Deliberately left for the next phase:**:
- Real email sending (SendGrid — free tier, cheap to add)
- Real SMS (Twilio — costs a few cents per message; wire up only if/when
  you want a live demo)
- Recurring cancellations / time-off exceptions on top of weekly `Availability`
- Admin-driven staff onboarding — currently the only way a Provider
  account gets created is the hardcoded `DevelopmentSeeder`; there's no
  real "Admin adds a new Provider" flow yet (create the `Provider` row,
  create the linked `ApplicationUser`, assign the `Provider` role, get
  them a way to set their own password)
- Appointment reminders — `INotificationService.SendAppointmentReminderAsync`
  exists and is implemented, but nothing calls it yet. Needs a scheduled
  background job (an ASP.NET Core `BackgroundService`, or a separate
  Azure Function on a timer trigger) that periodically checks for
  upcoming appointments and fires reminders for them.
