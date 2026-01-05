# Deployment Guide (DigitalOcean App Platform)

This project has been configured for deployment on DigitalOcean App Platform using Docker and SQLite.

## Prerequisites
- A DigitalOcean account.
- This GitHub repository linked to your DigitalOcean account.

## Steps to Deploy

1. **Log in to DigitalOcean** and go to **Apps** (App Platform).
2. Click **Create App**.
3. Select **GitHub** as the source.
4. Select the repository: `zainrafique38dev/EAD-AMS`.
5. Select the `main` branch and click **Next**.
6. **Resources**:
   - DigitalOcean should automatically detect the `Dockerfile`.
   - Ensure the resource type is **Web Service**.
   - **HTTP Port**: `8080` (The Dockerfile exposes 8080).
   - Click **Next**.

7. **Environment Variables**:
   You **MUST** set the following environment variables in the "Environment" step (click "Edit" next to Environment Variables):
   
   | Key | Value | Description |
   |-----|-------|-------------|
   | `ConnectionStrings__DefaultConnection` | `Data Source=app.db` | Uses SQLite for a self-contained database. |
   | `Jwt__Key` | `YourSecretKeyMustBe32CharsLong1234!` | Secure key for logins. |
   | `Jwt__Issuer` | `MessManagementSystem` | JWT Issuer setting. |
   | `Jwt__Audience` | `MessManagementSystemUsers` | JWT Audience setting. |

8. **Review and Deploy**:
   - Select your plan (Basic is fine for testing).
   - Click **Create Resources**.

## Important Notes

### Database Persistence
This deployment uses **SQLite** (`app.db`) stored inside the container for simplicity. 
- **WARNING**: If the app is redeployed (new code pushed) or the container restarts, the database file **may be reset** and data lost unless you use App Platform's persistent storage (if available) or switch to a Managed Database.
- **Better for Production**: Create a Managed PostgreSQL database on DigitalOcean, and set `ConnectionStrings__DefaultConnection` to the Postgres connection string. The app is already configured to switch to SQL Server/Postgres if the connection string format matches, but currently defaults to SQLite if not "Server=...".

### Troubleshooting
- If the build fails, check the "Build Logs" in DigitalOcean.
- If the app starts but errors occur, check "Runtime Logs".
