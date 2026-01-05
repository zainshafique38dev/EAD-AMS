# JWT Authentication API Documentation

## Overview
This document describes the JWT (JSON Web Token) authentication implementation in the Mess Management System. The system now supports both Cookie-based authentication (for web views) and JWT-based authentication (for API endpoints).

## Configuration

### JWT Settings (appsettings.json)
```json
"Jwt": {
  "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm",
  "Issuer": "MessManagementSystem",
  "Audience": "MessManagementSystemUsers"
}
```

**Important**: Change the `Key` value to a secure, randomly generated string in production.

## Authentication Endpoints

### 1. Login (Get JWT Token)
**Endpoint**: `POST /api/AuthApi/login`

**Request Body**:
```json
{
  "username": "admin",
  "password": "admin123",
  "role": "Admin"
}
```

**Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "username": "admin",
  "role": "Admin",
  "mustChangePassword": false
}
```

**Error Responses**:
- 400 Bad Request: Missing username or password
- 401 Unauthorized: Invalid credentials

### 2. Change Password
**Endpoint**: `POST /api/AuthApi/change-password`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Request Body**:
```json
{
  "oldPassword": "current_password",
  "newPassword": "new_password"
}
```

**Response** (200 OK):
```json
{
  "message": "Password changed successfully"
}
```

### 3. Validate Token
**Endpoint**: `GET /api/AuthApi/validate`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Response** (200 OK):
```json
{
  "userId": "1",
  "username": "admin",
  "role": "Admin",
  "isValid": true
}
```

## API Endpoints

### Teachers API

#### Get All Teachers (Admin only)
**Endpoint**: `GET /api/TeachersApi`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Response** (200 OK):
```json
[
  {
    "teacherId": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "03001234567",
    "department": "Computer Science",
    "dateJoined": "2025-01-01T00:00:00",
    "username": "john.doe"
  }
]
```

#### Get Teacher by ID
**Endpoint**: `GET /api/TeachersApi/{id}`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

#### Get My Profile (Teacher only)
**Endpoint**: `GET /api/TeachersApi/my-profile`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

#### Create Teacher (Admin only)
**Endpoint**: `POST /api/TeachersApi`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Request Body**:
```json
{
  "fullName": "Jane Smith",
  "email": "jane@example.com",
  "phoneNumber": "03001234567",
  "department": "Mathematics",
  "username": "jane.smith",
  "password": "initialPassword123"
}
```

### Attendance API

#### Get Attendance Records (Admin only)
**Endpoint**: `GET /api/AttendanceApi?date=2025-01-04&teacherId=1`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Query Parameters**:
- `date` (optional): Filter by specific date (YYYY-MM-DD)
- `teacherId` (optional): Filter by teacher ID

#### Get My Attendance (Teacher only)
**Endpoint**: `GET /api/AttendanceApi/my-attendance?startDate=2025-01-01&endDate=2025-01-31`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Query Parameters**:
- `startDate` (optional): Start date (YYYY-MM-DD)
- `endDate` (optional): End date (YYYY-MM-DD)

#### Mark Attendance (Admin only)
**Endpoint**: `POST /api/AttendanceApi`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Request Body**:
```json
{
  "teacherId": 1,
  "date": "2025-01-04",
  "breakfastTaken": true,
  "lunchTaken": true,
  "dinnerTaken": false,
  "remarks": "Off-site meeting during dinner"
}
```

#### Update Attendance (Admin only)
**Endpoint**: `PUT /api/AttendanceApi/{id}`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Request Body**:
```json
{
  "breakfastTaken": true,
  "lunchTaken": false,
  "dinnerTaken": true,
  "remarks": "Updated remarks"
}
```

### Menu API

#### Get Menu Items
**Endpoint**: `GET /api/MenuApi?dayOfWeek=Monday&mealType=Breakfast`

**Query Parameters**:
- `dayOfWeek` (optional): Filter by day (Monday, Tuesday, etc.)
- `mealType` (optional): Filter by meal type (Breakfast, Lunch, Dinner)

**Note**: This endpoint allows anonymous access.

#### Get Today's Menu
**Endpoint**: `GET /api/MenuApi/today`

**Response** (200 OK):
```json
{
  "day": "Saturday",
  "menu": [
    {
      "menuItemId": 1,
      "itemName": "Nihari",
      "description": "Spicy slow-cooked beef with naan",
      "mealType": "Breakfast",
      "ratePerServing": 120
    }
  ]
}
```

#### Get Menu Item by ID
**Endpoint**: `GET /api/MenuApi/{id}`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

#### Create Menu Item (Admin only)
**Endpoint**: `POST /api/MenuApi`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

**Request Body**:
```json
{
  "itemName": "Aloo Paratha",
  "description": "Potato-stuffed flatbread",
  "mealType": "Breakfast",
  "dayOfWeek": "Monday",
  "ratePerServing": 75
}
```

#### Update Menu Item (Admin only)
**Endpoint**: `PUT /api/MenuApi/{id}`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

#### Delete Menu Item (Admin only)
**Endpoint**: `DELETE /api/MenuApi/{id}`

**Headers**:
```
Authorization: Bearer <your-jwt-token>
```

## Using JWT Tokens

### Example with cURL

```bash
# 1. Login and get token
curl -X POST http://localhost:5000/api/AuthApi/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123","role":"Admin"}'

