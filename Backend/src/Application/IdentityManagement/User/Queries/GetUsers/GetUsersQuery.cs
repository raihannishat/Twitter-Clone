namespace Application.IdentityManagement.User.Queries.GetUsers;

public class GetUsersQuery : IRequest<Result<List<SearchedUserViewModel>>>
{
    public PaginationQueryRequest PageQuery { get; set; }

    public GetUsersQuery(PaginationQueryRequest paginationQuery) => PageQuery = paginationQuery;
}
