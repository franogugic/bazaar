# ADR-001: Self-hosted server instead of managed cloud

## Status

Accepted

## Context

Bazaar follows the book *Designing Data-Intensive Applications*. Its only purpose is learning and practicing the ideas from the book.

We need to decide where Bazaar runs. A good single server with a backup and a replica costs about 10 USD per month. A comparable managed cloud setup costs about 20 USD per month. These numbers are estimates.

The most important force is not cost. It is that the project must let us test failures on purpose, for example network failures and delays.

## Options considered

### Option 1: Managed cloud

Pros:

- Simple to start.
- Scales easily and can add or remove resources when needed (elasticity).
- Fault tolerance, high availability and monitoring are included.
- The provider runs the infrastructure for us.

Cons:

- We have little control over the system.
- If the provider does not offer a service, we have to wait.
- Moving to another provider is painful (lock-in).
- It is hard to find the cause of a problem, because we cannot see the infrastructure.

### Option 2: Self-hosted server

Pros:

- Much more control. We get the resources and manage the rest ourselves.
- We can create failures on purpose, such as network failures and delays.

Cons:

- We are responsible for everything else.
- We pay for resources all the time, even when they are not used.

## Decision

We use a self-hosted server.

Control is what lets us create failures such as network failures, and we need this for the learning goals of the project.

## Consequences

Easier:

- Testing failures on purpose (network failures, delays, killing processes).

Harder:

- We have to set up monitoring, logs and metrics ourselves.
- We have to make backups ourselves.
- We have to run the database replica ourselves.

## DDIA reference

Chapter 1, "Cloud Versus Self-Hosting" (pros and cons of cloud services, cloud-native system architecture, operations in the cloud era). Concepts applied: cost, control, operational work, lock-in.

## Revisit when

Bazaar moves from a learning project to a real application with more than 1000 users.
