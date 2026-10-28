# Pagination in the Donation Management API — Research Report

**Project:** `asp-playground/w5-t1` (ASP.NET Core, EF Core, SQL Server LocalDB)
**Project link:** [GitHub](https://github.com/SaifKhalifa/asp-playground/tree/w5-t1/asp-playground)
**Current state:** `GET /api/donations` uses page-based pagination with a total count.
**Recommendation Based On Ai Review:** migrate the list endpoint to keyset (cursor) pagination on `(CreatedAt DESC, Id DESC)`, and keep an optional count endpoint.

## 0. Where the project stands today

| Concern | Location | What it does |
|---|---|---|
| Endpoint | `Controllers/DonationsController.cs:20-27` | `GET api/donations`, binds `[FromQuery] PaginationQuery`, returns `PagedResponse<DonationResponse>` |
| Request DTO | `DTOs/PaginationQuery.cs` | `Page` (>= 1), `PageSize` (1–100, default 10, `MaxPageSize = 100`) |
| Response DTO | `DTOs/PagedResponse.cs` | `Items`, `Page`, `PageSize`, `TotalCount`, `TotalPages` |
| Query | `Services/DonationService.cs:17-45` | `CountAsync()` then `OrderByDescending(CreatedAt).ThenByDescending(Id).Skip().Take()` |
| Entity | `Entities/Donation.cs` | `Id` (int identity), `Amount`, `DonorName`, `IsAnonymous`, `CreatedAt` |
| Index | `Data/AppDbContext.cs:20` | `HasIndex(d => d.CreatedAt)` |
| Anonymity rule | `DTOs/DonationResponse.cs:9-18` | `DonationResponse.From` nulls `DonorName` when `IsAnonymous` |

Two things the existing code already gets right and the new design must keep:

- **Deterministic order.** `ThenByDescending(d => d.Id)` (`DonationService.cs:26`) is the tie-breaker, so rows with the same `CreatedAt` never swap places.
- **Bounded page size.** `MaxPageSize = 100` stops clients from requesting the whole table.

---

## Part 1 — Research & Comparison

### 1.1 Offset-based pagination

**Mechanism.** `GET /api/donations?offset=40&limit=20`. In this project it would be:

```csharp
_db.Donations.AsNoTracking()
    .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.Id)
    .Skip(40).Take(20);
```
```sql
SELECT ... FROM Donations
ORDER BY CreatedAt DESC, Id DESC
OFFSET 40 ROWS FETCH NEXT 20 ROWS ONLY;
```

| Aspect | Assessment |
|---|---|
| Pros | Simplest; stateless; random access to any offset. |
| Cons | Inserts/deletes between requests shift the window, causing **duplicates or skipped rows**. |
| Performance | Cost is O(offset + limit): SQL Server walks the `CreatedAt` index and discards `offset` rows. Page 1 is fast, a page at row 5,000,000 reads 5,000,000 index entries. |
| Fits | Small or rarely changing tables. |

### 1.2 Cursor / keyset pagination

**Mechanism.** `GET /api/donations?limit=20&cursor=<opaque>`; the cursor carries the last row's `(CreatedAt, Id)`.

```csharp
query.Where(d => d.CreatedAt < c.CreatedAt || (d.CreatedAt == c.CreatedAt && d.Id < c.Id))
     .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.Id)
     .Take(limit + 1);
```
```sql
SELECT TOP (21) ... FROM Donations
WHERE CreatedAt < @c OR (CreatedAt = @c AND Id < @id)
ORDER BY CreatedAt DESC, Id DESC;
```

| Aspect | Assessment |
|---|---|
| Pros | Stable under concurrent writes; O(limit) regardless of depth; no `COUNT`. |
| Cons | No jump-to-page-N; no total without a second query; cursor must be opaque; backward paging needs a reversed query. |
| Performance | Index seek on `(CreatedAt, Id)` then read `limit` rows. Latency is flat at 10 rows or 100 million. |
| Fits | Feeds, infinite scroll, exports, sync clients, large append-heavy tables. |

### 1.3 Page-based with total count (what the project does now)

**Mechanism.** `GET /api/donations?page=3&pageSize=20` — exactly `DonationService.GetAllAsync`:

```csharp
int totalCount = await _db.Donations.CountAsync();              // DonationService.cs:19
int skip = (query.Page - 1) * query.PageSize;                   // :21
... .Skip(skip).Take(query.PageSize).ToListAsync();             // :27-29
```
```sql
SELECT COUNT(*) FROM Donations;
SELECT ... ORDER BY CreatedAt DESC, Id DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
```

| Aspect | Assessment |
|---|---|
| Pros | Best UX for numbered pagers and "page X of Y" (`TotalPages`, `PagedResponse.cs:10`). Easy for front-ends. |
| Cons | All offset drawbacks, plus an extra `COUNT(*)` on every request, and the count and page are two separate statements (the count can already be stale when the page runs). |
| Performance | `COUNT(*)` scans the narrowest index, O(rows). Once filters are added (date range, minimum amount) the count must repeat them and scales with matching rows. Deep pages are O(offset). |
| Fits | Back-office grids on bounded data. |

### 1.4 Behaviour under real-time changes

`POST /api/donations` (`DonationsController.cs:47-54`) sets `CreatedAt = DateTime.UtcNow` (`DonationService.cs:68`), so **every new donation lands at the head of the newest-first list**. That is the worst case for offset: a user on page 1 who waits while 5 donations arrive and then clicks "next" sees the last 5 rows of page 1 again. `DELETE` (`:94-107`) causes the opposite, skipping a row. With keyset, page 2 starts strictly after the last row seen, so neither happens.

### 1.5 Query plan comparison on `Donations`

| | Offset / page-based | Keyset |
|---|---|---|
| Plan | Ordered index scan, discard `offset` rows, Top | Index **Seek** on anchor, read `limit` rows |
| Rows read | `offset + limit` (+ full `COUNT` scan) | `limit` |
| Page 1 vs very deep page | ms vs seconds | ms vs ms |

Index note: `HasIndex(d => d.CreatedAt)` is a non-clustered index, and SQL Server silently appends the clustering key (`Id`) to it, so it is effectively `(CreatedAt, Id)`. A backward scan of that index already yields `CreatedAt DESC, Id DESC`, so the current query avoids a sort. What the index does **not** contain is `Amount`, `DonorName` and `IsAnonymous`, so each returned row needs a key lookup; see 3.6.

---

## Part 2 — Choice: keyset pagination

### 2.1 Architectural justification

1. **The access pattern is "newest first, always growing".** Every create inserts at the head of the sort order (`CreatedAt = UtcNow`), the exact condition under which offset drifts.
2. **Flat cost as data grows.** The current `Skip`/`CountAsync` cost grows with the table; keyset cost depends only on page size.
3. **No extra scan.** `CountAsync()` at `DonationService.cs:19` disappears from the hot path; `hasNext` comes from fetching `limit + 1` rows.
4. **The index and tie-breaker already exist.** `IX_Donations_CreatedAt` plus `ThenByDescending(Id)` is already the right shape, so the change is mostly query and contract, not schema.

### 2.2 Trade-offs and mitigations

| Limitation | Mitigation |
|---|---|
| No jump to page N | Filter navigation by date range (`from`/`to` on `CreatedAt`) is itself a keyset anchor. |
| `TotalCount`/`TotalPages` go away | Add `GET /api/donations/count` (same filters), called lazily by the UI; cap or estimate for huge ranges. |
| Breaking contract change | Ship as `GET /api/donations` with `cursor`/`limit`; keep `page`/`pageSize` temporarily via a versioned route or deprecate with a date. |
| Tampered cursor | Base64url JSON, strictly validated; return `400` (existing `ValidationProblemDetails`/`ProblemDetails` conventions). |
| Backward paging | Return `prevCursor`, run the reversed comparison and ordering, reverse the list in memory. |
| Anonymity must hold | Keep going through `DonationResponse.From` for every item, and never put `DonorName` in the cursor. |

### 2.3 When to choose something else

- **Page-based with total** — if a back-office screen with numbered pages and "N results" becomes a hard requirement over a bounded/filtered set (keep the current implementation for that screen).
- **Plain offset** — tiny reference lists that rarely change.
- **Hybrid** — keyset for public/mobile and exports, page-based for the admin grid with a capped count.

---

## Part 3 — Implementation plan (against this codebase)

### 3.1 Request DTO — replaces `DTOs/PaginationQuery.cs`

Reuses the existing conventions (`DataAnnotations`, `MaxPageSize`, auto-400 from `[ApiController]`).

```csharp
using System.ComponentModel.DataAnnotations;

namespace asp_playground.DTOs;

public class DonationQuery : IValidatableObject
{
    public const int MaxLimit = 100;

    [Range(1, MaxLimit, ErrorMessage = "Limit must be between 1 and 100.")]
    public int Limit { get; set; } = 10;

    /// <summary>Opaque cursor returned by the previous page.</summary>
    public string? Cursor { get; set; }

    public DonationSort SortBy { get; set; } = DonationSort.CreatedAt;
    public SortDirection Direction { get; set; } = SortDirection.Desc;

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "MinAmount must be positive.")]
    public decimal? MinAmount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (From > To)
            yield return new ValidationResult("From must be earlier than or equal to To.", [nameof(From), nameof(To)]);
    }
}

public enum DonationSort { CreatedAt, Amount }
public enum SortDirection { Asc, Desc }
```

`MinAmount > 0` mirrors the DB constraint `CK_Donations_Amount_Positive` (`AppDbContext.cs:14`). Sorting is an enum, so no raw column name from the client ever reaches the query.

### 3.2 Response wrapper — replaces `DTOs/PagedResponse.cs`

```csharp
public class CursorPage<T>
{
    public required List<T> Items { get; init; }
    public int Limit { get; init; }
    public bool HasNext { get; init; }
    public bool HasPrevious { get; init; }
    public string? NextCursor { get; init; }
    public string? PrevCursor { get; init; }
}
```

```json
{ "items": [ { "id": 981, "amount": 25.00, "donorName": null, "isAnonymous": true, "createdAt": "2026-10-28T09:14:02Z" } ],
  "limit": 10, "hasNext": true, "hasPrevious": false,
  "nextCursor": "eyJzIjowLCJkIjowLCJjIjoiMjAyNi0xMC0yOFQwOToxNDowMloiLCJpIjo5ODF9", "prevCursor": null }
```

### 3.3 Cursor codec — new `DTOs/DonationCursor.cs`

```csharp
public record DonationCursor(DonationSort Sort, SortDirection Dir, DateTime CreatedAt, decimal Amount, int Id)
{
    public string Encode() => WebEncoders.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(this));

    public static DonationCursor? TryDecode(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        try { return JsonSerializer.Deserialize<DonationCursor>(WebEncoders.Base64UrlDecode(raw)); }
        catch (Exception ex) when (ex is JsonException or FormatException) { return null; }
    }
}
```

### 3.4 Controller — changes to `GetAll` in `Controllers/DonationsController.cs`

Same route, same `[ProducesResponseType]` style:

```csharp
/// <summary>Get a page of donations using a cursor (newest first by default).</summary>
[HttpGet]
[ProducesResponseType<CursorPage<DonationResponse>>(StatusCodes.Status200OK)]
[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<CursorPage<DonationResponse>>> GetAll([FromQuery] DonationQuery query)
{
    DonationCursor? cursor = DonationCursor.TryDecode(query.Cursor);
    if (query.Cursor != null && (cursor == null || cursor.Sort != query.SortBy || cursor.Dir != query.Direction))
    {
        return Problem("Invalid or mismatched cursor.", statusCode: StatusCodes.Status400BadRequest);
    }

    return Ok(await _service.GetAllAsync(query, cursor));
}

/// <summary>Count donations matching the filters.</summary>
[HttpGet("count")]
public async Task<ActionResult<int>> Count([FromQuery] DonationQuery query) =>
    Ok(await _service.CountAsync(query));
```

`IDonationService.GetAllAsync` gains the `DonationCursor?` parameter and a `CountAsync(DonationQuery)` member (`Services/IDonationService.cs:7`).

### 3.5 EF Core query — replaces `DonationService.GetAllAsync`

Filtering, keyset predicate, deterministic order (always `Id` last), one round trip, and the same `DonationResponse.From` mapping so the anonymity rule is untouched.

```csharp
public async Task<CursorPage<DonationResponse>> GetAllAsync(DonationQuery query, DonationCursor? cursor)
{
    IQueryable<Donation> q = ApplyFilters(_db.Donations.AsNoTracking(), query);
    bool desc = query.Direction == SortDirection.Desc;

    if (cursor != null)
    {
        q = (query.SortBy, desc) switch
        {
            (DonationSort.CreatedAt, true)  => q.Where(d => d.CreatedAt < cursor.CreatedAt || (d.CreatedAt == cursor.CreatedAt && d.Id < cursor.Id)),
            (DonationSort.CreatedAt, false) => q.Where(d => d.CreatedAt > cursor.CreatedAt || (d.CreatedAt == cursor.CreatedAt && d.Id > cursor.Id)),
            (DonationSort.Amount, true)     => q.Where(d => d.Amount < cursor.Amount || (d.Amount == cursor.Amount && d.Id < cursor.Id)),
            _                               => q.Where(d => d.Amount > cursor.Amount || (d.Amount == cursor.Amount && d.Id > cursor.Id)),
        };
    }

    q = (query.SortBy, desc) switch
    {
        (DonationSort.CreatedAt, true)  => q.OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.Id),
        (DonationSort.CreatedAt, false) => q.OrderBy(d => d.CreatedAt).ThenBy(d => d.Id),
        (DonationSort.Amount, true)     => q.OrderByDescending(d => d.Amount).ThenByDescending(d => d.Id),
        _                               => q.OrderBy(d => d.Amount).ThenBy(d => d.Id),
    };

    List<Donation> rows = await q.Take(query.Limit + 1).ToListAsync();   // +1 => HasNext without COUNT

    bool hasNext = rows.Count > query.Limit;
    if (hasNext) rows.RemoveAt(rows.Count - 1);

    List<DonationResponse> items = rows.Select(DonationResponse.From).ToList();
    Donation? last = rows.LastOrDefault();

    return new CursorPage<DonationResponse>
    {
        Items = items,
        Limit = query.Limit,
        HasNext = hasNext,
        HasPrevious = cursor != null,
        NextCursor = hasNext && last != null
            ? new DonationCursor(query.SortBy, query.Direction, last.CreatedAt, last.Amount, last.Id).Encode()
            : null
    };
}

public Task<int> CountAsync(DonationQuery query) =>
    ApplyFilters(_db.Donations.AsNoTracking(), query).CountAsync();

private static IQueryable<Donation> ApplyFilters(IQueryable<Donation> q, DonationQuery f)
{
    if (f.From is { } from) q = q.Where(d => d.CreatedAt >= from);
    if (f.To is { } to) q = q.Where(d => d.CreatedAt <= to);
    if (f.MinAmount is { } min) q = q.Where(d => d.Amount >= min);
    return q;
}
```

(`PrevCursor` uses the same code with the reversed comparison and ordering, then reverses `rows`.)

**N+1.** `Donation` has no navigation properties, and the page is one `SELECT` with no `Include`, so there is no N+1 today. If a `Donor` or `Campaign` entity is added later, project the needed columns in the same query rather than lazy-loading per row.

### 3.6 Index changes — `Data/AppDbContext.cs:20`

Make the covering index explicit and add the amount-sort index, then add a migration (`Data/Migrations`):

```csharp
e.HasIndex(d => new { d.CreatedAt, d.Id })
 .IsDescending(true, true)
 .IncludeProperties(d => new { d.Amount, d.DonorName, d.IsAnonymous })
 .HasDatabaseName("IX_Donations_CreatedAt_Id");

e.HasIndex(d => new { d.Amount, d.Id })
 .IncludeProperties(d => new { d.CreatedAt, d.DonorName, d.IsAnonymous })
 .HasDatabaseName("IX_Donations_Amount_Id");
```

With `INCLUDE`, the list query is a covering index seek with no key lookups. Check the actual plan: expect `Index Seek` + `Top`, and `SET STATISTICS IO ON` logical reads that stay the same for page 1 and a deep page. Trade-off: more index storage and slightly slower inserts; acceptable for a read-heavy list.

### 3.7 Test checklist

1. Create donations between two page requests: no duplicates or gaps (currently reproducible with `Skip`).
2. Several donations with identical `CreatedAt`: order stays stable via `Id`.
3. Anonymous donations: `donorName` stays `null` in every page.
4. Tampered cursor, cursor from a different `sortBy`, `limit=0`, `limit=101`: all `400`.
5. Seed ~5M rows; compare logical reads and duration of page 1 vs. a deep page for the old and new endpoints.

## Conclusion

The current page-based implementation is correct and a good fit for small data, but it pays for a full `COUNT(*)` on every request and drifts whenever a donation is created, which in this API is on every write. Keyset pagination on `(CreatedAt, Id)` removes both costs using an index the project already has, at the price of numbered pages and a free total, which a separate count endpoint and date-range filters cover.
