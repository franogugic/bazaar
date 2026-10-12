# Bazaar: Target Data Map

## Stores

| Store | What it holds | SoR or derived | OLTP or OLAP | Source and how it is rebuilt |
|---|---|---|---|---|
| Primary database | profiles, listings, inventory, orders, wallet ledger | system of record | OLTP | source: user input (source of truth, not built from another store); if lost, it is restored from the replica or from a backup |
| Object storage | listing images | system of record | OLTP | source: user input (source of truth); if lost, it is restored from a backup |
| Standby replica | copy of the primary database | derived | OLTP | source: primary database; rebuilt with a new base backup, then replication continues |
| Backups | backup of the primary database (snapshot + WAL) | derived | OLTP | source: primary database; rebuilt with a new snapshot (old states cannot be recreated) |
| Event log | domain events (listing created, order placed, payment captured) | derived | OLTP | source: changes in the primary database (outbox, later CDC); rebuilt with a new initial snapshot of the database, then it continues; the history of old events is lost |
| Search engine | listing index (title, description, category, price) | derived | OLTP | source: listings from the primary database, through batches or events; rebuilt by replay or by indexing again |
| Feed | listings from followed sellers | derived | OLTP | source: listings and follows from the primary database, through the event log; rebuilt by replaying events |
| Cache | frequently read listings and profiles | derived | OLTP | source: primary database; rebuilt by lazy loading (it starts cold) |
| Warehouse | history of orders and listings for analytics (star schema) | derived | OLAP | source: primary database and logs; rebuilt by running the ETL again or by a backfill |
| Recommendations | similar listings (listing → similar listings) | derived | OLTP | source: warehouse; rebuilt by running the batch job again |
| Logs | application messages (errors, request durations) | system of record | OLAP | source: the application writes them (source of truth, not built from another store) |

## Data flow

```
                                 USER / CLIENT
                                       │
                                       ▼
                              APPLICATION (.NET)
                                       │
         ┌─────────────────────────────┼───────────────────────────────────┐
         ▼                             ▼                                   ▼
  OBJECT STORAGE               PRIMARY DATABASE                          LOGS
(system of record)            (system of record)                  (system of record)
                                       │                                   │
         ┌──────────────┬──────────────┼──────────────┬────────────────────┤
         ▼              ▼              ▼              ▼                    ▼
  STANDBY REPLICA    BACKUPS       EVENT LOG        CACHE              WAREHOUSE
     (derived)      (derived)      (derived)      (derived)            (derived)
                                       │                                   │
                             ┌─────────┴─────────┐                         │
                             ▼                   ▼                         ▼
                       SEARCH ENGINE           FEED                 RECOMMENDATIONS
                         (derived)           (derived)                 (derived)
```

### Arrows

- Application → object storage, primary database, logs (reads and writes).
- Primary database → standby replica (replication).
- Primary database → backups (snapshot + WAL).
- Primary database → event log (outbox, later CDC).
- Primary database → cache (lazy loading).
- Primary database → warehouse (ETL).
- Event log → search engine, feed.
- Logs → warehouse.
- Warehouse → recommendations (batch job).
