namespace Application.IdentityManagement.Admin.Queries;

public class GetAllUsersQuery : IRequest<Result<List<UserViewModel>>>
{
    public PaginationQueryRequest PageQuery { get; set; }

    public GetAllUsersQuery(PaginationQueryRequest paginationQuery) => PageQuery = paginationQuery;
}
