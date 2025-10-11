# Nginx Configuration Templates

## portalforge-frontend.conf

This snippet should be included in `/etc/nginx/sites-available/krablab.pl` inside the main `server` block.

The GitHub Actions workflow automatically:
1. Creates a backup of the current Nginx config
2. Updates the PortalForge frontend section
3. Tests the configuration with `nginx -t`
4. Reloads Nginx if the test passes
5. Rolls back to backup if anything fails

## Manual Usage

If you need to manually apply this configuration:

```bash
# Backup current config
sudo cp /etc/nginx/sites-available/krablab.pl /etc/nginx/sites-available/krablab.pl.backup

# Edit the config
sudo nano /etc/nginx/sites-available/krablab.pl

# Test configuration
sudo nginx -t

# If OK, reload
sudo systemctl reload nginx

# If something breaks, restore backup
sudo cp /etc/nginx/sites-available/krablab.pl.backup /etc/nginx/sites-available/krablab.pl
sudo systemctl reload nginx
```

## Current Setup

The frontend location block should be placed between the backend API location and the main Krablab location:

```nginx
server {
    # ... SSL and other configs ...

    # ===== PORTALFORGE BACKEND API =====
    location /portalforge/be { ... }

    # ===== PORTALFORGE FRONTEND ===== (insert here)
    # Content from portalforge-frontend.conf

    # ===== Krablab main web =====
    location / { ... }
}
```
