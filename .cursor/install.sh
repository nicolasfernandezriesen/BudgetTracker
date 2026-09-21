#!/usr/bin/env bash
# Idempotent bootstrap for the BudgetTracker Cloud Agent environment.
# Installs the .NET 8 SDK and PostgreSQL, prepares the database and schema,
# then restores and builds the ASP.NET Core project.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DB_NAME="budgettracker"
DB_USER="budget"
DB_PASS="budget"

echo "==> Installing system dependencies (.NET 8 SDK, PostgreSQL)"
NEED_APT_UPDATE=1
if ! command -v dotnet >/dev/null 2>&1 || ! command -v pg_ctlcluster >/dev/null 2>&1; then
  sudo apt-get update -qq
  NEED_APT_UPDATE=0
fi
if ! command -v dotnet >/dev/null 2>&1; then
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-8.0
fi
if ! command -v pg_ctlcluster >/dev/null 2>&1; then
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y -qq postgresql postgresql-contrib
fi

echo "==> Ensuring PostgreSQL is running"
sudo pg_ctlcluster 16 main start 2>/dev/null || true
for _ in $(seq 1 30); do
  if sudo -u postgres pg_isready -q; then break; fi
  sleep 1
done

echo "==> Ensuring database role and database exist"
if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_roles WHERE rolname='${DB_USER}'" | grep -q 1; then
  sudo -u postgres psql -c "CREATE ROLE ${DB_USER} LOGIN PASSWORD '${DB_PASS}';"
fi
if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='${DB_NAME}'" | grep -q 1; then
  sudo -u postgres createdb -O "${DB_USER}" "${DB_NAME}"
fi

echo "==> Applying SQL migrations (only on a fresh database)"
export PGPASSWORD="${DB_PASS}"
HAS_MIGRATIONS="$(psql -h localhost -U "${DB_USER}" -d "${DB_NAME}" -tAc "SELECT to_regclass('public.migrations')" 2>/dev/null || true)"
if [ "${HAS_MIGRATIONS}" != "migrations" ]; then
  cd "${REPO_ROOT}/BudgetTracker/Data/Migraciones"
  for f in M1_Inital_Create.sql M2_Identity_Table.sql M3_Add_SubCategorys.sql M4_Add_MigrationTable.sql M5_Add_IsDarkTheme.sql; do
    echo "    - ${f}"
    psql -h localhost -U "${DB_USER}" -d "${DB_NAME}" -v ON_ERROR_STOP=1 -q -f "${f}"
  done
else
  echo "    migrations table already present, skipping"
fi

echo "==> Restoring and building the ASP.NET Core project"
cd "${REPO_ROOT}/BudgetTracker"
dotnet restore
dotnet build -c Release --no-restore

echo "==> Install complete"
