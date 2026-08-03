# 🛠️ Komut Referansı ve Hızlı Başlangıç

> Projeyi kurmak, çalıştırmak ve geliştirmek için gereken tüm komutlar.

---

## 🐳 Docker — Altyapı Servisleri

### docker-compose.yml (başlangıç şablonu)
```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    container_name: watchlist-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: watchlist_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    container_name: watchlist-redis
    ports:
      - "6379:6379"

  rabbitmq:
    image: rabbitmq:3-management
    container_name: watchlist-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    ports:
      - "5672:5672"
      - "15672:15672"

volumes:
  postgres_data:
```

### Başlat / Durdur
```bash
# Başlat (arka planda)
docker-compose up -d

# Durdur
docker-compose down

# Durdur + volume sil (temiz başlangıç)
docker-compose down -v

# Loglar
docker-compose logs -f
docker-compose logs -f postgres
```

### Bağlantı Test Komutları
```bash
# PostgreSQL bağlantı testi
docker exec -it watchlist-postgres psql -U postgres -d watchlist_db -c "SELECT 1;"

# Redis bağlantı testi
docker exec -it watchlist-redis redis-cli ping
# Beklenen cevap: PONG

# RabbitMQ Management UI
# Tarayıcıda: http://localhost:15672
# Kullanıcı: guest / Şifre: guest
```

---

## 🔨 .NET CLI — Proje Oluşturma

### Solution ve Proje Kurulumu
```bash
# Proje klasörüne git
cd c:\Users\Yasin\Desktop\proje

# Solution oluştur
dotnet new sln -n WatchListScreening

# Projeleri oluştur
dotnet new classlib -n WatchListScreening.Domain -o src/WatchListScreening.Domain
dotnet new classlib -n WatchListScreening.Application -o src/WatchListScreening.Application
dotnet new classlib -n WatchListScreening.Infrastructure -o src/WatchListScreening.Infrastructure
dotnet new webapi -n WatchListScreening.API -o src/WatchListScreening.API
dotnet new mvc -n WatchListScreening.Web -o src/WatchListScreening.Web

# Solution'a ekle
dotnet sln WatchListScreening.sln add src/WatchListScreening.Domain
dotnet sln WatchListScreening.sln add src/WatchListScreening.Application
dotnet sln WatchListScreening.sln add src/WatchListScreening.Infrastructure
dotnet sln WatchListScreening.sln add src/WatchListScreening.API
dotnet sln WatchListScreening.sln add src/WatchListScreening.Web
```

### Proje Referansları (Bağımlılık Yönü)
```bash
# Application → Domain
dotnet add src/WatchListScreening.Application reference src/WatchListScreening.Domain

# Infrastructure → Domain + Application
dotnet add src/WatchListScreening.Infrastructure reference src/WatchListScreening.Domain
dotnet add src/WatchListScreening.Infrastructure reference src/WatchListScreening.Application

# API → Application + Infrastructure
dotnet add src/WatchListScreening.API reference src/WatchListScreening.Application
dotnet add src/WatchListScreening.API reference src/WatchListScreening.Infrastructure

# Web → Application + Infrastructure
dotnet add src/WatchListScreening.Web reference src/WatchListScreening.Application
dotnet add src/WatchListScreening.Web reference src/WatchListScreening.Infrastructure
```

### NuGet Paketleri
```bash
# ---- Infrastructure ----
# EF Core + PostgreSQL
dotnet add src/WatchListScreening.Infrastructure package Microsoft.EntityFrameworkCore --version 8.0.*
dotnet add src/WatchListScreening.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.*
dotnet add src/WatchListScreening.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.0.*

# Redis
dotnet add src/WatchListScreening.Infrastructure package StackExchange.Redis

# RabbitMQ
dotnet add src/WatchListScreening.Infrastructure package RabbitMQ.Client

# Quartz.NET (background jobs)
dotnet add src/WatchListScreening.Infrastructure package Quartz
dotnet add src/WatchListScreening.Infrastructure package Quartz.Extensions.Hosting

# ---- API ----
# Swagger
dotnet add src/WatchListScreening.API package Swashbuckle.AspNetCore

# Serilog
dotnet add src/WatchListScreening.API package Serilog.AspNetCore
dotnet add src/WatchListScreening.API package Serilog.Sinks.Console

# EF Core Tools (migration)
dotnet tool install --global dotnet-ef

# ---- Web ----
# Serilog
dotnet add src/WatchListScreening.Web package Serilog.AspNetCore
dotnet add src/WatchListScreening.Web package Serilog.Sinks.Console
```

---

## 🗄️ EF Core Migration Komutları

```bash
# Migration oluştur (Infrastructure projesinde, API projesi startup olarak)
dotnet ef migrations add InitialCreate -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API

# Migration uygula
dotnet ef database update -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API

# Son migration'ı geri al
dotnet ef migrations remove -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API

# Veritabanını sil (temiz başlangıç)
dotnet ef database drop -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API

# Migration listesi
dotnet ef migrations list -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API
```

---

## 🏃 Build & Run

```bash
# Tüm solution'ı build et
dotnet build WatchListScreening.sln

# API'yi çalıştır (default: https://localhost:5001, http://localhost:5000)
dotnet run --project src/WatchListScreening.API

# Web panel'i çalıştır (farklı port!)
dotnet run --project src/WatchListScreening.Web --urls "http://localhost:5010"

# veya aynı anda ikisini çalıştırmak için iki terminal aç
```

---

## 📡 Connection Strings (appsettings.json)

### API ve Web için ortak ayarlar:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=watchlist_db;Username=postgres;Password=postgres123"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "screening-requests"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

---

## 🔍 Faydalı Kontrol Komutları

```bash
# PostgreSQL'de tabloları listele
docker exec -it watchlist-postgres psql -U postgres -d watchlist_db -c "\dt"

# PostgreSQL'de veri kontrol
docker exec -it watchlist-postgres psql -U postgres -d watchlist_db -c "SELECT * FROM \"SanctionEntries\" LIMIT 5;"

# Redis'te key'leri listele
docker exec -it watchlist-redis redis-cli KEYS "*"

# Redis'te belirli key'i oku
docker exec -it watchlist-redis redis-cli GET "sanctions:all"

# RabbitMQ kuyruk durumu
# http://localhost:15672 → Queues sekmesi
```

---

## 🌐 Erişim Adresleri

| Servis | URL |
|---|---|
| API (Swagger) | http://localhost:5000/swagger |
| Web Panel (MVC) | http://localhost:5010 |
| RabbitMQ Management | http://localhost:15672 |
| PostgreSQL | localhost:5432 |
| Redis | localhost:6379 |

---

## 📂 Git Komutları

```bash
# Git başlat
git init
git add .
git commit -m "initial commit: project structure"

# .gitignore oluştur (dotnet)
dotnet new gitignore
```
