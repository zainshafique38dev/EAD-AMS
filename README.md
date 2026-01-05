# Mess Attendance & Billing Management System

A comprehensive web-based system for managing mess attendance and billing for educational institutions.

## Features

### Admin Panel
- **Dashboard**: Overview of system statistics and recent activities
- **Teacher Management**: Add, edit, and manage teacher records
- **User Account Management**: Create user accounts for teachers with forced password change on first login
- **Attendance Tracking**: Mark daily attendance for breakfast, lunch, and dinner
- **Menu Management**: Create and manage weekly menu plans with rates
- **Billing System**: 
  - Automatic bill generation based on attendance
  - Water bill shared equally among all teachers
  - Food bill calculated based on meals consumed
  - Configurable meal rates
- **Reports**: Monthly attendance and billing reports

## Admin Credentials

- **Username**: admin
- **Password**: admin123

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server (LocalDB)
- **Authentication**: Cookie-based authentication with BCrypt password hashing
- **UI**: Bootstrap 5 with Font Awesome icons
- **ORM**: Entity Framework Core 8.0

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB) or SQL Server
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Restore NuGet Packages**
   ```powershell
   dotnet restore
   ```

2. **Build the Project**
   ```powershell
   dotnet build
   ```

3. **Run the Application**
   ```powershell
   dotnet run
   ```

4. **Access the Application**
   - Open browser and navigate to: `https://localhost:5001` or `http://localhost:5000`
   - Login with admin credentials

### Database

The database will be automatically created on first run with:
- Default admin user (admin/admin123)
- Default billing configuration

Connection string is configured in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MessManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

## Project Structure

```
MessManagementSystem/
├── Controllers/
│   ├── AccountController.cs      # Login, logout, password change
│   ├── AdminController.cs        # Dashboard
│   ├── TeachersController.cs     # Teacher CRUD operations
│   ├── AttendanceController.cs   # Attendance management
│   ├── MenuController.cs         # Menu management
│   └── BillingController.cs      # Billing and configuration
├── Data/
│   └── ApplicationDbContext.cs   # EF Core DbContext
├── Models/
│   ├── User.cs                   # User accounts
│   ├── Teacher.cs                # Teacher information
│   ├── Attendance.cs             # Daily attendance records
│   ├── MenuItem.cs               # Menu items with rates
│   ├── Bill.cs                   # Monthly bills
│   └── BillingConfiguration.cs   # Billing settings
├── Views/
│   ├── Account/                  # Login and password views
│   ├── Admin/                    # Dashboard views
│   ├── Teachers/                 # Teacher management views
│   ├── Attendance/               # Attendance views
│   ├── Menu/                     # Menu management views
│   ├── Billing/                  # Billing views
│   └── Shared/
│       ├── _Layout.cshtml        # Main layout with sidebar
│       └── Error.cshtml
├── Program.cs                    # Application entry point
├── appsettings.json             # Configuration
└── MessManagementSystem.csproj  # Project file
```

## Key Features Explained

### 1. Water Bill Sharing
- Total monthly water bill is configured in Billing Configuration
- Automatically divided equally among all active teachers
- Each teacher pays their share regardless of attendance

### 2. Food Bill Calculation
- Only teachers who consume meals are charged
- Calculated based on:
  - Number of breakfasts × Breakfast rate
  - Number of lunches × Lunch rate
  - Number of dinners × Dinner rate

### 3. Attendance Tracking
- Daily attendance marking for each meal type
- Historical attendance reports
- Monthly summaries with meal counts

### 4. Security
- Passwords are hashed using BCrypt
- Cookie-based authentication
- Role-based authorization (Admin role)
- New users must change password on first login
- Password input fields are type="password" (not visible)

### 5. User Management
- Admin can create teacher accounts
- Teachers can be assigned user accounts for system access
- Users can be deactivated instead of deleted (preserves history)

## Usage Workflow

1. **Initial Setup**
   - Login as admin
   - Configure billing rates (Billing → Configuration)
   - Add menu items for the week (Menu Management)

2. **Teacher Management**
   - Add teachers with their details
   - Optionally create user accounts for teachers
   - Teachers marked as inactive won't be included in bill calculations

3. **Daily Operations**
   - Mark attendance daily (Attendance)
   - Select date and mark meals for each teacher

4. **Monthly Billing**
   - Generate bills at month end (Billing → Generate Bills)
   - Review generated bills
   - Mark bills as paid when payment received

5. **Reports**
   - View attendance reports (Attendance → Report)
   - View billing history (Billing)

## Additional Notes

- All monetary values are in Indian Rupees (₹)
- Dates follow the format: Month DD, YYYY
- The system uses LocalDB by default but can be configured for SQL Server
- Bills can be regenerated if needed (will update existing bills)
- Historical data is preserved even when teachers are deactivated

## Support

For issues or questions, please refer to the documentation or contact the system administrator.

---

© 2025 Mess Management System
