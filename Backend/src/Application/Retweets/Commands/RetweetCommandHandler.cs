using Application.Notifications.Shared.Interfaces;
using Application.Retweets.Shared.Interfaces;
using Application.Retweets.Shared.Models;
using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Retweets.Commands;

public class RetweetCommandHandler : IRequestHandler<RetweetCommand, Result<RetweetResponse>>
{
    private readonly IRetweetRepository _retweetRepository;
    private readonly IUserTimelineRepository _userTimelineRepository;
    private readonly ITweetRepository _tweetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<RetweetCommandHandler> _logger;
    private readonly IMapper _mapper;

    public RetweetCommandHandler(IRetweetRepository retweetRepository,
        ICurrentUserService currentUserService,
        ITweetRepository tweetRepository,
        IUserTimelineRepository userTimelineRepository,
        IMapper mapper,
        INotificationRepository notificationRepository,
        ILogger<RetweetCommandHandler> logger)
    {
        _retweetRepository = retweetRepository;
        _currentUserService = currentUserService;
        _tweetRepository = tweetRepository;
        _userTimelineRepository = userTimelineRepository;
        _mapper = mapper;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<Result<RetweetResponse>> Handle(RetweetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var tweetObj = await _tweetRepository.FindByIdAsync(request.TweetId!);

        if (tweetObj == null)
        {
            return null!;
        }

        var retweetObject = await _retweetRepository
            .FindOneByMatchAsync(x => x.TweetId == request.TweetId && x.UserId == userId);

        var retweetResponse = new RetweetResponse();    

        if (retweetObject == null)
        {
            var retweetEntity = new Retweet
            {
                UserId = _currentUserService.UserId,
                TweetId = tweetObj.Id
            };

            await _retweetRepository.InsertOneAsync(retweetEntity);

            var notification = new Notification
            {
                TweetId = request.TweetId!,
                UserId = tweetObj.UserId!,
                ActionedUserId = userId,
                Action = "Retweeted",
                TweetType = "Tweet",
                Time = DateTime.Now
            };

            //var isRetweeted = await _retweetRepository.FindOneByMatchAsync(x =>
            //    x.TweetId == request.TweetId && x.UserId == request.TweetOwnerId);

            //if (isRetweeted != null)
            //{
            //    notification.TweetType = "Retweet";
            //}

            if (tweetObj.UserId != userId)
            {
                await _notificationRepository.InsertOneAsync(notification);
            }

            if (tweetObj.UserId == userId)
            {
                await _userTimelineRepository.DeleteOneAsync(x => x.UserId == userId && x.TweetId == tweetObj.Id);
            }

            var userTimeline = new UserTimeline
            {
                UserId = userId,
                TweetId = tweetObj.Id,
                CreatedAt = DateTime.Now
            };

            await _userTimelineRepository.InsertOneAsync(userTimeline);

            tweetObj.Retweets++;

            retweetResponse.IsRetweetedByCurrentUser = true;
        }
        else
        {
            await _retweetRepository.DeleteOneAsync(x => x.TweetId == request.TweetId && x.UserId == userId);

            if (tweetObj.UserId == userId)
            {
                var timeline = await _userTimelineRepository.FindOneByMatchAsync(x => x.UserId == userId && x.TweetId == tweetObj.Id);

                timeline.CreatedAt = tweetObj.CreatedAt;

                await _userTimelineRepository.ReplaceOneAsync(timeline);
            }
            else
            {
                await _userTimelineRepository.DeleteOneAsync(x => x.UserId == userId && x.TweetId == tweetObj.Id);
            }

            tweetObj.Retweets--;
        }

        await _tweetRepository.ReplaceOneAsync(tweetObj);

        retweetResponse.Retweets = tweetObj.Retweets;

        return Result<RetweetResponse>.Success(retweetResponse);
    }
}
