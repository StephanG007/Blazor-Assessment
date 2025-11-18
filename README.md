
# Clinic Booking System
## 🧭 Overview
This repository implements a simplified **Clinic Booking System** for the Department of Health and Wellness.  
It consists of a **Blazor WebAssembly frontend** and an **ASP.NET Core Web API backend**, with a shared `Contracts` project defining data models.

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

First, create a Dev certificate for HTTPS
```bash
dotnet dev-certs https --trust
```

Om Linux, this certificate will need to be registered on the OS:

<table>
<tr>
<td width="50%">

<pre><code class="language-bash">
// UBUNTU / DEBIAN
sudo cp ~/.dotnet/corefx/cryptography/x509stores/my/* /usr/local/share/ca-certificates/
sudo update-ca-certificates
</code></pre>

</td>
<td width="50%">

<pre><code class="language-bash">
// RPM / FEDORA
dotnet dev-certs https --export-path /tmp/aspnet-dev-cert.pem --format PEM
sudo trust anchor /tmp/aspnet-dev-cert.pem
</code></pre>

</td>
</tr>
</table>

To run API (from API folder):
```bash
dotnet restore
dotnet run
```

To run frontend (from UI folder):
```bash
dotnet restore
dotnet run
```

## 🔐 Login Instructions

To access the Blazor UI and test the system, log in with the following credentials:

- **Username:** `bruce@wayne.co.za`
- **Password:** `P@ssw0rd`

Once logged in, you can browse clinics, create bookings, and explore the system’s functionality.


