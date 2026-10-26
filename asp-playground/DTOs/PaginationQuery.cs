using System.ComponentModel.DataAnnotations;

namespace asp_playground.DTOs;

/// <summary>Query-string parameters for paged list endpoints.</summary>
public class PaginationQuery
{
    public const int MaxPageSize = 100;

    /// <summary>1-based page number.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
    public int Page { get; set; } = 1;

    /// <summary>Number of items per page (1 to 100).</summary>
    [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;
}
