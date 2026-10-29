using LibraryManagement.Api.RequestParameters.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.RequestParameters;

// the "struct" is necessary because a default value of an Enum is zero, but we want it to be null
public record SortingParameters<TSortBy> where TSortBy : struct, Enum
{
    [FromQuery(Name = "sortBy")]
    public TSortBy? SortBy { get; init; }

    [FromQuery(Name = "sortDirection")] 
    public SortDirections SortDirection { get; init; } = SortDirections.Asc;
}