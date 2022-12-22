using Application.Likes.Shared.Interfaces;
using Application.Likes.Shared.Models;
using Application.Notifications.Shared.Interfaces;
using Application.Retweets.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Likes.Commands;

public class LikeCommandHandler : IRequestHandler<LikeCommand, Result<LikeResponse>>
{
    private readonly ILikeRepository _likeRepository;
    private readonly ITweetRepository _tweetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IRetweetRepository _retweetRepository;
    private readonly ILogger<LikeCommandHandler> _logger;
    private readonly IUserRepository _userRepository;

    public LikeCommandHandler(ILikeRepository likeRepository,
        ICurrentUserService currentUserService,
        ITweetRepository tweetRepository,
        INotificationRepository notificationRepository,
        IRetweetRepository retweetRepository,
        ILogger<LikeCommandHandler> logger,
        IUserRepository userRepository)
    {
        _likeRepository = likeRepository;
        _currentUserService = currentUserService;
        _tweetRepository = tweetRepository;
        _notificationRepository = notificationRepository;
        _retweetRepository = retweetRepository;
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<Result<LikeResponse>> Handle(LikeCommand request, CancellationToken cancellationToken)
    {
        //pass tweetOwnerId also if send notifcation to tweetOwner not the tweetCreator

        var userId = _currentUserService.UserId;

        var tweetObj = await _tweetRepository.FindByIdAsync(request.TweetId!);

        if (tweetObj == null)
        {
            return null!;
        }

        var likeObject = await _likeRepository.FindOneByMatchAsync(x =>
            x.UserId == _currentUserService.UserId && x.TweetId == request.TweetId);

        var likeResponse = new LikeResponse();

        if (likeObject == null)
        {
            var likeEntity = new Like
            {
                UserId = _currentUserService.UserId,
                TweetId = tweetObj.Id
            };

            await _likeRepository.InsertOneAsync(likeEntity);

            var notification = new Notification
            {
                TweetId = request.TweetId!,
                UserId = tweetObj.UserId!,
                ActionedUserId = userId,
                Action = "Liked",
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

            tweetObj.Likes++;

            likeResponse.IsLikedByCurrentUser = true;
        }
        else
        {
            await _likeRepository.DeleteOneAsync(x => x.TweetId == request.TweetId && x.UserId == userId);

            tweetObj.Likes--;
        }

        await _tweetRepository.ReplaceOneAsync(tweetObj);

        likeResponse.Likes = tweetObj.Likes;

        return Result<LikeResponse>.Success(likeResponse);
    }
}
