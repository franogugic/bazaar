# ADR-004: JWT access token without a refresh token

## Status

Accepted

## Context

Users must prove who they are after login, so that endpoints like `/me` know which user sends the request.

The first milestone is small: one API, one PostgreSQL node (ADR-002), no frontend yet. We want the simplest approach that works and that we can explain.

## Options considered

### Option 1: JWT access token only

Login checks the password and returns a signed JWT with the user id (`sub`) and an expiry time. The server checks only the signature and the expiry on each request. It does not store anything.

Pros:

- Simple. No new table, no migration, no extra database query per request.
- Easy to run on more than one API node later, because every node only needs the signing key.

Cons:

- The server cannot revoke a token before it expires. Logout and blocking a user work only after the token expires.
- When the token expires, the user must log in again.

### Option 2: Opaque session token stored in the database

Login creates a random token and stores its hash in the database. Every request looks the token up.

Pros:

- Logout and blocking work immediately.

Cons:

- One extra database query on every request, on the same node that holds all the data.
- It is a session, not a JWT. Combining it with a JWT would remove the reason to use a JWT.

### Option 3: Short JWT access token plus a refresh token

The access token is checked without the database. A long-lived refresh token, stored hashed in the database, gives a new access token.

Pros:

- Short access token and a long login time together, with revocation through the refresh token.

Cons:

- The most work: a table, rotation of refresh tokens, and a refresh endpoint.
- Too much for the size of the first milestone.

## Decision

We use a JWT access token only (Option 1).

- The token is signed with HS256. The key comes from the configuration (`Jwt__Key`, in `.env`), must be at least 32 characters, and is checked when the application starts.
- The token lives for 1 hour.
- It contains `sub` (user id) and `email`. It is signed, not encrypted, so we never put secrets in it.
- We do not validate issuer and audience, because one service issues and checks the tokens.
- The token is returned in the body of the login response. The client keeps it and sends it in the `Authorization: Bearer` header.

## Consequences

Easier:

- No state on the server and no database query to check a token.
- No extra table.

Harder:

- A token cannot be revoked before it expires (up to 1 hour). The same is true after a password change or when a user is blocked.
- Users log in again every hour.
- The signing key is a secret that must not leak. Anyone with the key can issue a token for any user. Changing the key logs everyone out.

## Revisit when

- Logging in every hour bothers users, or the mobile client needs a long session: add a refresh token (Option 3).
- We need to block a user or log out immediately: add a refresh token, or a list of revoked tokens.
- More than one service issues or checks tokens: switch to an asymmetric key and validate issuer and audience.
