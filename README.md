# AppSec Demo Platform

A multi-technology security demonstration platform designed for Linux hosts with Dynatrace OneAgent. The first milestone includes a portal landing page and a .NET demo app with dynamic plugin execution and taint-flow scenarios.

## Quick start (Docker Compose)

```bash
docker compose up --build
```

Then open:
- Portal: `http://<server>/`
- .NET demo: `http://<server>/dotnet`

## GitHub repo workflow (simple pull + build)

If you want a GitHub repo that you can `git pull` on your Linux host and run immediately, nothing else is required beyond the Dockerfiles already in this repo.

On the Linux host:

```bash
git pull
docker compose up --build
```

This builds images locally from the Dockerfiles and keeps everything on a single Docker network (`appsec-demo`).

## Optional: prebuilt images in GHCR

If you want the Linux host (or teammates) to avoid local builds, publish images to GHCR and pull them instead.

1) Set GHCR variables (locally or in CI):

```bash
export GHCR_OWNER=your-org
export IMAGE_TAG=latest
```

You can also copy `.env.example` to `.env` and set the values there.

2) Pull and run the images:

```bash
docker compose -f docker-compose.yml -f docker-compose.ghcr.yml pull
docker compose -f docker-compose.yml -f docker-compose.ghcr.yml up --no-build
```

Notes:
- `docker-compose.ghcr.yml` assumes you have published images for **all** services it lists (including WAF and third‑party labs). If you only publish the in‑repo services, remove or edit the third‑party entries in that file.
- Third-party labs (Juice Shop, WebGoat) are pulled from Docker Hub by default in `docker-compose.yml`.
- All services are already attached to the same Docker network (`appsec-demo`), so they can reach each other by service name.

### Attacks catalog (data-backed)

The portal "Attacks" tab is driven by `portal/data/attacks.json`. Update the list of attack patterns, variants, and stack availability there to change the UI and lookup catalog without touching code.

### WAF (ModSecurity + CRS)

Enable the WAF profile to insert a ModSecurity + CRS proxy in front of the demo app:

```bash
docker compose --profile waf up --build
```

Then open:
- WAF-protected .NET demo: `http://<server>/waf/`

Notes:
- The WAF container is configured as a reverse proxy and rewrites `/waf/*` to the .NET app's `/dotnet/*`.
- Tune rules and obfuscation tests by editing `services/waf/conf.d/appsec-demo.conf` and the WAF environment variables in `docker-compose.yml`.

### Optional third‑party labs

Enable the optional profile when you want to add third‑party apps:

```bash
docker compose --profile thirdparty up --build
```

Notes:
- Some third‑party apps assume they are mounted at `/` and may require additional path‑base configuration to work cleanly under a prefix.
- If you hit routing issues, run them on dedicated ports or adjust Traefik rules per app.

## Local .NET dev (without Docker)

From `services/dotnet-demo`:

```bash
dotnet run --project src/AppSecDotnetDemo
```

The app expects plugins in `services/dotnet-demo/assemblies` by default.

## Structure

```
portal/                  # Landing page (static)
services/dotnet-demo/    # .NET demo app + plugins
services/waf/            # WAF reverse-proxy config (ModSecurity + CRS)
```

## SCA/SAST/DAST hooks (docs-only)

Use the demo environment as a scan target by pointing tools at the Docker Compose host and collecting output per app. A common flow is:

1. Run SCA/SAST/DAST tools against the same services in `docker-compose.yml` (and any additional apps on the host).
2. Export findings to a shared results directory or artifact store.
3. Compare scan totals vs Dynatrace runtime findings to highlight prioritization (what is actually exercised in production).

This repo intentionally avoids coupling to a specific scanner. Add your preferred tools or CI steps, then map them to Dynatrace service names for the comparison story.

## Safety

These apps are intentionally vulnerable and should only be deployed to isolated demo environments.
