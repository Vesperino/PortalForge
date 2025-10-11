# PortalForge - Deployment Guide

## Architecture

- **Frontend**: Nuxt 3 SSR in Docker container → `localhost:5001`
- **Backend**: .NET 8.0 as systemd service → `localhost:22022`
- **Database**: PostgreSQL via Supabase (cloud)
- **Deployment**: GitHub Actions with self-hosted runner
- **Web Server**: Nginx reverse proxy with SSL

---

## Prerequisites on VPS

1. **GitHub Actions Runner** installed as `githubactions` user
2. **Docker** installed
3. **Nginx** installed with your existing configuration
4. **.NET 8.0 Runtime** installed

---

## First-Time Setup on VPS

### 1. Create application directory

```bash
# As githubactions user
mkdir -p ~/portalforge/backend
```

### 2. Create backend .env file

```bash
nano ~/portalforge/backend/.env
```

Paste and fill in your values:
```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:22022
ConnectionStrings__DefaultConnection=Host=db.xxx.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD
Supabase__Url=https://xxx.supabase.co
Supabase__ServiceRoleKey=YOUR_SERVICE_ROLE_KEY
Jwt__Secret=YOUR_JWT_SECRET_32_CHARS_MIN
Jwt__Issuer=PortalForge
Jwt__Audience=PortalForge
Jwt__ExpirationMinutes=480
Serilog__MinimumLevel__Default=Information
```

```bash
# Secure the file
chmod 600 ~/portalforge/backend/.env
```

### 3. Configure Nginx

Add two configurations for reverse proxy:

**Backend API** (`/etc/nginx/sites-available/portalforge-backend`):
```nginx
upstream portalforge_backend {
    server 127.0.0.1:22022;
}

server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://portalforge_backend;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Frontend** (`/etc/nginx/sites-available/portalforge-frontend`):
```nginx
upstream portalforge_frontend {
    server 127.0.0.1:5001;
}

server {
    listen 80;
    server_name portal.yourdomain.com;

    location / {
        proxy_pass http://portalforge_frontend;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable and reload:
```bash
sudo ln -s /etc/nginx/sites-available/portalforge-backend /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/portalforge-frontend /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

Add SSL with your existing method (Certbot, manual, etc.).

---

## GitHub Secrets Configuration

Go to: **Repository → Settings → Secrets and variables → Actions**

Add these secrets:

| Secret Name | Value | Description |
|-------------|-------|-------------|
| `SUPABASE_URL` | `https://xxx.supabase.co` | Supabase project URL |
| `SUPABASE_ANON_KEY` | `eyJhbG...` | Supabase anon public key |
| `FRONTEND_API_URL` | `https://api.yourdomain.com` | Your backend API URL |

---

## Deployment Process

### Automatic Deployment

Push to `main` branch triggers automatic deployment:

```bash
git push origin main
```

- Changes in `backend/` → Deploy Backend workflow runs
- Changes in `frontend/` → Deploy Frontend workflow runs

### Manual Deployment

Go to **GitHub → Actions** tab, select workflow, click **Run workflow**.

---

## How It Works

### Backend Deployment

1. Runner checkouts code
2. Builds .NET project with `dotnet publish`
3. Publishes to `~/portalforge/backend/`
4. Copies systemd service file to `/etc/systemd/system/`
5. Restarts `portalforge-backend` service
6. Service runs on `localhost:22022`
7. Nginx proxies `api.yourdomain.com` → `localhost:22022`

### Frontend Deployment

1. Runner checkouts code
2. Builds Docker image with Nuxt 3 SSR
3. Stops old container
4. Runs new container on port `5001`
5. Nginx proxies `portal.yourdomain.com` → `localhost:5001`

---

## Useful Commands

### Backend (systemd service)

```bash
# Check status
sudo systemctl status portalforge-backend

# View logs
sudo journalctl -u portalforge-backend -f

# Restart
sudo systemctl restart portalforge-backend

# Stop
sudo systemctl stop portalforge-backend
```

### Frontend (Docker container)

```bash
# Check status
docker ps | grep portalforge-frontend

# View logs
docker logs -f portalforge-frontend

# Restart
docker restart portalforge-frontend

# Stop
docker stop portalforge-frontend
```

### Manual health checks

```bash
# Backend
curl http://localhost:22022/health

# Frontend
curl http://localhost:5001
```

---

## Troubleshooting

### Backend not starting

```bash
# Check logs
sudo journalctl -u portalforge-backend -n 50

# Check .env file
cat ~/portalforge/backend/.env

# Check if port is already in use
sudo netstat -tlnp | grep 22022
```

### Frontend not starting

```bash
# Check Docker logs
docker logs portalforge-frontend

# Check if port is already in use
sudo netstat -tlnp | grep 5001

# Rebuild container
cd frontend
docker build -t portalforge-frontend:latest .
docker run -d --name portalforge-frontend -p 5001:3000 portalforge-frontend:latest
```

### Nginx 502 Bad Gateway

```bash
# Check if services are running
sudo systemctl status portalforge-backend
docker ps | grep portalforge-frontend

# Check Nginx error logs
sudo tail -f /var/log/nginx/error.log

# Test upstream directly
curl http://localhost:22022/health
curl http://localhost:5001
```

---

## File Structure

```
PortalForge/
├── backend/
│   ├── .env.example                      # Template for environment variables
│   ├── portalforge-backend.service       # Systemd service configuration
│   └── PortalForge.Api/                  # .NET 8.0 Web API
├── frontend/
│   ├── .env.example                      # Template for environment variables
│   ├── Dockerfile                        # Docker configuration
│   └── (Nuxt 3 files)
├── .github/workflows/
│   ├── deploy-backend.yml                # Backend deployment workflow
│   └── deploy-frontend.yml               # Frontend deployment workflow
└── DEPLOYMENT.md                         # This file
```

---

## Security Notes

- `.env` files contain secrets → **NEVER commit to Git**
- Systemd service runs as `githubactions` user (not root)
- Docker container runs with `--restart unless-stopped`
- All services bind to `localhost` only (not exposed to internet)
- Nginx handles SSL/TLS termination
- GitHub Secrets are encrypted at rest

---

## Next Steps

1. Configure Supabase database schema
2. Add database migrations
3. Implement authentication flow
4. Add monitoring and alerting
5. Setup backup strategy

---

**Questions?** Check the main [README.md](README.md) or PRD in [.ai/prd.md](.ai/prd.md).
