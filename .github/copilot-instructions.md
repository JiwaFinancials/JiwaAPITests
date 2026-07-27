# Copilot Instructions

## General

- Follow existing project conventions.
- Before creating tests, scan the test project and reuse:
  - naming conventions
  - folder structure
  - helper methods
  - fixtures
  - builders
  - assertions
  - base classes
- Do not create duplicate tests.
- Use DTOs from the dtos.cs project.
- Do not create duplicate DTOs.

## Route Source

- Do not invent routes.
- Do not skip routes.
- Every route must be evaluated and assigned to exactly one test class.

## Exclusions

Do not generate tests for:
- /metadata
- metadata services
- metadata DTOs
- customer fields
- custom field values

## Route Grouping

Determine the test class from the route structure:

- First route segment = folder.
- Root resource routes = singular resource class.
- Child resources = child resource class.
- Route parameters, actions and HTTP verbs do not affect grouping.

Examples:

- /Customers/{CustomerID} → Customers/Customer.cs
- /Customers/{CustomerID}/Addresses → Customers/Addresses.cs
- /Jobs/{JobID} → Jobs/Job.cs
- /Jobs/{JobID}/Lines → Jobs/Lines.cs
- /CreditorPurchases/{CreditorPurchaseID} → CreditorPurchases/CreditorPurchase.cs
- /CreditorPurchases/{CreditorPurchaseID}/Lines → CreditorPurchases/Lines.cs

## Test Generation

Create missing tests only.

For CRUD resources, cover:
- Create
- Read
- Update
- Delete

Where supported, also cover:
- Activate
- Deactivate
- Approve
- Close
- Reopen
- Archive
- Restore

## Test Design

Prefer end-to-end lifecycle tests:

- Create
- Read
- Update
- Verify update
- Delete
- Verify deletion

For child resources:

- Create parent
- Create child
- Read child
- Update child
- Verify update
- Delete child
- Verify deletion

## Comments

Add comments before major operations.

Examples:

// Create resource
// Read resource
// Update resource
// Verify update
// Delete resource
// Verify deletion

## Output

Before generating code:
- Show route count.
- Show route-to-class mapping.
- Show planned folder structure.
- Show skipped routes with reason.

After generation:
- Verify every route is assigned once.
- Verify no excluded routes were included.
- Verify no duplicate tests were created.
- Show final coverage summary.
- Run only the tests that were generated.
