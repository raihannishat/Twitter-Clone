using Application.Notifications.Shared.Models;

namespace Application.Notifications.Queries;

public class GetNotificationQuery : IRequest<Result<List<NotificationViewModel>>>
{
    public PaginationQueryRequest PageQuery { get; set; }

    public GetNotificationQuery(PaginationQueryRequest paginationQuery) => PageQuery = paginationQuery;
}
