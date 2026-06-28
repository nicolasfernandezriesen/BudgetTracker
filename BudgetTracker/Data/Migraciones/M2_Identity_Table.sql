CREATE TABLE users_backup AS
SELECT * FROM users;

-- Verificar que el backup se creó correctamente
SELECT COUNT(*) as usuarios_backup FROM users_backup;


-- ================================================================
-- PASO 2: AGREGAR COLUMNAS DE IDENTITY A LA TABLA users
-- ================================================================

-- Agregar columna normalizada de email (requerida por Identity)
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS normalized_email VARCHAR(256);

-- Agregar columna normalizada de nombre de usuario (requerida por Identity)
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS normalized_user_name VARCHAR(256);

-- Agregar columna de confirmación de email
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS email_confirmed BOOLEAN DEFAULT false;

-- Agregar columna de teléfono (opcional pero requerida por Identity)
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS phone_number VARCHAR(20);

-- Agregar columna de confirmación de teléfono
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS phone_number_confirmed BOOLEAN DEFAULT false;

-- Agregar columna para autenticación de dos factores
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS two_factor_enabled BOOLEAN DEFAULT false;

-- Agregar columna para fecha de fin de bloqueo
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS lockout_end TIMESTAMP WITH TIME ZONE;

-- Agregar columna para habilitar bloqueo
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS lockout_enabled BOOLEAN DEFAULT true;

-- Agregar columna para contador de intentos fallidos
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS access_failed_count INT DEFAULT 0;

-- Agregar columna de security stamp (para invalidar tokens)
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS security_stamp VARCHAR(255);

-- Agregar columna de concurrency stamp (para control de concurrencia)
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS concurrency_stamp VARCHAR(255);


-- ================================================================
-- PASO 3: POBLAR LAS NUEVAS COLUMNAS CON DATOS
-- ================================================================

-- Poblar campos normalizados (Identity usa mayúsculas para búsquedas rápidas)
UPDATE users 
SET 
    normalized_email = UPPER(user_email),
    normalized_user_name = UPPER(user_name),
    email_confirmed = true, -- Marcar emails existentes como confirmados
    lockout_enabled = true,
    access_failed_count = 0,
    two_factor_enabled = false,
    phone_number_confirmed = false,
    security_stamp = MD5(RANDOM()::text || user_id::text),
    concurrency_stamp = MD5(RANDOM()::text || NOW()::text)
WHERE normalized_email IS NULL OR normalized_user_name IS NULL;

-- Verificar que los datos se poblaron correctamente
SELECT 
    user_id,
    user_name,
    normalized_user_name,
    user_email,
    normalized_email,
    email_confirmed,
    lockout_enabled
FROM users 
LIMIT 5;


-- ================================================================
-- PASO 4: CREAR ÍNDICES PARA MEJORAR RENDIMIENTO
-- ================================================================

-- Índice para búsqueda por email normalizado (mejora rendimiento de login)
CREATE INDEX IF NOT EXISTS IX_users_normalized_email 
ON users(normalized_email);

-- Índice para búsqueda por nombre de usuario normalizado
CREATE INDEX IF NOT EXISTS IX_users_normalized_user_name 
ON users(normalized_user_name);


-- ================================================================
-- PASO 5: CREAR TABLAS DE IDENTITY
-- ================================================================

-- Tabla de Roles
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" VARCHAR(255)
);

-- Índice único para búsqueda rápida de roles
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" 
ON "AspNetRoles" ("NormalizedName");


-- Tabla de Relación Usuario-Rol
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" INT NOT NULL,
    "RoleId" INT NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES users(user_id) ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" 
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

-- Índices para mejorar búsquedas
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" 
ON "AspNetUserRoles" ("RoleId");


-- Tabla de Claims de Usuario (reclamos/permisos adicionales)
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL,
    "ClaimType" VARCHAR(255),
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetUserClaims_users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES users(user_id) ON DELETE CASCADE
);

-- Índice para búsqueda por usuario
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" 
ON "AspNetUserClaims" ("UserId");


-- Tabla de Logins Externos (Google, Microsoft, etc.)
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" VARCHAR(128) NOT NULL,
    "ProviderKey" VARCHAR(128) NOT NULL,
    "ProviderDisplayName" VARCHAR(255),
    "UserId" INT NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES users(user_id) ON DELETE CASCADE
);

-- Índice para búsqueda por usuario
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" 
ON "AspNetUserLogins" ("UserId");


-- Tabla de Tokens de Usuario (para reset de password, etc.)
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" INT NOT NULL,
    "LoginProvider" VARCHAR(128) NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES users(user_id) ON DELETE CASCADE
);


-- Tabla de Claims de Rol
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" INT NOT NULL,
    "ClaimType" VARCHAR(255),
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" 
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

-- Índice para búsqueda por rol
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" 
ON "AspNetRoleClaims" ("RoleId");


-- ================================================================
-- PASO 6: INSERTAR ROLES PREDETERMINADOS (OPCIONAL)
-- ================================================================
-- Puedes insertar roles básicos si los necesitas

INSERT INTO "AspNetRoles" ("Name", "NormalizedName", "ConcurrencyStamp")
VALUES 
    ('Admin', 'ADMIN', MD5(RANDOM()::text)),
    ('User', 'USER', MD5(RANDOM()::text)),
    ('Test', 'TEST', MD5(RANDOM()::text))
ON CONFLICT DO NOTHING;

-- Verificar roles creados
SELECT * FROM "AspNetRoles";


-- ================================================================
-- PASO 7: VERIFICACIÓN COMPLETA
-- ================================================================

-- Verificar estructura de la tabla users
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'users' 
ORDER BY ordinal_position;

-- Verificar que todos los usuarios tienen datos completos
SELECT 
    COUNT(*) as total_usuarios,
    COUNT(normalized_email) as con_email_normalizado,
    COUNT(normalized_user_name) as con_nombre_normalizado,
    SUM(CASE WHEN email_confirmed THEN 1 ELSE 0 END) as emails_confirmados
FROM users;

-- Verificar tablas de Identity creadas
SELECT table_name 
FROM information_schema.tables 
WHERE table_name LIKE 'AspNet%'
ORDER BY table_name;

-- Ver primeros 3 usuarios con todos sus datos
SELECT 
    user_id,
    user_name,
    user_email,
    normalized_email,
    email_confirmed,
    lockout_enabled,
    access_failed_count,
    two_factor_enabled
FROM users 
LIMIT 3;

DROP TABLE users_backup;