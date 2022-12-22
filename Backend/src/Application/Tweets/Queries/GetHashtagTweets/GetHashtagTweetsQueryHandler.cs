using Application.Block.Shared.Interfaces;
using Application.Likes.Shared.Interfaces;
using Application.Retweets.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Application.Tweets.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Tweets.Queries.GetHashtagTweets;

public class GetHashtagTweetsQueryHandler : IRequestHandler<GetHashtagTweetsQuery, Result<List<TweetViewModel>>>
{
    private readonly IMapper _mapper;
    private readonly ITweetRepository _tweetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBlockRepository _blockRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IRetweetRepository _retweetRepository;
    private readonly ILogger<GetHashtagTweetsQueryHandler> _logger;

    public GetHashtagTweetsQueryHandler(
        IMapper mapper,
        ITweetRepository tweetRepository,
        ICurrentUserService currentUserService,
        IBlockRepository blockRepository,
        IHashtagRepository hashtagRepository,
        IUserRepository userRepository,
        ILikeRepository likeRepository,
        IRetweetRepository retweetRepository,
        ILogger<GetHashtagTweetsQueryHandler> logger)
    {
        _mapper = mapper;
        _tweetRepository = tweetRepository;
        _currentUserService = currentUserService;
        _blockRepository = blockRepository;
        _hashtagRepository = hashtagRepository;
        _userRepository = userRepository;
        _likeRepository = likeRepository;
        _retweetRepository = retweetRepository;
        _logger = logger;
    }

    public async Task<Result<List<TweetViewModel>>> Handle(GetHashtagTweetsQuery request, CancellationToken cancellationToken)
    {
        var hashTagName = request.PageQuery.Keyword;

        if (hashTagName == string.Empty || hashTagName == null)
        {
            return null!;
        }

        var hashtags = _hashtagRepository
            .GetHashtagTweetByDescendingTime(x => x.TagName == hashTagName, request.PageQuery.PageNumber);

        if (!hashtags.Any())
        {
            return Result<List<TweetViewModel>>.Success(new List<TweetViewModel>());
        }

        var hashtagTweets = new List<Hashtag>();

        foreach (var hashtag in hashtags)
        {
            var currentUserIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == _currentUserService.UserId && x.BlockedById == hashtag.UserId);

            var userIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedById == _currentUserService.UserId && x.BlockedId == hashtag.UserId);

            if (currentUserIsBlocked == null && userIsBlocked == null)
            {
                hashtagTweets.Add(hashtag);
            }
        }

        var tweetCollection = _tweetRepository.GetCollection();

        var userCollection = _userRepository.GetCollection();

        var tweetList = from hashtag in hashtagTweets
                        join tweet in tweetCollection on hashtag.TweetId equals tweet.Id
                        select tweet;

        var tweets = from tweet in tweetList
                     join user in userCollection on tweet.UserId equals user.Id
                     select new TweetViewModel
                     {
                         Id = tweet.Id,
                         Content = tweet.Content,
                         CreatedAt = tweet.CreatedAt,
                         Likes = tweet.Likes,
                         Retweets = tweet.Retweets,
                         Comments = tweet.Comments,
                         UserId = user.Id,
                         UserName = user.Name,
                         Image = user.Image,
                         Edited = tweet.Edited,
                         TweetCreatorId = user.Id,
                         TweetCreatorName = user.Name,
                         TweetCreatorImage = user.Image,
                     };

        var tweetVMList = new List<TweetViewModel>();

        foreach (var tweet in tweets)
        {
            var tweetViewModel = _mapper.Map<TweetViewModel>(tweet);

            var isLikedByCurrentUser = await _likeRepository.FindOneByMatchAsync(x =>
                x.TweetId == tweet.Id && x.UserId == _currentUserService.UserId);

            var isRetweetedByCurrentUser = await _retweetRepository.FindOneByMatchAsync(x =>
                x.TweetId == tweet.Id && x.UserId == _currentUserService.UserId);

            if (isLikedByCurrentUser != null)
            {
                tweet.IsLikedByCurrentUser = true;
            }

            if (isRetweetedByCurrentUser != null)
            {
                tweet.IsRetweetedByCurrentUser = true;
            }

            tweetVMList.Add(tweet);
        }

        return Result<List<TweetViewModel>>.Success(tweetVMList.ToList());
    }
}
