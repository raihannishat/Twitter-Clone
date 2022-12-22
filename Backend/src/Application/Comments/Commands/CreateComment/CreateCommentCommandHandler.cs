using Application.Comments.Shared.Interfaces;
using Application.Notifications.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<CommentResponse>>
{
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITweetRepository _tweetRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<CreateCommentCommandHandler> _logger;

    public CreateCommentCommandHandler(IMapper mapper,
        ICurrentUserService currentUserService,
        ITweetRepository tweetRepository,
        INotificationRepository notificationRepository,
        ICommentRepository commentRepository,
        ILogger<CreateCommentCommandHandler> logger)
    {
        _mapper = mapper;
        _currentUserService = currentUserService;
        _tweetRepository = tweetRepository;
        _notificationRepository = notificationRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<Result<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        //pass tweetOwner Id to send notification
        var tweet = await _tweetRepository.FindByIdAsync(request.TweetId);

        if (tweet == null)
        {
            return null!;
        }

        var comment = new Comment
        {
            TweetId = tweet.Id,
            UserId = _currentUserService.UserId,
            Content = request.Content,
            CreatedTime = DateTime.UtcNow,
        };

        await _commentRepository.InsertOneAsync(comment);

        var notification = new Notification
        {
            TweetId = tweet.Id,
            UserId = tweet.UserId!,
            ActionedUserId = _currentUserService.UserId,
            Action = "Commented",
            TweetType = "Tweet",
            Time = DateTime.Now
        };

        tweet.Comments++;

        await _tweetRepository.ReplaceOneAsync(tweet);

        if (tweet.UserId != _currentUserService.UserId)
        {
            await _notificationRepository.InsertOneAsync(notification);
        }

        var commentResponse = new CommentResponse()
        {
            TotalComments = tweet.Comments
        };

        return Result<CommentResponse>.Success(commentResponse);

    }
}
