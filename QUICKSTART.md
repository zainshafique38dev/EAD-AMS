# Quick Start Guide

## Running the Application

1. **Open Terminal/PowerShell** in the project directory:
   ```
   E:\.net code\Mess Management System
   ```

2. **Restore packages** (first time only):
   ```powershell
   dotnet restore
   ```

3. **Run the application**:
   ```powershell
   dotnet run
   ```

4. **Open your browser** and navigate to:
   ```
   https://localhost:5001
   ```

5. **Login** with:
   - Username: `admin`
   - Password: `admin123`

## First Time Setup

After logging in for the first time:

1. **Configure Billing** (Important!)
   - Click "Configuration" in the sidebar
   - Set monthly water bill total (e.g., 5000)
   - Set meal rates:
     - Breakfast: 30
     - Lunch: 60
     - Dinner: 50
   - Click "Save Configuration"

2. **Add Teachers**
   - Click "Teachers" in the sidebar
   - Click "Add New Teacher"
   - Fill in teacher details
   - Optionally create a user account for the teacher
   - Click "Save Teacher"

3. **Add Menu Items** (Optional)
   - Click "Menu Management" in the sidebar
   - Click "Add New Menu Item"
   - Add items for each day of the week
   - Set meal type and rate
   - Click "Save Menu Item"

## Daily Operations

### Mark Attendance
1. Click "Attendance" in the sidebar
2. Select date (defaults to today)
3. Click "Mark" button for each teacher
4. Check the meals consumed (Breakfast/Lunch/Dinner)
5. Click "Save Attendance"

### Generate Monthly Bills
1. Click "Billing" in the sidebar
2. Click "Generate Bills"
3. Select month and year
4. Click "Generate Bills"
5. Review the generated bills
6. Mark bills as paid when payment is received

## Common Tasks

### Add a New Teacher
`Teachers → Add New Teacher → Fill form → Save`

### View Attendance Report
`Attendance → Report → Select Month/Year → View`

### Change Your Password
`Click your username (top-right) → Change Password`

### View Dashboard Statistics
`Dashboard (click logo or Dashboard link)`

## Troubleshooting

### Database Issues
If you encounter database errors, delete the database and restart:
1. Close the application
2. Delete: `C:\Users\[YourUsername]\MessManagementDB.mdf`
3. Run the application again (database will be recreated)

### Cannot Login
- Make sure you're using correct credentials: admin/admin123
- Password field should not show characters (hidden for security)

### Port Already in Use
If ports 5000/5001 are in use, the application will automatically use different ports. Check the console output for the correct URL.

## Project Structure

```
Key Files:
├── Program.cs                    # Main entry point & configuration
├── appsettings.json             # Database connection & settings
├── Controllers/                  # Business logic
├── Models/                      # Data models
├── Views/                       # UI pages
└── Data/ApplicationDbContext.cs # Database context
```

## Tips

- **Water bills** are shared equally by ALL active teachers
- **Food bills** are only for teachers who eat meals
- Teachers can be **deactivated** instead of deleted to preserve history
- Bills can be **regenerated** if needed (will update existing data)
- All prices are in **Indian Rupees (₹)**

## Need Help?

Check the README.md file for detailed documentation.

---
Happy Managing! 🍽️
