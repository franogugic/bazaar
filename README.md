# Bazaar

Bazaar is a marketplace with a wallet. It is the main project of a study of
*Designing Data-Intensive Applications* (2nd edition).

## Current state

This is milestone M2.1, a minimal vertical slice on one PostgreSQL node:

- sign up and log in
- create and view a listing
- follow a seller
- a home feed of followed sellers' new listings, computed at read time

We kept this version minimal on purpose. For example, there is no unfollow
yet.

## Run

You need Docker.

```bash
docker compose up --build -d
```

Without a `.env` file, Docker Compose uses default values that are only for
local development. To use your own values, copy `.env.example` to `.env` and
change them.

The API runs on `http://localhost:8080`. The database migrations run when the
API starts.

## Endpoints

| Method | Path | Login |
|---|---|---|
| POST | `/signup` | no |
| POST | `/login` | no |
| GET | `/me` | yes |
| POST | `/listings` | yes |
| GET | `/listings/{id}` | no |
| POST | `/users/{id}/follow` | yes |
| GET | `/feeds` | yes |

For endpoints with login, send the token from `/login` in the header
`Authorization: Bearer <token>`.

## Docs

- `docs/product-brief.md`
- `docs/invariants.md`
- `docs/adr/`
