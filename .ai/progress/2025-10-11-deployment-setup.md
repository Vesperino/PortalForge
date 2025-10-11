# Progress Log - 2025-10-11: Deployment Infrastructure Setup

## Summary
Complete deployment infrastructure setup with GitHub Actions self-hosted runner, Docker containerization for both backend and frontend, and Nginx reverse proxy configuration.

## Completed Tasks

### ✅ GitHub Actions Self-Hosted Runner Configuration
1. **Runner Installation on VPS**
   - Runner installed at: `/home/[VPS_USER]/actions-runner`
   - Running as user: `[VPS_USER]`
   - Runner name: `portalforge-vps`
   - Labels: `self-hosted`, `vps`, `production`
   - Status: ✅ Active and connected to GitHub

2. **Docker Permissions**
   - Added `[VPS_USER]` to `docker` group
   - Restarted runner service to apply permissions
   - Docker commands now work without sudo

3. **VPS Environment**
   - IP Address: `[VPS_IP]`
   - SSH Port: `[SSH_PORT]`
   - Nginx: ✅ Installed and running
   - Docker: ✅ Installed and configured
   - Docker Compose: ✅ Installed

### ✅ Backend Deployment (.NET 8.0)

1. **Docker Configuration**
   - Created `backend/Dockerfile` with multi-stage build
   - Base image: `mcr.microsoft.com/dotnet/sdk:8.0` (build)
   - Runtime image: `mcr.microsoft.com/dotnet/aspnet:8.0`
   - Exposed port: `5000` (internal), `22022` (external)
   - Health check: `curl http://localhost:5000/health`
   - Non-root user: `appuser` (UID 1000)

2. **GitHub Actions Workflow**
   - File: `.github/workflows/deploy-backend.yml`
   - Trigger: Push to `main` branch with `backend/**` changes or manual dispatch
   - Steps:
     1. Checkout code
     2. Stop existing container
     3. Remove existing container
     4. Build Docker image
     5. Run Docker container with environment variables
     6. Clean up old images
     7. Check container status
     8. Health check endpoint

3. **Backend Health Endpoint**
   - Added `/health` endpoint in `Program.cs`
   - Returns: `{ status: "healthy", timestamp: "...", environment: "..." }`
   - Used for Docker HEALTHCHECK and deployment verification

4. **Swagger Configuration**
   - Enabled Swagger UI in production for MVP development
   - Access: `http://[VPS_IP]:22022/swagger`
   - Will be restricted to development only in future production releases

5. **Environment Variables**
   - Location: `~/portalforge/backend/.env` on VPS
   - Variables:
     - `ASPNETCORE_ENVIRONMENT=Production`
     - `ASPNETCORE_URLS=http://+:5000`
     - `ConnectionStrings__DefaultConnection` (Supabase PostgreSQL)
     - `Supabase__Url`
     - `Supabase__ServiceRoleKey`
     - `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationMinutes`
     - `Serilog__MinimumLevel__Default`

6. **Deployment Status**
   - ✅ Docker container running: `portalforge-backend`
   - ✅ Port binding: `0.0.0.0:22022->5000/tcp`
   - ✅ Health check: Passing
   - ✅ Auto-restart: `unless-stopped`

### ✅ Frontend Deployment (Nuxt 3)

1. **Docker Configuration**
   - Created `frontend/Dockerfile` with multi-stage build
   - Build stage: `node:20-alpine` with `npm ci` and `npm run build`
   - Runtime stage: `node:20-alpine` with `.output` folder (SSR)
   - Exposed port: `3000` (internal), `5001` (external)
   - Health check: `wget http://localhost:3000`
   - Non-root user: `appuser` (UID 1001, fixed GID conflict)
   - Signal handling: `dumb-init` for proper process management

2. **GitHub Actions Workflow**
   - File: `.github/workflows/deploy-frontend.yml`
   - Trigger: Push to `main` branch with `frontend/**` changes or manual dispatch
   - Build arguments:
     - `NUXT_PUBLIC_SUPABASE_URL` (from GitHub Secrets)
     - `NUXT_PUBLIC_SUPABASE_KEY` (from GitHub Secrets)
     - `NUXT_PUBLIC_API_BASE_URL` (from GitHub Secrets)
   - Runtime environment variables passed to container
   - Steps similar to backend workflow

