# HomeCare — Smart Home Maintenance Tracker

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-blue.svg)](https://learn.microsoft.com/aspnet/core)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0.12-68217a.svg)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB%20%2F%20Express-CC292B.svg)](https://www.microsoft.com/sql-server)
[![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952b3.svg)](https://getbootstrap.com/)
[![Visual Studio](https://img.shields.io/badge/IDE-Microsoft%20Visual%20Studio-5C2D91.svg)](https://visualstudio.microsoft.com/)

A modern, centralized household appliance and home maintenance management platform built with **ASP.NET Core MVC**, **C# .NET 10**, **Entity Framework Core**, **SQL Server**, and **Bootstrap 5**. Designed to run natively inside **Microsoft Visual Studio**.

---

## Table of Contents
1. [Project Overview](#project-overview)
2. [Key Features](#key-features)
3. [Technology Stack](#technology-stack)
4. [Architecture & Design](#architecture--design)
5. [Database Structure & ER Diagram](#database-structure--er-diagram)
6. [Visual Studio Run Instructions (F5 Workflow)](#visual-studio-run-instructions-f5-workflow)
7. [SQL Server & LocalDB Configuration](#sql-server--localdb-configuration)
8. [Database Migrations Guide](#database-migrations-guide)
9. [Application Workflow](#application-workflow)
10. [Security & User Data Isolation](#security--user-data-isolation)
11. [Troubleshooting & FAQs](#troubleshooting--faqs)

---

## Project Overview

Modern homes rely on an increasing array of electronics, HVAC units, kitchen appliances, and water systems. However, warranty documentation, invoices, technician receipts, and service schedules are often scattered across drawers or lost.

**HomeCare** solves this by providing homeowners and property managers with a centralized command center to:
* Track multiple households (primary homes, rental flats, vacation villas, offices).
* Monitor appliance lifecycles, purchase details, models, and serial numbers.
* Track active warranties with dynamic real-time countdown alerts.
* Maintain complete maintenance histories, technician logs, and expenditure breakdowns.
* Securely store digital invoices, manuals, and warranty receipts (PDF/images) with anti-traversal protection.
* Schedule recurring preventive maintenance and view real-time countdowns.
* Compute **Appliance Health Scores (0–100)** through deterministic rule-based algorithms.
* Generate and print **scannable physical QR Code stickers** for every appliance.
* Gain actionable financial insights through interactive **Chart.js** charts.

---

## Key Features

### 1. Multi-Home Management
* Seamlessly manage multiple properties (e.g., *Primary Residence*, *Parents' Home*, *Rental Apartment*).
* Filter dashboards, appliances, and maintenance logs by active property or view an aggregate summary.
* Dedicated **Household Health Overview** displaying property-specific analytics, spending, and most-serviced hardware.

### 2. Appliance Management
* Complete hardware specifications: Name, Brand, Model Number, Serial Number, Category, Room Location, Purchase Date, Purchase Price, Operational Status, and Photo.
* Operational statuses: `Active`, `Under Repair`, `Retired`, `Sold`.
* Global search by name, brand, model, serial number, and room location, with multi-criteria filters.

### 3. Appliance Health Score (0–100)
* Deterministic scoring engine based on:
  * Hardware age and expected lifecycle.
  * Warranty coverage status (+5 active bonus, -10 expired penalty).
  * Preventive service regularity within the past 6–12 months.
  * Breakdown and repair frequency (penalties for &ge;2 repairs in 12 months).
  * Overdue tasks or unperformed scheduled servicing.
* Clear categorization: **Excellent (90–100)**, **Good (70–89)**, **Needs Attention (40–69)**, and **Critical (0–39)**.
* *Note: Labelled clearly as "HomeCare Maintenance Health Indicator" (Rule-based guidance, not a mechanical engineering diagnosis).*

### 4. Smart Maintenance Insights
* Deterministic rule-based intelligence alerting users to:
  * "Warranty expires in X days. Keep your invoice ready."
  * "This appliance has not been serviced recently."
  * "Multiple repair records logged this year. Component stress detected."
  * "Maintenance spending has increased this year."

### 5. Warranty Tracking & Countdown
* Dynamic calculation of days remaining until expiration (`245 days remaining`, `18 days remaining`, `Expired 34 days ago`).
* Expiration color badges: **Green (Active >30d)**, **Yellow (Expiring Soon &le;30d)**, **Red (Expired)**.
* Tracks warranty provider, policy/contract number, coverage scope, customer service hotlines, and terms.

### 6. Service Records & Visual Timeline
* Comprehensive service logging: Service Date, Service Type (Routine, Repair, Filter, Inspection), Technician Name, Agency, Cost, and Next Recommended Date.
* Interactive chronological **Lifecycle Timeline**: `Purchase &rarr; Warranty Started &rarr; Service #1 &rarr; Repair &rarr; Next Scheduled Service`.

### 7. Secure Document Vault
* Upload and preview purchase invoices, warranty certificates, service receipts, and user manuals.
* **Strict Security:** Allowed extensions (`.pdf`, `.jpg`, `.jpeg`, `.png`), MIME validation, 10MB size cap, randomized server-side filenames (`Guid.NewGuid()`), and path traversal prevention.
* Inline PDF/image preview in browser or secure download.

### 8. Automated Reminders & Notification Center
* Automated scanning for upcoming warranty expirations (30d, 15d, 7d, 1d) and scheduled maintenance due dates.
* Topbar notification bell with live unread badge and dropdown preview.
* Notification management: Mark as read, mark all read, and clear history.

### 9. Appliance QR Code Tagging
* High-resolution, scannable QR codes generated natively via `QRCoder` (`/Appliance/Details/{id}`).
* QR modal with Print Tag preview and PNG download for physical sticker printing.
* Zero external API reliance; completely self-contained.

### 10. Financial & Lifecycle Analytics
* Powered by **Chart.js**:
  * Monthly Maintenance Expenses (Last 12 months).
  * Appliance Distribution by Category (Doughnut).
  * Highest Maintenance Spending by Hardware (Horizontal Bar).
  * Warranty Status Distribution (Doughnut).
  * Hardware Age Lifecycle Distribution (Bar).

---

## Technology Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core MVC (.NET 10.0) |
| **Language** | C# 13 |
| **Data Access** | Entity Framework Core 10.0.12 (Code-First) |
| **Database** | Microsoft SQL Server (LocalDB / Express / Enterprise) |
| **Authentication** | ASP.NET Core Cookie Authentication & PBKDF2 Password Hasher |
| **Frontend UI** | Razor Views, Bootstrap 5.3, Bootstrap Icons, Custom CSS |
| **Charts** | Chart.js 4.4 |
| **QR Generation**| QRCoder 1.8.0 |
| **Target IDE** | Microsoft Visual Studio 2022 / 2025 |

---

## Architecture & Design

HomeCare follows the classic ASP.NET Core Model-View-Controller (MVC) architectural pattern:

```text
HomeCare/
├── Controllers/         # MVC Controllers handling routing, business rules, and security
│   ├── AccountController.cs         # Registration, login, logout, profile
│   ├── AnalyticsController.cs       # Chart.js metrics and financial KPIs
│   ├── ApplianceController.cs       # Appliance CRUD, tabs, QR code generation
│   ├── CategoryController.cs        # Category management
│   ├── DashboardController.cs       # Dashboard metrics, alerts, home switcher
│   ├── DocumentController.cs        # Secure file upload, preview, download
│   ├── HomeController.cs            # Landing page, privacy, 404/403/500 errors
│   ├── HomesController.cs           # Multi-home CRUD and household health overview
│   ├── NotificationController.cs    # Notification center and API endpoints
│   ├── ReminderController.cs        # Task scheduling and completion toggling
│   ├── ServiceRecordController.cs   # Service logging and maintenance costs
│   └── WarrantyController.cs        # Warranty policy registration and tracking
│
├── Data/                # EF Core DbContext and database seeders
│   ├── HomeCareDbContext.cs         # Entity relationships, cascade rules, precision, indexes
│   └── DbInitializer.cs             # Automatic seeding of standard appliance categories
│
├── Migrations/          # EF Core migration history files
│   ├── 20260929111103_InitialCreate.cs
│   └── 20261002063742_UpgradeHomeCareSchema.cs
│
├── Models/              # Core Domain Entities
│   ├── User.cs                      # Registered users (PasswordHash, Email, Role)
│   ├── Home.cs                      # Properties belonging to a user
│   ├── Category.cs                  # Appliance classification (Kitchen, HVAC, etc.)
│   ├── Appliance.cs                 # Hardware specs, status, location, health
│   ├── Warranty.cs                  # Provider, dates, coverage, contact number
│   ├── ServiceRecord.cs             # Date, type, technician, cost, next service
│   ├── Document.cs                  # Secure path, MIME type, file size
│   ├── Reminder.cs                  # Maintenance task, due date, completion status
│   └── Notification.cs              # User alerts and system notices
│
├── Services/            # Business Logic & Infrastructure Services
│   ├── IPasswordService.cs / PasswordService.cs
│   ├── IUserContext.cs / UserContext.cs
│   ├── IApplianceAuthorizationService.cs / ApplianceAuthorizationService.cs
│   ├── IFileService.cs / FileService.cs
│   ├── IQrCodeService.cs / QrCodeService.cs
│   ├── IHealthScoreService.cs / HealthScoreService.cs
│   └── INotificationService.cs / NotificationService.cs
│
├── ViewModels/          # Strongly-typed presentation view models
├── Views/               # Razor Views and UI Components
│   ├── Account/
│   ├── Analytics/
│   ├── Appliance/
│   ├── Category/
│   ├── Dashboard/
│   ├── Home/
│   ├── Homes/
│   ├── Notification/
│   ├── Reminder/
│   ├── ServiceRecord/
│   ├── Shared/
│   └── Warranty/
│
├── wwwroot/             # Static web assets (CSS, JS, uploads, icons)
├── HomeCare.slnx        # Visual Studio XML Solution File
├── HomeCare.csproj      # .NET 10 project file
├── Program.cs           # Web application bootstrap, middleware, DI setup
├── appsettings.json     # Production / default settings
└── appsettings.Development.json # Development LocalDB connection string
```

---

## Database Structure & ER Diagram

The database maintains strict relational integrity with cascade deletes and performance indexes:

```text
User (1)
 │
 ├── (N) Home
 │        │
 │        └── (N) Appliance (1) ──── (1) Warranty
 │                 │
 │                 ├── (N) ServiceRecord
 │                 ├── (N) Document
 │                 └── (N) Reminder
 │
 ├── (N) Notification
 └── (N) Reminder (Direct User FK for fast querying)

Category (1) ─────────── (N) Appliance
```

### Relational Schema Rules
1. **User &rarr; Homes:** One user can own multiple homes (`1:N`, Cascade delete).
2. **Home &rarr; Appliances:** One home contains multiple appliances (`1:N`, Cascade delete).
3. **Category &rarr; Appliances:** One category groups multiple appliances (`1:N`, Restrict delete to prevent orphaned items).
4. **Appliance &rarr; Warranty:** One appliance has one optional warranty policy (`1:1`, Cascade delete).
5. **Appliance &rarr; ServiceRecords:** One appliance has multiple historical service logs (`1:N`, Cascade delete).
6. **Appliance &rarr; Documents:** One appliance stores multiple uploaded receipts/manuals (`1:N`, Cascade delete).
7. **Appliance &rarr; Reminders:** One appliance has multiple scheduled tasks (`1:N`, Cascade delete).
8. **User &rarr; Notifications:** One user receives multiple alerts (`1:N`, Cascade delete).

### Optimized Database Indexes
* `Users`: Unique index on `Email`.
* `Homes`: Index on `UserId`.
* `Appliances`: Indexes on `HomeId`, `CategoryId`, `SerialNumber`, and `Status`.
* `Warranties`: Index on `EndDate`.
* `ServiceRecords`: Indexes on `ServiceDate` and `NextServiceDate`.
* `Reminders`: Indexes on `ReminderDate` and `UserId`.
* `Notifications`: Composite index on `(UserId, IsRead)`.

---

## Visual Studio Run Instructions (F5 Workflow)

This project is built and optimized specifically to run inside **Microsoft Visual Studio 2022 or Visual Studio 2025**.

### Prerequisites
1. **Microsoft Visual Studio 2022 / 2025** with the **ASP.NET and web development** workload installed.
2. **.NET 10 SDK** (Installed automatically with latest Visual Studio or from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0)).
3. **SQL Server Express LocalDB** (Included with Visual Studio by default).

### Step-by-Step Execution
1. **Open the Solution:**
   * Double-click `HomeCare.slnx` in the project root to open Microsoft Visual Studio.
2. **Restore NuGet Packages:**
   * Visual Studio automatically restores dependencies upon opening. If prompted, right-click the solution in Solution Explorer and choose **Restore NuGet Packages**.
3. **Build the Solution:**
   * Press `Ctrl + Shift + B` or click **Build &rarr; Build Solution**.
   * Output should report: `Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped`.
4. **Run the Application:**
   * Set `HomeCare` as the Startup Project.
   * Press **F5** (or **Ctrl + F5** for non-debugging run).
   * Your browser will automatically launch at `https://localhost:7124` or `http://localhost:5117`.
5. **Get Started:**
   * Click **Get Started** or **Register**.
   * Create an account (e.g. `user@example.com` / `Password123!`).
   * Your default primary residence is created automatically, and you are directed to your **Household Dashboard**.

---

## SQL Server & LocalDB Configuration

The application uses connection string `HomeCareConnection` in `appsettings.json` and `appsettings.Development.json`.

### Default: Visual Studio LocalDB (Recommended for Local Dev)
```json
"ConnectionStrings": {
  "HomeCareConnection": "Server=(localdb)\\mssqllocaldb;Database=HomeCareDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Option 2: SQL Server Express (`.\SQLEXPRESS`)
If your machine runs SQL Server Express under an instance name like `SQLEXPRESS`:
```json
"ConnectionStrings": {
  "HomeCareConnection": "Server=.\\SQLEXPRESS;Database=HomeCareDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Option 3: Full SQL Server / Docker SQL Server
```json
"ConnectionStrings": {
  "HomeCareConnection": "Server=localhost,1433;Database=HomeCareDB;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

---

## Database Migrations Guide

All database tables and seed categories are automatically applied upon application startup via `db.Database.Migrate()` in `Program.cs`.

If you prefer using the **Visual Studio Package Manager Console (PMC)**:
1. Open Visual Studio: **Tools &rarr; NuGet Package Manager &rarr; Package Manager Console**.
2. To apply all migrations:
   ```powershell
   Update-Database
   ```
3. To add a new migration in the future:
   ```powershell
   Add-Migration YourMigrationName
   Update-Database
   ```

Or using the .NET CLI:
```bash
dotnet ef database update
```

---

## Application Workflow

```text
Landing Page
    │
    ▼
Register / Login
    │
    ▼
Dashboard (Multi-Home Switcher)
    │
    ├── [+ Add Property] ──> Enter Residence Name & Address
    │
    ├── [+ Add Appliance] ──> Category, Brand, Model, S/N, Location, Purchase Info & Photo
    │         │
    │         ▼
    │   Appliance Details Profile
    │         │
    │         ├── Overview Tab ──> Health Diagnostics & Scoring Factors
    │         │
    │         ├── Warranty Tab ──> Expiration Countdown, Policy #, Hotline & Coverage
    │         │
    │         ├── Service Tab ──> Visual Timeline, Log Routine Service or Repair ($)
    │         │
    │         ├── Documents Tab ──> Secure Upload of Invoices / PDF Manuals & Previews
    │         │
    │         ├── Reminders Tab ──> Filter Cleaning, Inspections, Checkbox Completion
    │         │
    │         └── QR Tag Modal ──> Instant Scannable QR Sticker (Print / Download PNG)
    │
    ├── [Notification Center] ──> Automated 30d/15d/7d Warranty Warnings & Overdue Notices
    │
    ├── [Household Health Overview] ──> Aggregate Property Health & Most Serviced Hardware
    │
    └── [Analytics Hub] ──> Chart.js Monthly Cost Trends, Category Doughnut & Lifetime Spending
```

---

## Security & User Data Isolation

HomeCare enforces comprehensive defense-in-depth security:

1. **Strict User Data Isolation:**
   * User A can **never** view or alter User B's properties, appliances, documents, warranties, or service records.
   * Every controller checks ownership through `IApplianceAuthorizationService`:
     ```csharp
     db.Appliances.AnyAsync(a => a.Id == applianceId && a.Home.UserId == currentUserId);
     ```
   * Access attempts across accounts return `HTTP 403 Forbidden` (`/Account/AccessDenied`).
2. **Secure Passwords:**
   * Utilizes ASP.NET Core's NIST-compliant PBKDF2 with HMAC-SHA512 and salted hashes via `Microsoft.AspNetCore.Identity.PasswordHasher<User>`. Plaintext passwords are never stored.
3. **Anti-Forgery Protection:**
   * Every POST form validates `[ValidateAntiForgeryToken]`.
4. **Secure File Uploads:**
   * Strict whitelist of file extensions: `.pdf`, `.jpg`, `.jpeg`, `.png`, `.webp`.
   * Executables (`.exe`, `.bat`, `.cmd`, `.dll`, `.ps1`) are explicitly blocked.
   * MIME content-type validation and 10MB file size limit.
   * Randomized server filenames (`Guid.NewGuid():N`) and canonical path checks prevent directory traversal attacks.
5. **Safe QR Codes:**
   * QR codes encode only the safe appliance profile URL (`/Appliance/Details/{id}`).
   * Never contains passwords, credentials, sensitive documents, or database connection strings.

---

## Troubleshooting & FAQs

#### Q: How do I open and run the project without command lines?
**A:** Simply open `HomeCare.slnx` in Visual Studio and press **F5**. Visual Studio will build the solution, verify SQL Server LocalDB, apply migrations, seed categories, and launch your default browser automatically.

#### Q: I get a SQL Server connection error on first run.
**A:** Ensure SQL Server LocalDB or SQL Server Express is installed. Open PowerShell and run `sqllocaldb start MSSQLLocalDB`. If using full SQL Server or a named instance, verify the connection string in `appsettings.Development.json`.

#### Q: Where are uploaded invoices and appliance photos saved?
**A:** Files are stored securely under `wwwroot/uploads/documents` and `wwwroot/uploads/appliances` using randomized GUID names.

#### Q: How is the Appliance Health Score calculated?
**A:** The score starts at 100 and applies deterministic adjustments: age bracket deductions, active warranty bonuses, recent service bonuses, deductions for overdue tasks, and deductions for multiple recent breakdown repairs.

---

*HomeCare — Built with ASP.NET Core MVC, C# .NET 10, EF Core, SQL Server, and Microsoft Visual Studio.*
