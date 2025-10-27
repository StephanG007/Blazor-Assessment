# Clinic Booking System
## 🧭 Overview
This repository implements a simplified **Clinic Booking System** for the Department of Health and Wellness.  
It consists of a **Blazor WebAssembly frontend** and an **ASP.NET Core Web API backend**, with a shared `Contracts` project defining data models.

The purpose of this README is to help **Codex** understand:
- how to build, run, and test the project, and  
- how to automatically fix or iterate on failed builds.

---

## 🧩 Architecture Summary

| Component | Description |
|------------|-------------|
| `API/` | ASP.NET Core Web API that exposes endpoints for users, clinics, and bookings. |
| `UI/` | Blazor WebAssembly frontend for booking appointments. |
| `Contracts/` | Shared DTOs and models for request/response data. |
| `Tests/` | xUnit-based tests for backend booking logic. |

Key libraries:
- **Entity Framework Core** for persistence
- **MudBlazor** for UI
- **JWT authentication**
- **xUnit + Moq** for testing

---

## 🧱 Build Instructions

All .NET projects are built using **.NET 9.0** (or **.NET 8.0** if 9.0 SDK unavailable).

```bash
dotnet restore
dotnet build --configuration Release
