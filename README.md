# BudgetTracker

Aplicación web para registrar y consultar ingresos y gastos personales, con calendario mensual, categorías, autenticación y tema claro/oscuro.

---

# BudgetTracker (EN)

## Description

BudgetTracker is an ASP.NET Core MVC web app for tracking personal income and expenses. Users can create an account, confirm email, manage profile data, switch light/dark theme, and manage transactions from a monthly calendar and detail views.

## Technologies

| Layer | Stack |
| --- | --- |
| Runtime | .NET 8 |
| Backend / UI | ASP.NET Core MVC, Razor Views |
| Auth | ASP.NET Core Identity (email confirmation, lockout, password reset) |
| Database | PostgreSQL + Entity Framework Core (Npgsql) |
| Background jobs | Hangfire + Hangfire.PostgreSql |
| Email | Brevo (SMTP / API) |
| Frontend | Bootstrap 5, Bootstrap Icons, jQuery, SweetAlert2, Chart.js, Tom Select |
| Deploy | Docker (`Dockrfile`), prepared for hosting such as Render.com |

## Features

1. **Users**
   - Account creation with email confirmation
   - Login / logout
   - Profile edit (username, email, password)
   - Forgot password / reset password
   - Light / dark theme (persisted per user; test users cannot change theme permanently)
   - Demo account support (`Test` role) for quick trials without full permissions

2. **Income and expenses**
   - Create, edit, and delete income and bills
   - Amount, category (searchable), optional description, and date
   - Categories grouped for easier selection

3. **History and visualization**
   - Monthly calendar with day totals
   - Day detail lists for income / expenses
   - Full-month detail with totals, category breakdown, and doughnut charts
   - Add a transaction from a calendar day

4. **UI**
   - Responsive layout for desktop and mobile viewports

5. **Database health check (keep-alive)**
   - Anonymous endpoint `GET /check` runs `SELECT 1` against PostgreSQL
   - Returns `OK` (200) when the DB is reachable, or `server wasn't reached` (500) on failure
   - Used to keep the database warm and ready for queries (important on free hosting that sleeps idle services)

## Jobs and cron-job.org

There are two complementary keep-alive mechanisms:

### Internal job (Hangfire)

- Recurring job id: `db-check`
- Schedule: every 10 minutes (`*/10 * * * *`)
- Action: HTTP GET to `/check` (uses `App:BaseUrl` when configured, otherwise the app listen address)
- Dashboard (local only): `/hangfire`

### External cron (cron-job.org)

In production, an external cron on [cron-job.org](https://cron-job.org) is configured to call the public check endpoint every **10 minutes**:

1. Create a job in cron-job.org
2. URL: `https://<your-deployed-host>/check`
3. Method: `GET`
4. Schedule: every 10 minutes
5. Purpose: ping the app so PostgreSQL stays up and ready to accept queries, even if the process was idle

This external ping complements Hangfire: cron-job.org can wake the site from outside; Hangfire keeps checking while the app is already running.

Optional config:

```json
"App": {
  "BaseUrl": "https://your-app.onrender.com"
}
```

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL
- (Optional) Brevo account for transactional email

## Installation and configuration

1. **Clone the repository**

```bash
git clone https://github.com/nicolasfernandezriesen/BudgetTracker.git
cd BudgetTracker/BudgetTracker
```

2. **Configure `appsettings.json` / user secrets / environment variables**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=BudgetTrackerDB;Username=...;Password=..."
  },
  "EmailSettings": {
    "SmtpServer": "smtp-relay.brevo.com",
    "Port": 587,
    "SenderName": "Budget Tracker",
    "SenderEmail": "your-sender@example.com",
    "Username": "your-smtp-user",
    "Password": "your-smtp-password",
    "ApiKey": "your-brevo-api-key"
  },
  "App": {
    "BaseUrl": "https://localhost:7170"
  }
}
```

Apply the SQL scripts under `Data/Migraciones/` if the database is empty.

3. **Run**

```bash
dotnet restore
dotnet run
```

Default HTTP profile: `http://localhost:5036`

4. **Docker (optional)**

```bash
docker build -f Dockrfile -t budgettracker .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." budgettracker
```

## Project structure (high level)

```
BudgetTracker/
  Controllers/     Home, User, Bill, Income, History, Check
  Jobs/            Hangfire recurring jobs (DbCheckJob)
  Services/        Business logic
  Repositories/    Data access
  Views/           Razor UI
  wwwroot/         CSS / JS / static assets
  Data/            DbContext + SQL migrations
```

## License / notes

Demo credentials may be shown on the guest banner for local testing. Do not commit real production secrets; prefer environment variables or user secrets.

---

# BudgetTracker (ES)

## Descripción

BudgetTracker es una aplicación web en ASP.NET Core MVC para llevar el registro de ingresos y gastos personales. Permite crear cuenta, confirmar email, editar perfil, usar tema claro/oscuro y gestionar movimientos desde un calendario mensual y vistas de detalle.

