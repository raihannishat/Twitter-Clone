namespace Application.Block.Queries;

public class GetBlockUsersQuery: IRequest<Result<List<SearchedUserViewModel>>>
{
    public PaginationQueryRequest PageQuery { get; set; }

    public GetBlockUsersQuery(PaginationQueryRequest paginationQuery) => PageQuery = paginationQuery;
}
