# DispatchManager Backend API

**Sistema de gestión de órdenes de despacho con cálculo automático de distancias y costos**

## 🚀 Stack Técnico

- **.NET 9.0** - Framework principal
- **C# 13** - Lenguaje de programación
- **SQL Server** - Base de datos relacional
- **Entity Framework Core** - ORM
- **Clean Architecture** - Patrón arquitectónico
- **DDD** - Domain Driven Design
- **MediatR** - Patrón CQRS y Mediator
- **AutoMapper** - Mapeo de objetos
- **NUnit** - Testing framework

## 📁 Estructura del Proyecto

```
DispatchManager/
├── DispatchManager.API/          # Capa de presentación (Controllers, Middleware)
├── DispatchManager.Application/  # Lógica de aplicación (Commands, Queries, DTOs)
├── DispatchManager.Domain/       # Dominio (Entities, Value Objects, Services)
├── DispatchManager.Infrastructure/ # Persistencia y servicios externos
├── DispatchManager.Identity/     # Autenticación y autorización
└── DispatchManager.Application.UnitTests/ # Pruebas unitarias
```

## ⚙️ Configuración Inicial

### 1. Prerrequisitos
```bash
# .NET 9.0 SDK
dotnet --version  # Debe mostrar 9.x.x

# SQL Server LocalDB o instancia completa
# Visual Studio 2022 o VS Code con extensión C#
```

### 2. Clonar y configurar
```bash
git clone https://github.com/alonsodev/dispatch-manager-backend
cd dispatch-manager-backend
dotnet restore
```

### 3. Base de datos
```bash
# Configurar connection string en appsettings.json
# Ejecutar migraciones
cd DispatchManager.API
dotnet ef database update
```

### 4. Ejecutar API
```bash
cd DispatchManager.API
dotnet run
# API disponible en: https://localhost:7196/
# Swagger UI: https://localhost:7196/index.html
```

## 🔧 Configuración de Base de Datos

### Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=dbpruebatecnica.database.windows.net;Initial Catalog=Alonso_DispatchManager;user id=usuarioprueba;password=Usu4ri0Pru3ba12;Encrypt=False"
  }
}
```

### Comandos Entity Framework
```bash
# Crear migración
dotnet ef migrations add <MigrationName> --project DispatchManager.Infrastructure --startup-project DispatchManager.API

# Aplicar migración
dotnet ef database update --project DispatchManager.Infrastructure --startup-project DispatchManager.API

# Eliminar última migración
dotnet ef migrations remove --project DispatchManager.Infrastructure --startup-project DispatchManager.API
```

## 📊 Endpoints Principales

### Órdenes
```http
POST   /api/orders              # Crear orden
GET    /api/orders              # Listar órdenes
GET    /api/orders/{id}         # Obtener orden por ID
GET    /api/orders/customer/{customerId} # Órdenes por cliente
```

### Clientes y Productos
```http
GET    /api/customers           # Listar clientes
GET    /api/products            # Listar productos
```

### Reportes
```http
GET    /api/reports/distance-intervals # Reporte por intervalos
GET    /api/reports/excel/{customerId}  # Descargar Excel
```

### Cache
```http
DELETE /api/cache               # Invalidar caché
```

## 🧮 Lógica de Negocio

### Cálculo de Distancia
- **Fórmula de Haversine** para coordenadas geográficas
- Precisión: hasta 6 decimales
- Resultado en kilómetros

### Cálculo de Costo
```
1-50 km     → $100 USD
51-200 km   → $300 USD  
201-500 km  → $1000 USD
501-1000 km → $1500 USD
```

### Validaciones
- Distancia mínima: **1 km**
- Distancia máxima: **1000 km**
- Cantidad de producto: > 0
- Cliente y producto deben existir

## 🧪 Testing

### Ejecutar pruebas unitarias
```bash
dotnet test
```

### Ejecutar con cobertura
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Estructura de tests
```
DispatchManager.Application.UnitTests/
├── Features/
│   ├── Orders/
│   │   ├── Commands/
│   │   └── Queries/
├── Services/
└── Common/
```

## 🔒 Seguridad

- **API Keys** para autenticación
- **CORS** configurado para frontend
- **HTTPS** obligatorio en producción
- **Input validation** en todos los endpoints
- **Rate limiting** implementado

## 📝 Patrones Implementados

### Domain Driven Design
- **Entities**: Order, Customer, Product
- **Value Objects**: Coordinate, Quantity, Money
- **Domain Services**: DistanceCalculation, CostCalculation
- **Aggregates**: Order como aggregate root

### CQRS con MediatR
- **Commands**: CreateOrder, UpdateOrder
- **Queries**: GetOrders, GetOrdersByCustomer
- **Handlers**: Separación clara de responsabilidades

### Repository Pattern
- **IUnitOfWork**: Manejo de transacciones
- **Generic Repository**: CRUD operations
- **Specification Pattern**: Consultas complejas

## 🚀 Deploy

### Azure
```bash
# Configurar Azure CLI y deploy
az webapp deploy --name <app-name> --resource-group <rg-name>
```

## 🐛 Logging

- **Serilog** configurado
- Logs estructurados en JSON
- Niveles: Debug, Info, Warning, Error
- Archivo de logs: `logs/log-.txt`

## 📈 Performance

- **Redis Cache** para consultas frecuentes
- **Lazy Loading** deshabilitado
- **Async/Await** en toda la aplicación
- **Connection pooling** optimizado

---

**Desarrollado con Clean Architecture, SOLID principles y Domain Driven Design**