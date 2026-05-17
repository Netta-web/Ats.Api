# Assignment Answers

---

## 1. Schema Design

### Tables

**Applications**
The core entity. Stores the candidate's name, email, cover letter, current stage, and a foreign key to the job. A unique index on `(JobId, CandidateEmail)` enforces the business rule that one candidate cannot apply to the same job twice, both at the API level (checked before write) and at the database level (enforced even under concurrent requests).

**ApplicationNotes**
Stores recruiter notes attached to an application. Foreign keys to `Applications` (with cascade delete) and `TeamMembers` (with restrict delete — a team member cannot be deleted while notes exist). An index on `ApplicationId` supports fast retrieval of all notes for a given application without a full table scan.

**StageHistory**
An append-only audit log. Each row records a single stage transition: the application it belongs to, who made the change, from/to stage values, timestamp, and a reason. Foreign keys to `Applications` (cascade) and `TeamMembers` (restrict). Indexed on `ApplicationId`. Rows are never updated or deleted.

**ApplicationScores**
One row per dimension per application, enforced by a unique index on `(ApplicationId, Dimension)`. Stores the current score, optional comment, and who last updated it. Upsert semantics: if a row exists for that dimension, it is updated in place. Foreign keys to `Applications` (cascade) and `TeamMembers` (restrict).

### Why these indexes?

- **`(JobId, CandidateEmail)` unique** — duplicate prevention without a full scan
- **`ApplicationId` on Notes, StageHistory** — recruiters query all notes/history for a single application constantly; without an index this is a sequential scan across the full table
- **`(ApplicationId, Dimension)` unique on Scores** — prevents duplicate dimension rows and supports fast point-lookups per dimension

### How `GET /applications/{id}` works

The endpoint uses EF Core's `Select()` projection inside a single LINQ query. EF Core translates this into SQL with `LEFT JOIN` clauses across `Jobs`, `ApplicationNotes → TeamMembers`, `ApplicationScores → TeamMembers`, and `StageHistories → TeamMembers`. All nested collections (Notes, Scores, StageHistory) are populated in the same query using projected sub-selects.

In practice this results in **1–2 database round trips** depending on EF Core's query splitting configuration. With the default single-query mode, it is one query with multiple JOINs. This is the endpoint most suitable for Redis caching because it is read-heavy, expensive to compute, and changes only on explicit write operations.

---

## 2. Scoring Trade-offs

### (a) Why separate endpoints per dimension?

`PUT /api/applications/{id}/scores/culture-fit` is explicit — the recruiter is setting one specific score in one deliberate action. The API uses a single action method with a `{dimension}` route parameter, mapped to the `ScoreDimension` enum via a private helper. This avoids three identical methods while keeping URL semantics clean.

The alternative — a single `PUT /scores` with the dimension in the body — is simpler but less RESTful. Resources should be addressable by URL. `scores/culture-fit` is a distinct, identifiable resource.

PUT semantics mean the operation is idempotent: calling it twice with the same value is safe and produces the same result. This is correct for scoring — a recruiter revising their score is an update, not a new event.

### (b) If we needed full score history

**Schema change required:** Add an `ApplicationScoreHistory` table:

```
ApplicationScoreHistory
  Id               GUID
  ApplicationId    FK → Applications
  Dimension        string (enum)
  Score            int
  Comment          string
  ScoredByTeamMemberId  FK → TeamMembers
  ScoredAt         datetime
```

On each upsert, before updating the current score row, insert a new row into `ApplicationScoreHistory`. The current `ApplicationScores` table becomes a "latest value" cache for efficient reads; the history table is the source of truth for the timeline.

**Endpoint changes:** The existing `PUT` endpoints do not need to change. A new read endpoint would be added: `GET /api/applications/{id}/scores/{dimension}/history`. This follows the open/closed principle — existing behavior is unchanged, new capability is added as a new resource.

---

## 3. Debugging Scenario

**"Candidate moved to Interview but still shows Screening"**

**Step 1 — Verify the PATCH request succeeded**  
Check the API logs for the `PATCH /api/applications/{id}/stage` request. Did it return `200 OK`? If it returned `400`, the transition was rejected (check the error body — most likely the application was already past Screening or the team member ID was invalid). If it returned `404`, the application ID was wrong.

**Step 2 — Query StageHistory directly**  
```sql
SELECT * FROM "StageHistories"
WHERE "ApplicationId" = '<id>'
ORDER BY "ChangedAt" DESC;
```
If there is no row with `ToStage = 'Interview'`, the write never happened. If there is a row, the database received the change.

**Step 3 — Query the Applications table**  
```sql
SELECT "CurrentStage" FROM "Applications" WHERE "Id" = '<id>';
```
If this returns `Interview` but the API returns `Screening`, the read path is serving stale data. Check whether Redis caching is active and the cache was not invalidated after the write.

**Step 4 — Check for a race condition**  
If the stage shows `Interview` briefly and then reverts, two concurrent PATCH requests may have raced. EF Core does not use optimistic concurrency on this table by default. The solution would be to add a `RowVersion` column and handle `DbUpdateConcurrencyException`.

**Step 5 — Check client-side state**  
If the database is correct and the API returns the right value, the issue is the client reading a cached previous response (browser cache, local state). A hard refresh or cache-busting request header would confirm this.

---

## 4. Self-Assessment

| Skill | Rating | Note |
|---|---|---|
| C# | — | *(fill in honestly: e.g., "Comfortable with async/await, EF Core, and DI patterns; still growing on advanced generics and performance tuning")* |
| SQL | — | *(e.g., "Confident with JOINs, indexes, and query planning; less experienced with window functions and complex aggregations")* |
| Git | — | *(e.g., "Comfortable with feature branches, rebasing, and resolving merge conflicts")* |
| REST API Design | — | *(e.g., "Strong understanding of resource modeling, status codes, and idempotency; aware of HATEOAS but don't apply it by default")* |
| Testing | — | *(e.g., "Comfortable writing integration tests with WebApplicationFactory; growing experience with unit test design and mocking strategies")* |