3. **Docker Build Fix**
   - **Issue**: GID 1000 conflict in Alpine Linux
   - **Solution**: Changed to GID 1001 for `appuser`
   - **Commit**: `f2b385a` - "fix: Change frontend Docker user GID to avoid conflict"

4. **Environment Variables**
   - Build-time (baked into image):
     - `NUXT_PUBLIC_SUPABASE_URL`
     - `NUXT_PUBLIC_SUPABASE_KEY`
     - `NUXT_PUBLIC_API_BASE_URL`
   - Runtime (passed to container):
     - `NODE_ENV=production`
     - Plus all public env vars repeated

5. **Deployment Status**
   - ✅ Docker container running: `portalforge-frontend`
   - ✅ Port binding: `0.0.0.0:5001->3000/tcp`
   - ✅ Health check: Passing
   - ✅ Auto-restart: `unless-stopped`
   - ✅ Nuxt SSR: Working correctly

### ✅ DNS Configuration

1. **Domain**: `krablab.pl`
2. **DNS Provider**: Zenbox.pl
3. **DNS Records** (Created):
   ```
   Name        Type    Value           TTL
   api         A       [VPS_IP]   900
   portal      A       [VPS_IP]   900
   ```
   - Full subdomains: `api.krablab.pl`, `portal.krablab.pl`
   - Status: ✅ DNS records added
   - Propagation: In progress (up to 48h, usually < 1h)

### 🚧 Nginx Reverse Proxy Configuration

1. **Configuration Files Created**
   - `.deployment/nginx/api.krablab.pl.conf` - Backend API proxy
   - `.deployment/nginx/portal.krablab.pl.conf` - Frontend portal proxy
   - `.deployment/nginx/README.md` - Installation instructions

2. **Backend API Config** (`api.krablab.pl.conf`):
   - Listen: Port 80 (HTTP)
   - Server name: `api.krablab.pl`
   - Proxy to: `http://localhost:22022`
   - Headers: `Host`, `X-Real-IP`, `X-Forwarded-For`, `X-Forwarded-Proto`
   - Logging: `/var/log/nginx/api.krablab.pl.{access,error}.log`
   - Security headers: `X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection`

3. **Frontend Portal Config** (`portal.krablab.pl.conf`):
   - Listen: Port 80 (HTTP)
   - Server name: `portal.krablab.pl`
   - Proxy to: `http://localhost:5001`
   - WebSocket support for Nuxt HMR
   - Static asset caching for `/_nuxt/` (1 year)
   - Logging: `/var/log/nginx/portal.krablab.pl.{access,error}.log`

4. **Installation Steps** (To be executed on VPS):
   ```bash
   # 1. Copy configs to sites-available
   sudo cp api.krablab.pl.conf /etc/nginx/sites-available/
   sudo cp portal.krablab.pl.conf /etc/nginx/sites-available/

   # 2. Create symbolic links to sites-enabled
   sudo ln -s /etc/nginx/sites-available/api.krablab.pl.conf /etc/nginx/sites-enabled/
   sudo ln -s /etc/nginx/sites-available/portal.krablab.pl.conf /etc/nginx/sites-enabled/

   # 3. Test configuration
   sudo nginx -t

   # 4. Reload Nginx
   sudo systemctl reload nginx
   ```

5. **Status**: ⏳ Nginx configs created, not yet installed on VPS

### ✅ GitHub Repository Secrets

Configured the following secrets in GitHub repository settings:
- `SUPABASE_URL` - Supabase project URL
- `SUPABASE_ANON_KEY` - Supabase anon/public key (frontend)
- `FRONTEND_API_URL` - Backend API URL for frontend (`http://api.krablab.pl`)
- Note: `SUPABASE_SERVICE_ROLE_KEY` is stored in VPS `.env` file only (not in GitHub)

## Technical Decisions

### Decision 1: GitHub Actions Self-Hosted Runner vs SSH Deployment
**Chosen**: Self-hosted GitHub Actions runner

