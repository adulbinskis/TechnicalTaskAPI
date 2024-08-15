# Download git

https://git-scm.com/downloads

# Clone repo

git clone https://github.com/adulbinskis/TechnicalTaskAPI.git

# Download Visual Studio

https://visualstudio.microsoft.com/downloads/

# Install Visual Studio worklands

.NET 8
.NET desktop development
ASP.NET and web development

# Install SQL server and SSMS

SQL server:
https://www.microsoft.com/lv-lv/sql-server/sql-server-downloads

SSMS:
https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16

# Create empty database

Database name:
TehnicalTaskDb
Server name:
localhost
Authentication:
Windows Authentication

# Modefie connection string if needed

By deafult:
appsettings.json

"ConnectionStrings": {
  "DefaultConnection": "Data Source=localhost;Initial Catalog=TehnicalTaskDb;Integrated Security=True;Trust Server Certificate=True"
},

And for tests:
DatabaseFixture.cs

private readonly string _connectionString = "Data Source=localhost;Initial Catalog=TehnicalTaskDbTests;Integrated Security=True;Trust Server Certificate=True";

# Install dotnet ef global:

dotnet tool install --global dotnet-ef

# Update Database

1. Open terminal in solution
2. Type command: cd TechnicalTaskAPI
3. Type command: dotnet ef database update -c ApplicationDbContext

# Setup complete

# Optional

Migrations:

cd TechnicalTaskAPI

dotnet ef migrations add AddedQandATables -c ApplicationDbContext -o ORM\Migrations

Tests:

Open solution dir

dotnet test
