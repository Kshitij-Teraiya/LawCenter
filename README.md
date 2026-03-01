# LegalConnect – Blazor WebAssembly Frontend

> A production-ready lawyer marketplace platform built with Blazor WebAssembly (.NET 8),
> Bootstrap 5, and JWT authentication. Connects clients with verified legal professionals.

---

## Table of Contents

1. [Tech Stack](#tech-stack)
2. [Project Structure](#project-structure)
3. [Prerequisites](#prerequisites)
4. [Setup & Run](#setup--run)
5. [Configuration](#configuration)
6. [Architecture Overview](#architecture-overview)
7. [Authentication Flow](#authentication-flow)
8. [Role-Based Access](#role-based-access)
9. [API Integration Contracts](#api-integration-contracts)
10. [Key Design Decisions](#key-design-decisions)

---

## Tech Stack

| Layer         | Technology                         |
|---------------|------------------------------------|
| Framework     | Blazor WebAssembly (.NET 8)        |
| UI Library    | Bootstrap 5.3 + Bootstrap Icons    |
| Fonts         | Google Fonts – Inter               |
| Auth          | JWT (stored in LocalStorage)       |
| HTTP          | Named HttpClient + DelegatingHandler |
| State         | Scoped services (DI)               |
| Storage       | Blazored.LocalStorage              |
| JWT Parsing   | System.IdentityModel.Tokens.Jwt    |
| Form Validation | DataAnnotations + EditForm       |

---

## Project Structure

```
LegalConnect.Client/
├── Pages/
│   ├── Auth/
│   │   ├── Login.razor               # /login
│   │   ├── RegisterClient.razor      # /register-client
│   │   ├── RegisterLawyer.razor      # /register-lawyer
│   │   └── ForgotPassword.razor      # /forgot-password
│   ├── Client/
│   │   ├── ClientDashboard.razor     # /client/dashboard    [Client]
│   │   ├── MyAppointments.razor      # /client/appointments [Client]
│   │   └── PaymentHistory.razor      # /client/payments     [Client]
│   ├── Lawyer/
│   │   ├── LawyerDashboard.razor     # /lawyer/dashboard    [Lawyer]
│   │   ├── ManageProfile.razor       # /lawyer/profile      [Lawyer]
│   │   ├── LawyerAppointments.razor  # /lawyer/appointments [Lawyer]
│   │   └── EarningsSummary.razor     # /lawyer/earnings     [Lawyer]
│   ├── Admin/
│   │   ├── AdminDashboard.razor      # /admin/dashboard     [Admin]
│   │   ├── ApproveLawyers.razor      # /admin/approve-lawyers
│   │   ├── ManageCategories.razor    # /admin/categories
│   │   ├── CommissionSettings.razor  # /admin/commission
│   │   └── RevenueStats.razor        # /admin/revenue
│   ├── Home.razor                    # /
│   ├── LawyerListing.razor           # /lawyers
│   ├── LawyerProfilePage.razor       # /lawyers/{id}
│   └── AppointmentBooking.razor      # /book/{lawyerId}     [Client]
│
├── Services/
│   ├── IAuthService.cs + AuthService.cs
│   ├── ILawyerService.cs + LawyerService.cs
│   ├── IAppointmentService.cs + AppointmentService.cs
│   ├── IAdminService.cs + AdminService.cs
│   └── ToastService.cs
│
├── Models/
│   ├── Auth/     (LoginDto, RegisterClientDto, RegisterLawyerDto, AuthResponseDto, …)
│   ├── Lawyer/   (LawyerDto, LawyerSummaryDto, LawyerFilterDto, CategoryDto, …)
│   ├── Appointment/ (AppointmentDto, BookAppointmentDto, TimeSlotDto, …)
│   └── Admin/    (PendingLawyerDto, RevenueStatsDto, CommissionSettingDto, …)
│
├── Shared/
│   ├── MainLayout.razor              # Full layout with navbar + sidebar
│   ├── MinimalLayout.razor           # Auth pages layout (no sidebar)
│   ├── NavMenu.razor                 # Role-based sidebar nav
│   └── AppFooter.razor
│
├── Components/
│   ├── LawyerCard.razor              # Lawyer listing card
│   ├── RatingStars.razor             # ★★★★☆ display
│   ├── Pagination.razor              # Page navigator
│   ├── ConfirmationModal.razor       # Delete/action confirm dialog
│   ├── LoadingSpinner.razor          # Centered spinner
│   └── ToastContainer.razor          # Global toast notifications
│
├── Helpers/
│   ├── JwtAuthenticationStateProvider.cs   # Custom Blazor auth provider
│   ├── AuthorizationMessageHandler.cs      # Attaches Bearer token
│   ├── RedirectToLogin.razor               # Unauthenticated redirect
│   └── ApiResponse.cs                      # API envelope + PagedResult<T>
│
└── wwwroot/
    ├── index.html
    ├── appsettings.json
    └── css/app.css
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js (optional – only if you run a bundler step)
- Your ASP.NET Core 8 Web API running on `https://localhost:7001`

---

## Setup & Run

### 1. Clone / Copy the project

```bash
cd C:\Project\Lawyer_Claude\LegalConnect.Client
```

### 2. Restore NuGet packages

```bash
dotnet restore
```

### 3. Configure the API base URL

Edit `wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001/api/"
}
```

Replace `https://localhost:7001` with your actual backend URL.

### 4. Run the Blazor WASM app

```bash
dotnet run
```

The app opens at `https://localhost:5001` (or the port shown in the console).

---

## Configuration

### appsettings.json

| Key         | Description                                    |
|-------------|------------------------------------------------|
| `ApiBaseUrl`| Base URL of the ASP.NET Core 8 Web API         |

### Named HttpClients

Two `HttpClient` instances are registered in `Program.cs`:

| Name       | Auth header | Use case               |
|------------|-------------|------------------------|
| `public`   | None        | Login, register, search |
| `secured`  | Bearer JWT  | Authenticated endpoints |

---

## Architecture Overview

```
Component / Page
     │
     ▼
Service Interface (IAuthService, ILawyerService, …)
     │
     ▼
Service Implementation (uses IHttpClientFactory)
     │
     ▼
Named HttpClient  ──►  AuthorizationMessageHandler  ──►  ASP.NET Core API
                           (reads JWT from LocalStorage,
                            sets Authorization: Bearer …)
```

**Auth state flow:**

```
Browser LocalStorage
     │  jwt_token
     ▼
JwtAuthenticationStateProvider
     │  parses claims from JWT
     ▼
CascadingAuthenticationState  ──►  AuthorizeView / [Authorize]
```

---

## Authentication Flow

1. **Login**: `POST /api/auth/login` → returns `{ token, role, … }`
2. Token stored in `localStorage` via `Blazored.LocalStorage`
3. `JwtAuthenticationStateProvider` reads token, parses claims (including role)
4. `AuthorizationMessageHandler` attaches `Authorization: Bearer <token>` to every `secured` client request
5. On logout: token removed from storage; auth state reset to anonymous
6. Token expiry: checked on each `GetAuthenticationStateAsync()` call; expired tokens auto-clear

---

## Role-Based Access

| Role   | Routes                                    | Nav Sections             |
|--------|-------------------------------------------|--------------------------|
| Client | `/client/*`, `/book/*`                    | Client section           |
| Lawyer | `/lawyer/*`                               | Lawyer section           |
| Admin  | `/admin/*`                                | Administration section   |
| Public | `/`, `/lawyers`, `/lawyers/{id}`, auth pages | General section        |

Routes use `@attribute [Authorize(Roles = "X")]`.
`App.razor` handles `<NotAuthorized>` redirect to `/login`.

---

## API Integration Contracts

All API responses are expected in this envelope:

```json
{
  "success": true,
  "message": "...",
  "data": { ... },
  "errors": []
}
```

Paginated lists:

```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 12,
    "totalPages": 9
  }
}
```

### Key Backend Endpoints Expected

| Method | Endpoint                            | Auth  |
|--------|-------------------------------------|-------|
| POST   | /api/auth/login                     | None  |
| POST   | /api/auth/register/client           | None  |
| POST   | /api/auth/register/lawyer           | None  |
| POST   | /api/auth/forgot-password           | None  |
| GET    | /api/lawyers?{filter}               | None  |
| GET    | /api/lawyers/{id}                   | None  |
| GET    | /api/lawyers/me                     | Lawyer|
| PUT    | /api/lawyers/me                     | Lawyer|
| POST   | /api/lawyers/me/experiences         | Lawyer|
| GET    | /api/categories                     | None  |
| GET    | /api/appointments/slots?…           | None  |
| POST   | /api/appointments                   | Client|
| GET    | /api/appointments/my                | Client|
| GET    | /api/appointments/lawyer            | Lawyer|
| PUT    | /api/appointments/{id}/confirm      | Lawyer|
| GET    | /api/admin/lawyers/pending          | Admin |
| PUT    | /api/admin/lawyers/{id}/approve     | Admin |
| GET    | /api/admin/commission               | Admin |
| PUT    | /api/admin/commission               | Admin |
| GET    | /api/admin/revenue                  | Admin |

---

## Key Design Decisions

- **No Blazor Server** – pure WASM for offline capability and reduced server load
- **Two HttpClients** – `public` (no auth) vs. `secured` (with Bearer) avoids token leaks on public routes
- **`JwtAuthenticationStateProvider`** – parses JWT client-side; no round-trip needed to validate session
- **`ToastService`** – singleton bus pattern; any component can trigger toasts without cascading parameters
- **`ApiResponse<T>` envelope** – standardizes success/error handling across all service calls
- **`PagedResult<T>`** – pagination metadata included in every list response
- **`MinimalLayout`** for auth pages – no sidebar/navbar clutter during login/register
- **Bootstrap 5 utility classes** + custom CSS variables (`--lc-*`) for consistent theming
- **`ConfirmationModal`** is a ref-based component; parent calls `.Show()` / `.Hide()` directly

---

## Adding a New Page (Checklist)

1. Create `Pages/{Section}/MyPage.razor`
2. Add `@page "/my-route"` directive
3. Add `@attribute [Authorize(Roles = "Role")]` if protected
4. Add nav link in `Shared/NavMenu.razor` wrapped in `<AuthorizeView Roles="…">`
5. Inject required services
6. Use `LoadingSpinner` while async data loads
7. Use `ToastService` for feedback
8. Use `ConfirmationModal` for destructive actions

---

*Generated by LegalConnect Architect – February 2026*