**Rationale**:
- ✅ No need to store SSH private keys in GitHub Secrets
- ✅ Better security - runner runs on VPS with local Docker access
- ✅ Simpler workflow - no SSH connection overhead
- ✅ Native GitHub integration
- ✅ Easier to audit and monitor deployments
- ❌ Requires runner maintenance (minor overhead)

**Implementation**:
- Runner installed as systemd service
- Runs as dedicated user (`abialecki`)
- Auto-restarts on failure
- Labels: `self-hosted`, `vps`, `production`

### Decision 2: Docker vs Systemd Services
**Chosen**: Docker containers for both backend and frontend

**Rationale**:
- ✅ Consistent deployment strategy for both services
- ✅ Easy rollback (just run previous image)
- ✅ Environment isolation
- ✅ No need to install .NET or Node.js on VPS
- ✅ Portable - can move to any Docker-compatible host
- ✅ Built-in health checks
- ✅ Resource limits can be easily set
- ❌ Slightly more complex than systemd (acceptable trade-off)

**Implementation**:
- Multi-stage Dockerfiles for optimized image sizes
- Non-root users for security
- Health checks for monitoring
- Auto-restart policy: `unless-stopped`

### Decision 3: Docker Compose vs Docker Run
**Chosen**: `docker run` commands in GitHub Actions workflows

**Rationale**:
- ✅ Simpler for single-container deployments
- ✅ No need for docker-compose.yml on VPS
- ✅ More control in CI/CD pipeline
- ✅ Easier to pass secrets from GitHub
- ❌ Less convenient for local multi-container dev (acceptable - use separate docker-compose for local)

**Implementation**:
- Each workflow manages its own container
- Environment variables passed via `--env-file` and `-e` flags
- Port mappings explicit in workflow

### Decision 4: HTTP-only vs HTTPS (MVP)
**Chosen**: HTTP-only for initial deployment, SSL to be added later

**Rationale**:
- ✅ Faster MVP deployment
- ✅ Existing SSL cert doesn't cover subdomains
- ✅ Can add SSL later with Certbot or wildcard cert
- ⚠️ Acceptable for internal testing, **must add SSL before production use**

**Next Steps for SSL**:
1. Install Certbot: `sudo apt install certbot python3-certbot-nginx`
2. Get certificates: `sudo certbot --nginx -d api.krablab.pl -d portal.krablab.pl`
3. Auto-renewal: Certbot installs cron job automatically

### Decision 5: Environment Variable Strategy
**Chosen**: Hybrid approach - build-time for frontend public vars, runtime for backend secrets

**Rationale**:
- ✅ Frontend public vars baked into build (faster runtime)
- ✅ Backend secrets in `.env` file on VPS (not in repo or image)
- ✅ GitHub Secrets used only for CI/CD access to sensitive values
- ✅ Clear separation between public and private config

**Implementation**:
- Frontend: `--build-arg` for public Supabase keys
- Backend: `--env-file ~/portalforge/backend/.env` for secrets
- GitHub Secrets: Only for passing to workflows, not committed

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    GitHub Repository                     │
│                   Vesperino/PortalForge                  │
└────────────────────┬────────────────────────────────────┘
                     │ Push to main
                     ↓
