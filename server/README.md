# Chinese Auction API Server

.NET 9 Web API backend for the Chinese Auction Management System.

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB/Express/Full)

### Setup
1. **Configure Database:**
   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ChineseAuctionDB;Trusted_Connection=true;"
     }
   }
   ```

2. **Run Migrations:**
   ```bash
   dotnet ef database update
   ```

3. **Start Server:**
   ```bash
   dotnet run
   ```

## 📡 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### Gifts Management
- `GET /api/gift` - List all gifts
- `GET /api/gift/available` - Available gifts for purchase
- `POST /api/gift` - Create new gift (Admin)
- `PUT /api/gift/{id}` - Update gift (Admin)
- `DELETE /api/gift/{id}` - Delete gift (Admin)

### Purchase System
- `GET /api/purchase/cart/{userId}` - User's cart items
- `POST /api/purchase/add-to-cart` - Add gift to cart
- `POST /api/purchase/checkout` - Complete purchase
- `DELETE /api/purchase/cart/{id}` - Remove from cart

### Lottery System
- `POST /api/lottery/draw/{giftId}` - Draw lottery (Admin)
- `GET /api/lottery/results` - Public lottery results
- `GET /api/lottery/results/{giftId}` - Specific gift result

### Reporting
- `GET /api/purchase/summary` - Purchase analytics (Admin)
- `GET /api/purchase/export` - Excel export (Admin)

## 🔐 Security Features

- **JWT Authentication** with role-based authorization
- **Cryptographically secure** lottery system using `RandomNumberGenerator`
- **Input validation** with data annotations
- **SQL injection protection** via Entity Framework

## ⚡ Performance Features

- **Optimized queries** with aggregate functions
- **Database indexes** for frequently accessed data
- **Efficient Entity Framework** includes and projections
- **Structured logging** with Serilog

## 🛠️ Configuration

### Email Settings
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Chinese Auction"
  }
}
```

### JWT Configuration
```json
{
  "JwtConfig": {
    "Secret": "your-super-secret-key-here",
    "ExpirationInMinutes": 60
  }
}
```

## 📁 Project Structure

```
server/
├── Controllers/          # API endpoints
├── Services/            # Business logic layer
├── Repositories/        # Data access layer
├── Models/             # Entity models
├── DTOs/               # Data transfer objects
├── Helpers/            # Utility classes
├── Middleware/         # Custom middleware
├── Migrations/         # EF Core migrations
└── Configurations/     # Configuration classes
```

## 🔧 Database Setup

### Apply Performance Indexes
```bash
# Run the performance optimization script
sqlcmd -S .\SQLEXPRESS -d ChineseAuctionDB -i performance_indexes.sql
```

### Manual Migration
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

## 🧪 Development

### Run in Development Mode
```bash
dotnet run --environment Development
```

### Watch Mode (Auto-restart)
```bash
dotnet watch run
```

### View Logs
Logs are stored in `/logs` directory with daily rotation.

## 📊 Monitoring

- **Application logs:** `/logs/app-{date}.log`
- **Health check:** `GET /health`
- **Metrics:** Available via structured logging

Built with ASP.NET Core 9.0 🚀
