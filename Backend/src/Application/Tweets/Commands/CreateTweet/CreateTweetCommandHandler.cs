using Application.Follows.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Application.Tweets.Commands.CreateTweet;

public class CreateTweetCommandHandler : IRequestHandler<CreateTweetCommand, Result<Unit>>
{
    private readonly ITweetRepository _tweetRepository;
    private readonly IUserTimelineRepository _userTimelineRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly ISearchRepository _searchRepository;
    private readonly ILogger<CreateTweetCommandHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFollowRepository _followRepository;
    private readonly IHomeTimelineRepository _homeTimelineRepository;

    public CreateTweetCommandHandler(ITweetRepository tweetRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IUserTimelineRepository userTimelineRepository,
        IHashtagRepository hashtagRepository,
        ISearchRepository searchRepository,
        ILogger<CreateTweetCommandHandler> logger,
        IFollowRepository followRepository,
        IHomeTimelineRepository homeTimelineRepository)
    {
        _tweetRepository = tweetRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userTimelineRepository = userTimelineRepository;
        _hashtagRepository = hashtagRepository;
        _searchRepository = searchRepository;
        _logger = logger;
        _followRepository = followRepository;
        _homeTimelineRepository = homeTimelineRepository;
    }

    public async Task<Result<Unit>> Handle(CreateTweetCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        //----------------------
        //MongoDB
        //----------------------

        var entity = new Tweet
        {
            UserId = currentUserId,
            Content = request.Content,
            CreatedAt = DateTime.Now,
        };

        await _tweetRepository.InsertOneAsync(entity);

        await InsertUserTimeline(entity);

        var hashtags = await ExtractHashTag(request.Content);

        if (hashtags.Count > 0)
        {
            await InsertHashtag(hashtags, entity);

            await InsertHashtagInSearchTable(hashtags);
        }

        var followerList = await _followRepository.GetFollowers(x => x.FollowedId == currentUserId);

        var followerIdList = followerList.Select(x => x.FollowerId).ToList();

        await InsertFollowerHomeTimline(followerIdList, entity, currentUserId);

        //await _tweetPublisher.SendPostToQueue(currentUserId, entity);

        //await _tweetConsumer.Connect(currentUserId);

        return Result<Unit>.Success(Unit.Value);
    }

    private async Task<List<string>> ExtractHashTag(string content)
    {
        var regex = new Regex(@"#\w+");

        var matches = regex.Matches(content);

        var hashtags = new HashSet<string>();

        if (matches.Count == 0)
        {
            return new List<string>();
        }

        foreach (var match in matches)
        {
            hashtags.Add(match.ToString()!.ToLower());
        }

        return hashtags.ToList();
    }

    private async Task InsertUserTimeline(Tweet tweet)
    {
        var timeline = new UserTimeline
        {
            TweetId = tweet.Id,
            UserId = _currentUserService.UserId,
            CreatedAt = tweet.CreatedAt
        };

        await _userTimelineRepository.InsertOneAsync(timeline);
    }

    private async Task InsertHashtag(List<string> hashtags, Tweet tweet)
    {
        foreach (var tag in hashtags)
        {
            var tagEntity = new Hashtag
            {
                TagName = tag,
                TweetId = tweet.Id,
                UserId = _currentUserService.UserId,
                CreatedAt = tweet.CreatedAt
            };

            await _hashtagRepository.InsertOneAsync(tagEntity);
        }
    }

    private async Task InsertHashtagInSearchTable(List<string> hashtags)
    {
        foreach (var tag in hashtags)
        {
            var tagExist = await _searchRepository
                .FindOneByMatchAsync(x => x.HashTag == tag);

            if (tagExist == null)
            {
                var tagEntity = new Search
                {
                    HashTag = tag
                };

                await _searchRepository.InsertOneAsync(tagEntity);
            }
        }
    }

    private async Task InsertFollowerHomeTimline(List<string> followersId, Tweet tweet, string currentUserId)
    {
        foreach (var followerId in followersId)
        {
            var homeTimeline = new HomeTimeline
            {
                UserId = followerId,
                TweetOwnerId = currentUserId,
                TweetId = tweet.Id,
                CreatedAt = tweet.CreatedAt
            };

            _logger.LogInformation(currentUserId + "  post tweet --->");

            _logger.LogInformation(followerId + "  got tweet --->");

            await _homeTimelineRepository.InsertOneAsync(homeTimeline);
        }

    }
}
