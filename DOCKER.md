# Running CAPNEXA with Docker

This guide is for running the whole stack locally: SQL Server, the database migration job, the ASP.NET Core API, the React dashboard, and Caddy for HTTPS.

## 1. What you need

- Docker Desktop with the Compose v2 plugin (`docker compose`, not `docker-compose`).
- About 10 GB of free disk space for images and the SQL Server data volume.
- Host ports **80** and **443** available.
- No local Node.js or .NET install is required; both are built inside Docker.

## 2. One-time setup

### 2.1 Create your local `.env`

The repository contains `.env.example`, but never commit your own `.env`.

PowerShell:

```powershell
Copy-Item .env.example .env
```

Then edit `.env` and replace every value that starts with `REPLACE_WITH_`:

| Variable | How to choose a value |
| --- | --- |
| `MSSQL_SA_PASSWORD` | At least 8 characters with upper case, lower case, digit, and symbol. |
| `DB_CONNECTION_STRING` | Use the **same** password as `MSSQL_SA_PASSWORD`. |
| `JWT_KEY` | A random string of at least 64 characters. Example: `[BitConverter]::ToString((1..32 | ForEach-Object { [byte](Get-Random -Max 256) })).Replace('-', '')` |
| `SEED_ADMIN_PASSWORD` | A strong local admin password. |
| `EMAIL_SETTINGS_EMAIL` | Keep the valid-looking placeholder `capnexa-noreply@example.com` unless you have real SMTP credentials. An empty value breaks registration. |
| `GOOGLE_*`, `GITHUB_*` | Leave empty unless you are testing OAuth. |

`.env` is ignored by Git. `.env.example` contains placeholders only.

### 2.2 Optional: make the hostnames resolve

The stack uses:

- `https://capnexa.local` — web dashboard
- `https://api.capnexa.local` — API

Add this to your hosts file if you want the names to work in a browser:

```text
127.0.0.1 capnexa.local
127.0.0.1 api.capnexa.local
```

On Windows the file is `C:\Windows\System32\drivers\etc\hosts`.

You can also skip the hosts file and use `curl --resolve` as shown below.

### 2.3 Optional: trust Caddy's local certificate

Caddy issues a local development certificate on first start. Browser warnings are expected until you trust the root CA.

```powershell
docker compose cp caddy:/data/caddy/pki/authorities/local/root.crt ./caddy-local-root.crt
```

Import `caddy-local-root.crt` into your OS/browser trust store, or continue using `curl -k`. The file is Git-ignored.

## 3. Start the stack (Development)

```powershell
docker compose up --build -d
```

`docker-compose.override.yml` is loaded automatically and sets `ASPNETCORE_ENVIRONMENT=Development`, which enables `/openapi/v1.json` and `/scalar`.

Check status:

```powershell
docker compose ps
```

All long-running services should be `healthy`. The one-shot `migrate` service should show `Exited (0)`.

Wait for logs if needed:

```powershell
docker compose logs -f db migrate api web caddy
```

Open:

- Dashboard: <https://capnexa.local>
- OpenAPI JSON: <https://api.capnexa.local/openapi/v1.json>
- Scalar UI: <https://api.capnexa.local/scalar>

The seeded admin login is the `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD` pair from `.env`.

## 4. Start the stack (Production-like)

The override file is **not** used when you specify only the base file:

```powershell
docker compose -f docker-compose.yml up --build -d
```

In this mode `ASPNETCORE_ENVIRONMENT=Production`, so `/openapi` and `/scalar` return `404`.

## 5. How startup works

1. `db` starts SQL Server and waits for its healthcheck.
2. `migrate` runs `dotnet-ef database update --no-build` once and exits.
3. `api` starts only after `migrate` exits successfully.
4. `web` serves the built Vite app with nginx SPA fallback.
5. `caddy` proxies both hostnames over HTTPS and forwards `X-Forwarded-For` / `X-Forwarded-Proto`.

The API no longer runs `Database.Migrate()` at startup. Seeding of roles and the admin user still happens at API startup; the migration itself is the one-shot job.

## 6. Stopping and resetting

Stop without deleting data:

```powershell
docker compose down
```

Start again; the SQL Server named volume is preserved:

```powershell
docker compose up -d
```

Delete the database and Caddy state as well (**destructive**):

```powershell
docker compose down -v
```

## 7. Configuration keys

Compose maps `.env` values to the real .NET configuration keys using double underscores:

| `.env` variable | .NET configuration key |
| --- | --- |
| `DB_CONNECTION_STRING` | `ConnectionStrings__DefaultConnection` |
| `JWT_KEY` | `Jwt__Key` |
| `JWT_ISSUER` | `Jwt__Issuer` |
| `JWT_AUDIENCE` | `Jwt__Audience` |
| `APP_API_BASE_URL` | `AppUrls__ApiBaseUrl` |
| `APP_FRONTEND_BASE_URL` | `AppUrls__FrontendBaseUrl` |
| `GOOGLE_CLIENT_ID` | `Auth__Google__ClientId` |
| `GITHUB_CLIENT_SECRET` | `Auth__GitHub__ClientSecret` |
| `EMAIL_SETTINGS_EMAIL` | `EmailSettings__Email` |
| `SEED_ADMIN_PASSWORD` | `SeedAdmin__Password` |

The full list is in `docker-compose.yml`.

## 8. Quick smoke test through Caddy

If you did not add hosts-file entries:

```powershell
curl.exe -k --resolve api.capnexa.local:443:127.0.0.1 https://api.capnexa.local/openapi/v1.json
curl.exe -k --resolve capnexa.local:443:127.0.0.1 https://capnexa.local/
```

A student account needs a confirmed email before login. With SMTP disabled, registration succeeds but returns `emailConfirmationSent: false`. For local testing, confirm the test user directly:

```powershell
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<sa password from .env>" -C -d CapnexaDb -I -Q "UPDATE dbo.AspNetUsers SET EmailConfirmed=1 WHERE Email='student@example.edu';"
```

The `-I` flag sets `QUOTED_IDENTIFIER ON`, which Identity's filtered indexes require.

## 9. Troubleshooting

| Symptom | Fix |
| --- | --- |
| `migrate` exits with `dotnet tool restore` message | Rebuild the image: `docker compose build migrate`. The image invokes `dotnet-ef` directly. |
| `web` is unhealthy with `wget: can't connect` | The healthcheck uses `127.0.0.1` to avoid IPv6 `localhost` resolution; rebuild/recreate with `docker compose up -d --force-recreate web`. |
| Build fails with `read-only file system` | Docker's disk image is full. Check `docker system df`, remove unused images (`docker image prune`) or increase/move the Docker disk image in Docker Desktop settings, then restart Docker Desktop. |
| `443` already in use | Stop the other service or change `CADDY_HTTPS_PORT` in `.env`. |
| API redirects to HTTP behind Caddy | Confirm `UseForwardedHeaders()` runs before HTTPS redirection and that Caddy sends `X-Forwarded-Proto`. |

## 10. Security notes

- Never commit `.env`, certificates, or generated secrets.
- Rotate any JWT, SMTP, or admin passwords that were ever committed in older git history.
- The local certificates and `local_certs` mode are for development only; use a trusted CA and real DNS names in production.
