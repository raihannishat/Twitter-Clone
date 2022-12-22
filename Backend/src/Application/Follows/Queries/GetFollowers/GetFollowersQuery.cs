namespace Application.Follows.Queries.GetFollowers;

public class GetFollowersQuery : IRequest<Result<List<SearchedUserViewModel>>>
{
    public PaginationQueryRequest PageQuery { get; set; }

    public GetFollowersQuery(PaginationQueryRequest paginationQuery) => PageQuery = paginationQuery;
}
