# Calculator API - Docker Setup

This project includes Docker Compose configurations for both development and production environments.

## Development Environment

### Quick Start

```bash
# Start all services (MongoDB, Redis, API)
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f api

# Stop all services
docker-compose -f docker-compose.dev.yml down
```

### Services

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **MongoDB**: localhost:27017
- **Redis**: localhost:6379

### Configuration

Development uses hardcoded configuration:
- JWT Signing Key: `94517c4ad9aea8de84c25036e2c0428ea32228553707e5e886a70839e807f12c`
- MongoDB Database: `calculator_dev`
- Redis Prefix: `calcapi:dev:`

## Test Environment

### Quick Start

```bash
# Run tests in isolated environment
docker-compose -f docker-compose.test.yml up --build --abort-on-container-exit

# Clean up test environment
docker-compose -f docker-compose.test.yml down -v
```

### Features

- **Isolated Infrastructure**: Dedicated MongoDB and Redis instances for testing
- **Configuration**: Uses `appsettings.Test.json` configuration
- **Network Isolation**: Runs on a separate `calculator-test-network`
- **Automatic Cleanup**: Tests run and containers exit automatically

### Services

- **MongoDB Test**: localhost:27019 (mapped to container 27017)
- **Redis Test**: localhost:6381 (mapped to container 6379)

## Production Environment

### Setup

**Start services:**
```bash
# Generate JWT key and start all services
docker-compose up -d

# View generated JWT key (optional)
docker run --rm -v calculator_jwt-keys:/keys alpine cat /keys/jwt.key
```

### Services

- **API**: http://localhost:5001
- **MongoDB**: localhost:27018 (username: admin, password: admin)
- **Redis**: localhost:6380 (password: admin)

### Configuration

Production features:
- **Auto-generated JWT key** (32-byte random hex)
- **MongoDB authentication** (username: admin, password: admin)
- **Redis password protection** (password: admin)
- **Swagger disabled** in production
- **Non-root user** in container

### Security Notes

⚠️ **Important:**
- Default credentials are **admin/admin** for development convenience
- **Change these in production** by editing `docker-compose.yml`
- JWT key is generated once and persisted in Docker volume
- To regenerate JWT key: `docker volume rm calculator_jwt-keys` and restart

## Useful Commands

### Development

```bash
# Rebuild and restart API only
docker-compose -f docker-compose.dev.yml up -d --build api

# Access MongoDB shell
docker exec -it calculator-mongo-dev mongosh calculator_dev

# Access Redis CLI
docker exec -it calculator-redis-dev redis-cli

# Clean up volumes (removes all data)
docker-compose -f docker-compose.dev.yml down -v
```

### Production

```bash
# View all logs
docker-compose logs -f

# Restart API only
docker-compose restart api

# Access MongoDB with authentication
docker exec -it calculator-mongo-prod mongosh -u admin -p admin

# Access Redis with password
docker exec -it calculator-redis-prod redis-cli -a admin

# Backup MongoDB
docker exec calculator-mongo-prod mongodump --out=/data/backup

# Clean up (keeps volumes)
docker-compose down

# Clean up including volumes (DELETES ALL DATA)
docker-compose down -v
```

## Troubleshooting

### API won't start
- Check logs: `docker-compose logs api`
- Ensure MongoDB and Redis are healthy: `docker-compose ps`

### JWT key issues
- View key: `docker run --rm -v calculator_jwt-keys:/keys alpine cat /keys/jwt.key`
- Regenerate: `docker volume rm calculator_jwt-keys && docker-compose up -d`

### Database connection issues
- Verify network: `docker network inspect calculator-network`
- Check service health: `docker-compose ps`