## Tecnologías

| Capa | Stack |
| --- | --- |
| Runtime | .NET 8 |
| Backend / UI | ASP.NET Core MVC, Razor Views |
| Autenticación | ASP.NET Core Identity (confirmación de email, bloqueo, recuperación de contraseña) |
| Base de datos | PostgreSQL + Entity Framework Core (Npgsql) |
| Jobs en background | Hangfire + Hangfire.PostgreSql |
| Email | Brevo (SMTP / API) |
| Frontend | Bootstrap 5, Bootstrap Icons, jQuery, SweetAlert2, Chart.js, Tom Select |
| Deploy | Docker (`Dockrfile`), preparado para hosting como Render.com |

## Funcionalidades

1. **Usuarios**
   - Alta con confirmación por email
   - Inicio / cierre de sesión
   - Edición de perfil (usuario, email, contraseña)
   - Olvidé mi contraseña / restablecer contraseña
   - Tema claro / oscuro (persistido por usuario; el rol `Test` no persiste el cambio)
   - Cuenta demo con rol `Test` para pruebas rápidas

2. **Ingresos y gastos**
   - Alta, edición y baja
   - Monto, categoría (buscable), descripción opcional y fecha
   - Categorías agrupadas

3. **Historial y visualización**
   - Calendario mensual con totales por día
   - Detalle diario de ingresos / gastos
   - Detalle mensual con totales, desglose por categoría y gráficos
   - Crear movimiento desde un día del calendario

4. **Interfaz**
   - Layout responsive para escritorio y móvil

5. **Check de base de datos (keep-alive)**
   - Endpoint anónimo `GET /check` ejecuta `SELECT 1` en PostgreSQL
   - Responde `OK` (200) si la DB responde, o `server wasn't reached` (500) si falla
   - Sirve para mantener la base levantada y lista para consultas (útil en hosting gratuito que duerme servicios inactivos)

## Jobs y cron-job.org

Hay dos mecanismos complementarios de keep-alive:

### Job interno (Hangfire)

- Id del job recurrente: `db-check`
- Cron: cada 10 minutos (`*/10 * * * *`)
- Acción: HTTP GET a `/check` (usa `App:BaseUrl` si está configurado; si no, la dirección de escucha de la app)
- Dashboard (solo localhost): `/hangfire`

### Cron externo (cron-job.org)

En producción se usa un cron en [cron-job.org](https://cron-job.org) que llama al endpoint público cada **10 minutos**:

1. Crear un job en cron-job.org
2. URL: `https://<tu-host-desplegado>/check`
3. Método: `GET`
4. Frecuencia: cada 10 minutos
5. Objetivo: pegarle a la app para que PostgreSQL se mantenga levantada y lista para recibir consultas, aunque el servicio hubiera estado idle

El ping externo complementa a Hangfire: cron-job.org puede despertar el sitio desde afuera; Hangfire sigue verificando mientras la app ya está en ejecución.

Configuración opcional:

```json
"App": {
  "BaseUrl": "https://tu-app.onrender.com"
}
```

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL
- (Opcional) cuenta Brevo para emails transaccionales

## Instalación y configuración

1. **Clonar el repositorio**

```bash
git clone https://github.com/nicolasfernandezriesen/BudgetTracker.git
cd BudgetTracker/BudgetTracker
```

2. **Configurar `appsettings.json` / user secrets / variables de entorno**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=BudgetTrackerDB;Username=...;Password=..."
  },
  "EmailSettings": {
    "SmtpServer": "smtp-relay.brevo.com",
    "Port": 587,
    "SenderName": "Budget Tracker",
    "SenderEmail": "tu-remitente@ejemplo.com",
    "Username": "tu-usuario-smtp",
    "Password": "tu-password-smtp",
    "ApiKey": "tu-api-key-brevo"
  },
  "App": {
    "BaseUrl": "https://localhost:7170"
  }
}
```

Si la base está vacía, aplicar los scripts SQL de `Data/Migraciones/`.

3. **Ejecutar**

```bash
dotnet restore
dotnet run
```

Perfil HTTP por defecto: `http://localhost:5036`

4. **Docker (opcional)**

```bash
docker build -f Dockrfile -t budgettracker .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." budgettracker
```

## Estructura del proyecto (resumen)

```
BudgetTracker/
  Controllers/     Home, User, Bill, Income, History, Check
  Jobs/            Jobs recurrentes de Hangfire (DbCheckJob)
  Services/        Lógica de negocio
  Repositories/    Acceso a datos
  Views/           UI Razor
  wwwroot/         CSS / JS / estáticos
  Data/            DbContext + migraciones SQL
```

## Notas

En el banner de invitados puede mostrarse una cuenta demo para pruebas. No commitear secretos de producción; preferir variables de entorno o user secrets.