# 2. Use token in subsequent requests
curl -X GET http://localhost:5000/api/TeachersApi \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### Example with JavaScript/Fetch

```javascript
// Login
const loginResponse = await fetch('http://localhost:5000/api/AuthApi/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'admin',
    password: 'admin123',
    role: 'Admin'
  })
});

const { token } = await loginResponse.json();

// Use token for authenticated requests
const teachersResponse = await fetch('http://localhost:5000/api/TeachersApi', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const teachers = await teachersResponse.json();
```

### Example with Postman

1. **Login to get token**:
   - Method: POST
   - URL: `http://localhost:5000/api/AuthApi/login`
   - Body (JSON):
     ```json
     {
       "username": "admin",
       "password": "admin123"
     }
     ```
   - Copy the `token` from the response

2. **Use token for authenticated requests**:
   - Method: GET (or any other)
   - URL: `http://localhost:5000/api/TeachersApi`
   - Headers:
     - Key: `Authorization`
     - Value: `Bearer YOUR_TOKEN_HERE`

## Security Best Practices

1. **Secure the JWT Key**: 
   - Use a strong, randomly generated key (at least 32 characters)
   - Store it securely (environment variables, Azure Key Vault, etc.)
   - Never commit it to version control

2. **Use HTTPS**: 
   - Always use HTTPS in production to prevent token interception

3. **Token Expiration**: 
   - Tokens expire after 8 hours
   - Implement token refresh mechanism if needed

4. **Token Storage**:
   - Store tokens securely on the client side
   - Use HttpOnly cookies or secure storage mechanisms
   - Never store in localStorage if XSS is a concern

5. **Role-Based Access Control**:
   - API endpoints are protected with role-based authorization
   - Admin endpoints: Only accessible by Admin role
   - Teacher endpoints: Accessible by both Admin and Teacher roles

## Testing

Default credentials for testing:
- **Admin**:
  - Username: `admin`
  - Password: `admin123`

Create additional users through the web interface or API endpoints.

## Troubleshooting

### Common Issues

1. **401 Unauthorized**:
   - Check if token is included in the Authorization header
   - Verify token format: `Bearer <token>`
   - Ensure token hasn't expired

2. **403 Forbidden**:
   - User doesn't have required role for the endpoint
   - Check role claims in the token

3. **500 Internal Server Error**:
   - Check JWT configuration in appsettings.json
   - Verify database connection
   - Check application logs

## Architecture

The JWT implementation includes:

1. **JwtService**: Generates and validates JWT tokens
2. **API Controllers**: RESTful endpoints with JWT authentication
3. **Dual Authentication**: Supports both Cookie (web) and JWT (API) authentication
4. **Role-Based Authorization**: Fine-grained access control

## Future Enhancements

Consider implementing:
- Token refresh mechanism
- Token blacklisting for logout
- Rate limiting for API endpoints
- API versioning
- Swagger/OpenAPI documentation
