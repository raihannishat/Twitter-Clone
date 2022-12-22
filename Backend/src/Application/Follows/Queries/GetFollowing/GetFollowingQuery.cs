namespace Application.Follows.Queries.GetFollowing;

public class GetFollowingQuery : IRequest<Result<List<SearchedUserViewModel>>>
{
    public PaginationQueryRequest PageQuery { get; set; }

    public GetFollowingQuery(PaginationQueryRequest paginationQuery) => PageQuery = paginationQuery;
}
