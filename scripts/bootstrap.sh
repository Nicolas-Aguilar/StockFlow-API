#!/usr/bin/env bash
set -euo pipefail

run_api=false
if [[ "${1:-}" == "--run-api" ]]; then
  run_api=true
fi

script_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_root/.." && pwd)"
api_project="$repo_root/src/StockFlow.Api/StockFlow.Api.csproj"
infrastructure_project="$repo_root/src/StockFlow.Infrastructure/StockFlow.Infrastructure.csproj"
solution_path="$repo_root/StockFlow.sln"
env_path="$repo_root/.env"

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Required command '$1' was not found in PATH." >&2
    exit 1
  fi
}

generate_secret() {
  if command -v python3 >/dev/null 2>&1; then
    python3 - <<'PY'
import base64
import secrets
print(base64.b64encode(secrets.token_bytes(32)).decode().rstrip('=').replace('+', 'A').replace('/', 'B'))
PY
    return
  fi

  if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 32 | tr -d '=\n' | tr '+/' 'AB'
    return
  fi

  echo "Could not generate a local secret. Install python3 or openssl." >&2
  exit 1
}

read_env_value() {
  local key="$1"
  if [[ -f "$env_path" ]]; then
    awk -F= -v target="$key" '$1 == target { print substr($0, index($0, "=") + 1) }' "$env_path"
  fi
}

require_command dotnet
require_command docker

sql_password="$(read_env_value SQLSERVER_SA_PASSWORD)"
sql_port="$(read_env_value SQLSERVER_SQL_PORT)"
reset_sql_volume=false

if [[ -z "$sql_password" || "$sql_password" == __* ]]; then
  sql_password="LocalDev_$(generate_secret)!aA1"
  reset_sql_volume=true
fi

if [[ -z "$sql_port" ]]; then
  sql_port="1433"
fi

cat > "$env_path" <<EOF
SQLSERVER_SA_PASSWORD=$sql_password
SQLSERVER_SQL_PORT=$sql_port
EOF

connection_string="Server=localhost,$sql_port;Database=StockFlowDb;User Id=sa;Password=$sql_password;TrustServerCertificate=True;"
current_jwt_key="$(dotnet user-secrets list --project "$api_project" 2>/dev/null | awk -F' = ' '$1 == "Jwt:Key" { print $2 }')"

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$connection_string" --project "$api_project" >/dev/null

if [[ -z "$current_jwt_key" ]]; then
  dotnet user-secrets set "Jwt:Key" "LocalJwt_$(generate_secret)" --project "$api_project" >/dev/null
fi

echo "[1/5] Starting SQL Server container..."
if [[ "$reset_sql_volume" == true ]]; then
  echo "Detected new local SQL credentials. Recreating SQL Server volume to keep credentials in sync..."
  docker compose --file "$repo_root/docker-compose.yml" --project-directory "$repo_root" down -v --remove-orphans
fi

docker compose --file "$repo_root/docker-compose.yml" --project-directory "$repo_root" up -d sqlserver

echo "[2/5] Waiting for SQL Server healthcheck..."
for _ in $(seq 1 40); do
  status="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' stockflow-sqlserver 2>/dev/null || true)"
  if [[ "$status" == "healthy" ]]; then
    break
  fi
  sleep 3
done

status="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' stockflow-sqlserver)"
if [[ "$status" != "healthy" ]]; then
  echo "SQL Server container did not become healthy in time. Check 'docker compose logs sqlserver'." >&2
  exit 1
fi

echo "[3/5] Restoring tools and packages..."
dotnet tool restore --tool-manifest "$repo_root/.config/dotnet-tools.json"
dotnet restore "$solution_path"

echo "[4/5] Applying migrations..."
dotnet ef database update --project "$infrastructure_project" --startup-project "$api_project"

echo "[5/5] Building solution..."
dotnet build "$solution_path" --no-restore

echo "StockFlow local environment is ready."
echo "SQL Server: localhost:$sql_port"
echo "Swagger: http://localhost:5133/swagger"

if [[ "$run_api" == true ]]; then
  echo "Starting API..."
  dotnet run --project "$api_project" --no-build
fi