┌─────────────────────────────────────────────────────────┐
│              GitHub Actions Workflows                    │
│  ┌──────────────────┐      ┌─────────────────────┐     │
│  │ Deploy Backend   │      │  Deploy Frontend    │     │
│  │ (on push to      │      │  (on push to        │     │
│  │  backend/**)     │      │   frontend/**)      │     │
│  └──────────────────┘      └─────────────────────┘     │
└────────────────────┬────────────────┬───────────────────┘
                     │                │
                     ↓                ↓
┌─────────────────────────────────────────────────────────┐
│        VPS ([VPS_IP]:[SSH_PORT])                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │   GitHub Actions Runner ([VPS_USER])             │  │
│  │   - Listens for workflow jobs                    │  │
│  │   - Has Docker permissions                       │  │
│  │   - Executes deployment steps                    │  │
│  └──────────────────────────────────────────────────┘  │
│                                                          │
│  ┌───────────────────┐       ┌────────────────────┐    │
│  │ Docker Container  │       │ Docker Container   │    │
│  │ portalforge-      │       │ portalforge-       │    │
│  │ backend           │       │ frontend           │    │
│  │                   │       │                    │    │
│  │ .NET 8.0 API      │       │ Nuxt 3 SSR         │    │
│  │ Port: 22022:5000  │       │ Port: 5001:3000    │    │
│  │ Health: /health   │       │ Health: /          │    │
│  └───────────────────┘       └────────────────────┘    │
│           ↑                           ↑                 │
│           │                           │                 │
│  ┌────────┴───────────────────────────┴──────────────┐ │
│  │            Nginx Reverse Proxy                    │ │
│  │  ┌─────────────────┐    ┌────────────────────┐   │ │
│  │  │ api.krablab.pl  │    │ portal.krablab.pl  │   │ │
│  │  │ → :22022        │    │ → :5001            │   │ │
│  │  └─────────────────┘    └────────────────────┘   │ │
│  │            Port 80 (HTTP)                         │ │
│  └───────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                     ↑
                     │ DNS Resolution
                     │
┌─────────────────────────────────────────────────────────┐
│                  DNS (Zenbox.pl)                        │
│  api.krablab.pl    → [VPS_IP]                     │
│  portal.krablab.pl → [VPS_IP]                     │
└─────────────────────────────────────────────────────────┘
                     ↑
                     │
                  Users
```

## Files Created/Modified

### Created Files:
1. **Docker Configuration**
   - `backend/Dockerfile` - Backend multi-stage Dockerfile
   - `backend/.env.example` - Environment variable template
   - `frontend/Dockerfile` - Frontend multi-stage Dockerfile
   - `frontend/.env.example` - Environment variable template

2. **GitHub Actions Workflows**
   - `.github/workflows/deploy-backend.yml` - Backend deployment workflow
   - `.github/workflows/deploy-frontend.yml` - Frontend deployment workflow

3. **Nginx Configuration**
   - `.deployment/nginx/api.krablab.pl.conf` - Backend API proxy config
   - `.deployment/nginx/portal.krablab.pl.conf` - Frontend portal proxy config
   - `.deployment/nginx/README.md` - Installation instructions

4. **Documentation**
   - `.ai/progress/2025-10-11-deployment-setup.md` - This file
   - `.ai/decisions/002-github-actions-deployment.md` - ADR for deployment strategy

### Modified Files:
1. `backend/PortalForge.Api/Program.cs`
   - Added `/health` endpoint
   - Enabled Swagger in production

2. `.gitignore` (should include):
   - `backend/.env`
   - `frontend/.env`
   - `.DS_Store`
   - `*.log`

## Testing Results

### Backend Tests
✅ **Docker Build**: Success
✅ **Container Start**: Success
✅ **Health Check**: `http://localhost:22022/health` → 200 OK
✅ **Swagger UI**: `http://localhost:22022/swagger` → Accessible
✅ **Direct IP Access**: `http://[VPS_IP]:22022/health` → Works
⏳ **Domain Access**: `http://api.krablab.pl/health` → Pending Nginx config

### Frontend Tests
✅ **Docker Build**: Success (after GID fix)
✅ **Container Start**: Success
✅ **Health Check**: `http://localhost:5001` → 200 OK
✅ **Direct IP Access**: `http://[VPS_IP]:5001` → Works
⏳ **Domain Access**: `http://portal.krablab.pl` → Pending Nginx config

### GitHub Actions Tests
✅ **Runner Connection**: Active
✅ **Backend Workflow**: Passing
✅ **Frontend Workflow**: Passing (after GID fix)
✅ **Docker Permissions**: Working
✅ **Secrets Access**: Working
✅ **Build Artifacts**: Optimized images

## Issues Encountered & Resolutions

### Issue 1: Docker Permission Denied
**Error**: `permission denied while trying to connect to the Docker daemon socket`

**Root Cause**: User `[VPS_USER]` (running runner) not in `docker` group

**Resolution**:
```bash
sudo usermod -aG docker [VPS_USER]
cd ~/actions-runner
sudo ./svc.sh stop
sudo ./svc.sh start
# Required logout/login for group to take effect
```

**Status**: ✅ Resolved

### Issue 2: Health Check 404
**Error**: `curl: (22) The requested URL returned error: 404`

**Root Cause**: Backend didn't have `/health` endpoint implemented

**Resolution**:
- Added `/health` endpoint in `Program.cs`
- Returns JSON with status, timestamp, environment

**Commit**: `1dbab8b` - "feat: Add health check endpoint to backend API"

**Status**: ✅ Resolved

### Issue 3: Frontend Docker Build - GID Conflict
**Error**: `addgroup: gid '1000' in use`

**Root Cause**: Alpine Linux base image already has group with GID 1000

**Resolution**:
- Changed `appuser` GID from 1000 to 1001
- Updated Dockerfile to use GID 1001

**Commit**: `f2b385a` - "fix: Change frontend Docker user GID to avoid conflict"

**Status**: ✅ Resolved

### Issue 4: GitHub Actions Not Triggering
**Error**: Push didn't trigger workflows

**Root Cause**: Workflows have `paths` filter - only trigger on specific directories

**Resolution**:
- Use manual workflow dispatch from GitHub UI
- Or make changes to `backend/**` or `frontend/**` paths

**Status**: ✅ Resolved (documented)

### Issue 5: Swagger Not Accessible in Production
**Error**: 404 on `/swagger` endpoint

**Root Cause**: Swagger only enabled in Development environment

**Resolution**:
- Removed environment check
- Enabled Swagger in all environments for MVP
- Added comment to restrict in future production

**Commit**: `0812c78` - "feat: Enable Swagger in production for MVP development"

**Status**: ✅ Resolved

## Blockers & Risks

### Current Blockers
- ⏳ Nginx configuration not yet applied on VPS (manual step required)
- ⏳ DNS propagation may take up to 48h (usually < 1h)

### Potential Risks
1. **SSL Certificate** - Current cert doesn't cover subdomains
   - **Mitigation**: Use Certbot or get wildcard cert from Zenbox
   - **Priority**: Medium (can deploy with HTTP initially)

2. **Secrets Management** - `.env` files on VPS are manual
   - **Mitigation**: Document in runbook, use secret management tool later
   - **Priority**: Low (acceptable for MVP)

3. **Single Point of Failure** - One VPS, no redundancy
   - **Mitigation**: Regular backups, monitoring
   - **Priority**: Low (acceptable for MVP)

4. **No Automated Rollback** - Rollback requires manual intervention
   - **Mitigation**: Document rollback procedure
   - **Priority**: Medium (should implement soon)

## Metrics

- **Time Spent**: ~3-4 hours (including troubleshooting)
- **Commits**: 4
  - `1dbab8b` - Add health endpoint
  - `f2b385a` - Fix frontend Docker GID
  - `0812c78` - Enable Swagger in production
  - (Next commit will add Nginx configs)
- **Docker Images Built**: 2 (backend, frontend)
- **Workflows Created**: 2 (deploy-backend, deploy-frontend)
- **Nginx Configs Created**: 2 (api, portal)
- **DNS Records Added**: 2 (api, portal)
- **Deployment Time**: ~2-3 minutes per service

## Notes

### Security Considerations
- ✅ Non-root Docker containers
- ✅ Health checks implemented
- ✅ Environment variables separated from code
- ✅ GitHub Secrets for sensitive data
- ⏳ SSL/HTTPS to be added
- ⏳ Rate limiting to be configured
- ⏳ CORS to be properly configured

### Performance Considerations
- ✅ Multi-stage Docker builds (smaller images)
- ✅ Image layer caching
- ✅ Nuxt SSR for fast initial load
- ✅ Static asset caching in Nginx config
- ⏳ CDN for static assets (future)
- ⏳ Database connection pooling (to be verified)

### Monitoring & Observability
- ✅ Docker health checks
- ✅ GitHub Actions deployment logs
- ✅ Nginx access/error logs
- ⏳ Application logs to centralized system
- ⏳ Uptime monitoring
- ⏳ Performance monitoring

---

**Session Date**: 2025-10-11
**Next Session Goal**: Install Nginx configs, test domain access, add SSL certificates
**Repository**: https://github.com/Vesperino/PortalForge.git
**VPS**: [VPS_IP]:[SSH_PORT]
