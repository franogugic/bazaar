# ADR-002: Start on one PostgreSQL node

## Status

Accepted

## Context

We need to decide how many database nodes Bazaar uses at the start.

The server from ADR-001 costs about 10 USD per month and has 4 GB of RAM and 40 GB of disk.

Size estimates (these numbers are assumptions):

- A listing is about 5 KB. A user profile is about 3 KB.
- 100,000 users are about 0.3 GB.
- 5 million listings are about 24 GB.
- Together this is about 70% of the disk. These are quite large numbers for the start of a project.
- The working set is the listings and profiles that are used often. We assume it is about 1 GB (around 200,000 items). This is about 25% of the RAM and fits in the buffer pool.

## Options considered

### Option 1: One database node with a standby and a backup

Pros:

- The cheapest option.
- The simplest to run and to understand.

Cons:

- The capacity is limited to this one node.

### Option 2: Primary and read replicas

Pros:

- More capacity for reads.

Cons:

- More expensive.
- More complex, because data is replicated to other databases.

### Option 3: Sharded database

Pros:

- The highest scalability.

Cons:

- The highest complexity.

## Decision

We start with one PostgreSQL node, with a standby and a backup.

With the numbers above, one node is enough. The other options would be unnecessary extra cost and complexity.

## Consequences

Easier:

- The system is simple and cheap.
- We can test network failures, delays and similar problems on a small setup.

Harder:

- Moving to sharding later is harder.
- Scaling is harder, because the capacity is limited to one node.

## DDIA reference

- Chapter 1, "Distributed Versus Single-Node Systems": reasons to distribute a system and what it costs.
- Chapter 7, Sharding: pros and cons of sharding, and when to avoid it.

## Revisit when

- The database is larger than 32 GB (80% of the 40 GB disk), or
- the traffic is higher than 100 requests per second.
