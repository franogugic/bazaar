# Bazaar: Personal Data Inventory

This document lists every personal field that Bazaar collects. For each field it shows why we need it, how long we keep it, and which stores it reaches.

## General rules

- **Backups are not edited.** Deleted data leaves the backups when they expire. The backup retention is 30 days.
- **The warehouse, logs and event log use `user_id` instead of the username, email or name.** After an account is deleted, the `user_id` points to nobody, so the records can no longer be linked to a person.
- **Passwords are never stored.** Only the password hash is stored, and it never goes to logs or to the event log.

## Account

| Field | Purpose | Retention | Stores |
|---|---|---|---|
| Username | Unique identifier of the user. | While the account exists. After that it is deleted everywhere; from backups it leaves when they expire. | Primary database, replica, backups, cache, search engine, feed. The warehouse, logs and event log keep only the `user_id`. |
| Country | Lets buyers filter listings by country. | While the account exists. After that it is deleted. In the warehouse the PII (the link to `user_id`) is removed. From backups it leaves when they expire. | Primary database, replica, backups, search engine, cache, warehouse. |
| City | Lets buyers filter listings by city. | While the account exists. After that it is deleted. In the warehouse the PII (the link to `user_id`) is removed. From backups it leaves when they expire. | Primary database, replica, backups, search engine, cache, warehouse. |
| Email | Login and password recovery. | While the account exists. From backups it leaves when they expire. | Primary database, replica, backups. |
| Password hash | Login. | While the account exists. From backups it leaves when they expire. | Primary database, replica, backups. |

## Wallet

| Field | Purpose | Retention | Stores |
|---|---|---|---|
| Transactions (ledger entries) | Show the user their transactions and keep the ledger balanced. | While the account exists. After deletion the personal data is removed, but the entries stay (amount, date, pair), so the ledger stays balanced. | Primary database, replica, backups, warehouse (anonymous). |
| Balance | Know how much money the user has. It is derived from the transactions. | While the account exists. Its retention depends on the retention of the transactions. | Primary database, replica, backups. |

## Messages

| Field | Purpose | Retention | Stores |
|---|---|---|---|
| Messages | Communication between users. | The whole conversation is deleted when either participant deletes the account, or after one year without activity. | Primary database, replica, backups. |

## Security

| Field | Purpose | Retention | Stores |
|---|---|---|---|
| IP address | Monitoring, fraud detection, finding many accounts from the same address. | 30 days. | Logs. |
| Device ID | Monitoring, fraud detection, finding many accounts on the same device. | 30 days. | Logs. |

## Listings

| Field | Purpose | Retention | Stores |
|---|---|---|---|
| Title, description, images | The core of the app: showing items for sale. | While the user exists. When a user is deleted, all their listings and everything related to them are deleted. From backups it leaves when they expire. | Primary database, object storage (images), replica, backups, search engine, feed, cache. The warehouse gets only metadata (no descriptions and no images). Logs and event log get only the `listing_id`. |

## Social

| Field | Purpose | Retention | Stores |
|---|---|---|---|
| Follows (who follows whom) | The feed shows listings from the sellers a user follows. | While the user exists or until they unfollow. When a seller deletes the account, the follows of all their followers are deleted too. | Primary database, replica, backups, feed, cache, event log (only IDs). |

## Fields removed from the plan

| Field | Reason |
|---|---|
| First name and last name | The username is enough as an identity. |
| Phone number | Users contact each other through messages inside Bazaar. |
| Exact address | Delivery is out of scope. A city is enough. |

## Open questions

- **Event log and IP address or device ID:** fraud signals in M12 may read the IP address from events. We decide this in Chapter 12.
