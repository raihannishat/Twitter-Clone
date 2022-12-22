using Application.Notifications.Shared.Interfaces;
using Application.Notifications.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Notifications.Queries;

public class GetNotificationQueryHandler : IRequestHandler<GetNotificationQuery, Result<List<NotificationViewModel>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetNotificationQueryHandler> _logger;

    public GetNotificationQueryHandler(ICurrentUserService currentUserService,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        ILogger<GetNotificationQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<List<NotificationViewModel>>> Handle(GetNotificationQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var userNotifications = await _notificationRepository.GetNotifications(x =>
        x.UserId == userId, request.PageQuery.PageNumber);

        var userCollection = _userRepository.GetCollection();

        var notifications = from notification in userNotifications
                            join user in userCollection on notification.ActionedUserId equals user.Id
                            select new NotificationViewModel
                            {
                                UserId = userId,
                                TweetId = notification.TweetId,
                                Action = notification.Action,
                                ActionedUserId = user.Id,
                                ActionedUserName = user.Name,
                                ActionedUserImage = user.Image,
                                TweetType = notification.TweetType,
                                Time = notification.Time
                            };

        return Result<List<NotificationViewModel>>.Success(notifications.ToList());
    }
}
