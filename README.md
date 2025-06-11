# 🚌 BusBooking — Bus Tickets Booking System

## 📌 About the Project

**BusBooking** is a backend web application developed using **ASP.NET Core MVC**. It is designed to support a full-featured bus ticket booking system where users will eventually be able to choose routes, dates, return times, and specific seats. Although only the server-side logic is implemented at this stage, the project is structured to be ready for frontend integration.

> 🎨 I have created a design for the frontend interface. Screenshots are available in the `/design` folder and can be used as a reference for future development.

---

## 🖼️ UI Design Preview

_Design previews will be added here. Example:_

![Homepage Design](BusBooking/BusBooking/design/Homepage.jpg.jpg)
![Seat Selection](./design/seat-selection.png)

---

## 🛠️ Current Functionality (Backend)

- Management of **routes**, **buses**, **schedules**, and **seat reservations**
- MVC architecture with Controllers, Models, and Views
- Entity Framework Core for data access and migrations
- Database structure for users, tickets, routes, and admin content
- Admin role can:
  - Add/edit/delete routes
  - Manage bus schedules
  - View ticket reservations
  - Post weather and route announcements
- Basic support for:
  - Ticket bookings (server-side)
  - Viewing available routes and schedule information

---

## 📦 Tech Stack

- 🧠 **ASP.NET Core MVC** — server-side web framework
- 🗄️ **Entity Framework Core** — ORM for database access
- 🧱 **MSSQL Server** or **PostgreSQL** — database system
- 🧾 **Razor Views** (optional for development)
- 🔐 **Identity & Roles** — user and admin authentication/authorization
- 🌐 REST API endpoints (planned for future frontend integration)

---

## 🚧 Project Status

- [x] Project structure and database schema
- [x] Admin dashboard and route management
- [x] Models for tickets, users, buses, and schedules
- [ ] REST API for external frontend use
- [ ] User booking flow
- [ ] Integration with maps and weather APIs
- [ ] Frontend (Next.js + TypeScript) — _planned_

---

## 📁 Project Structure

/BusBooking
│
├── Controllers # ASP.NET MVC controllers
├── Models # Data models (EF Core)
├── Views # Razor views (admin panel)
├── Data # Database context and migrations
├── wwwroot # Static files
└── design # Screenshots or design references
