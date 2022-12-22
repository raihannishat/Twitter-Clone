using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Tweets.Commands.DeleteTweet;

public class DeleteTweetCommandHandler : IRequestHandler<DeleteTweetCommand, Result<Unit>>
{
    private readonly ITweetRepository _tweetRepository;
    private readonly IUserTimelineRepository _userTimelineRepository;
    private readonly ILogger<DeleteTweetCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public DeleteTweetCommandHandler(IMapper mapper,
        ICurrentUserService currentUserService,
        ITweetRepository tweetRepository,
        IUserTimelineRepository userTimelineRepository,
        ILogger<DeleteTweetCommandHandler> logger)
    {
        _mapper = mapper;
        _currentUserService = currentUserService;
        _tweetRepository = tweetRepository;
        _userTimelineRepository = userTimelineRepository;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteTweetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var tweet = await _tweetRepository.FindByIdAsync(request.Id);

        if (tweet == null || tweet.UserId != _currentUserService.UserId)
        {
            return null!;
        }

        await _tweetRepository.DeleteByIdAsync(tweet.Id);

        await _userTimelineRepository
            .DeleteOneAsync(x => x.TweetId == tweet.Id && x.UserId == userId);

        return Result<Unit>.Success(Unit.Value);
    }
}
