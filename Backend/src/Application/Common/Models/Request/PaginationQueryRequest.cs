namespace Application.Common.Models.Request;

public class PaginationQueryRequest
{
    public string? UserId { get; set; }

    public string? Keyword { get; set; }

    const int maxPageSize = 50;
    public int PageNumber { get; set; } = 1;
    
    private int _pageSize = 5;

    public int PageSize
    {
        get { return _pageSize; }

        set
        {
            _pageSize = value > maxPageSize ? maxPageSize : value;
        }
    }
}
