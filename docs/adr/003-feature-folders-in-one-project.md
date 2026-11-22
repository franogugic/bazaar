# ADR-003: Feature folders in one project

## Status

Accepted

## Context

We need to decide how to organize the code of the Bazaar API for the first milestone.

The first milestone has a small scope: sign up, create and view a listing, follow a seller, and a home feed. The data is stored in one PostgreSQL node (ADR-002). We use plain SQL migrations and Dapper, so there is no ORM layer between the code and the database.

## Options considered

### Option 1: Feature folders in one project

Each feature has its own folder (for example `Features/Users`). Each use case is one file (for example `SignUp.cs`) that holds the request and response types, the minimal API endpoint, and the data access code.

Pros:

- Simple. A change to one feature touches one folder.
- Less code, because there are no layers that only pass calls on.
- Easy to move to another structure later, because only files move.

Cons:

- The same logic can be copied between features if we are not careful.
- It is less familiar to people who expect layers.

### Option 2: Layers by type in one project

Folders `Controllers`, `Services` and `Repositories`. A request goes from the controller to the service to the repository.

Pros:

- A common and well-known structure.
- Clear separation: HTTP, logic, and SQL are in different places.

Cons:

- One feature is spread over several folders.
- For a small scope, some layers would only forward calls.

### Option 3: Clean Architecture with separate projects

Separate projects for domain, application, infrastructure, and API.

Pros:

- Strong separation, easy to test the logic without the database.

Cons:

- The most files and the most setup.
- Too much structure for the size of the first milestone.

## Decision

We use feature folders in one project (Option 1), with minimal API endpoints instead of controllers. Each use case is one file.

For the current task this is the best fit, and we did not want to make the code more complicated than the problem needs. The cost of changing the structure later is low, so we do not need to guess the final shape now.

## Consequences

Easier:

- Adding or changing a feature touches one folder.
- The code is shorter and easier to read.

Harder:

- We must watch for duplicated logic between features.
- The structure is less standard, so it needs this explanation.

## Revisit when

- The same SQL or the same logic is repeated in more than one feature, or
- we need to test the logic without a database, or
- more people work on the code and need clearer layer boundaries.
