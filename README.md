# Clinic Booking System – Practical Assessment

## 📋 Scenario Overview

You are tasked with designing and implementing a simplified **Clinic Booking System** for the **Department of Health and Wellness**.

The system should allow patients to:

- Book appointments at clinics  
- View available time slots  
- Receive confirmation messages  

---

## 🧩 Assessment Tasks

### 1. Implementation

Build a solution using **Blazor WebAssembly (WASM)** that includes:

- **Booking logic**
- **Time slot availability**
- **Confirmation messages**
- **Basic validation** (e.g., prevent double bookings)

### 2. Code Quality

- Follow **SOLID principles**
- Use **meaningful naming conventions**
- Implement **basic unit testing**

---

## ⚙️ Technology Stack Overview

### Backend – ASP.NET Core Web API
- A **single controller** responsible for booking logic.
- Uses a **local database** via **Entity Framework Core**.
- Must handle:
  - **Basic availability checks**
  - **Validation** to prevent double bookings

### Frontend – Blazor WebAssembly
- A **single-page interface** for patients to:
  - Select a clinic and date
  - View available time slots
  - Submit bookings
- Recommended libraries and features:
  - **MudBlazor** for UI components
  - **HttpClient** for API communication
  - **JWT Token Authentication**
- Emphasis on **clean, readable code** and **meaningful naming**

---

## 🧪 Unit Testing

Implement **at least one** critical unit test for the booking logic using:

- **xUnit** for testing
- **Moq** (or a similar mocking library) for dependency mocking

---

## ✅ Deliverables Summary

| Area | Requirement |
|------|--------------|
| Framework | Blazor WebAssembly (Frontend), ASP.NET Core Web API (Backend) |
| Database | Local EF Core database |
| Authentication | JWT-based |
| UI Components | MudBlazor |
| Testing | xUnit + Moq |
| Design Principles | SOLID, Clean Code, Meaningful Naming |
| Validation | Prevent double bookings |

---

## 💡 Notes

- Keep the architecture simple and modular.
- Focus on demonstrating **clarity, structure, and maintainability** over feature breadth.
- Use **modern C# patterns** and concise data models.
- The goal is to showcase both **frontend and backend proficiency**.
