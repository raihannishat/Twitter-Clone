using Application.Tweets.Queries.GetUserTimelineTweets;
using System.Linq.Expressions;

namespace Application.UnitTests.Tweet;

public class GetUserTimelineTweetsQueryHandlerTest
{
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserTimelineRepository> _userTimelineRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<IRetweetRepository> _retweetRepositoryMock;
    private readonly Mock<IBlockRepository> _blockRepositoryMock;
    private readonly Mock<ILogger<GetUserTimelineTweetsQueryHandler>> _loggerMock;
    private readonly GetUserTimelineTweetsQueryHandler _sut;

    public GetUserTimelineTweetsQueryHandlerTest()
    {
        _tweetRepositoryMock = new();
        _currentUserServiceMock = new();
        _mapperMock = new();
        _userTimelineRepositoryMock = new();
        _userRepositoryMock = new();
        _likeRepositoryMock = new();
        _retweetRepositoryMock = new();
        _blockRepositoryMock = new();
        _loggerMock = new();

        _sut = new GetUserTimelineTweetsQueryHandler(
            _tweetRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object,
            _userTimelineRepositoryMock.Object,
            _userRepositoryMock.Object,
            _likeRepositoryMock.Object,
            _retweetRepositoryMock.Object,
            _blockRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldHaveNoItem_WhenUserTimelineDoseNotHaveAnyItem()
    {
        // Arrange
        var userId = "D79465D4-2CAC-46DF-875F-EB77F1A65B7E";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>();

        var queryRequest = new PaginationQueryRequest();

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(userTimelineList);
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldcontinue_WhenCurrentUserBlockedAndTweetCreatorBlockedDoesNotExsist()
    {
        // Arrange
        var userId = "6324E6EB-7494-41BE-811B-E314BF90C9A7";
        var currentUserId = "9009AE55-3B89-4069-8D15-A8B1100F1FE9";
        var tweetId = "26023F25-BA83-4C2A-ADED-3A3028E4ABB0";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweetViewModel = new List<TweetViewModel>();

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModel);
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldAddUserTimeline_WhenCurrentUserBlockedAndTweetCreatorBlockedDoesNotExsist()
    {
        // Arrange
        var userId = "6324E6EB-7494-41BE-811B-E314BF90C9A7";
        var currentUserId = "9009AE55-3B89-4069-8D15-A8B1100F1FE9";
        var tweetId = "26023F25-BA83-4C2A-ADED-3A3028E4ABB0";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = userId
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = userId
        };

        var creatorBlock = new Blocks
        {
            BlockedId = userId,
            BlockedById = currentUserId
        };

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(creatorBlock))))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.
            Value!.FirstOrDefault()!.UserId;

        // Assert
        result.Should().BeEquivalentTo(userId);
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldContinue_WhenTweetDoesNotExsist()
    {
        // Arrange
        var userId = "D6FD653A-E9EA-45BB-9118-4A2D522EAC54";
        var tweetId = "BE18C77C-1CD4-4BC2-A8BD-A2ECF1F751E3";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweetList = new List<TweetViewModel>();

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetList);
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldContinue_WhenTweetCreatorDoesNotExsist()
    {
        // Arrange
        var userId = "14BA81AD-8974-46C1-B796-B66C9DBC6F73";
        var tweetId = "9D094918-00F4-4263-B104-F6171733B1ED";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweetObj = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = userId
        };

        var tweetList = new List<TweetViewModel>();

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweetObj);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetObj.UserId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetList);
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldBeTrue_WhenLikedByCurrentUserIsExsist()
    {
        // Arrange
        var userId = "C7DEA3BE-4F66-46C9-8356-91A0E5CCAFDB";
        var currentUserId = "23340717-F7D9-4DF6-A2C6-70BF57D582BA";
        var tweetId = "49CFC69C-AF5B-4EEB-8C7F-86AE15284FE5";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweetObj = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = userId
        };

        var tweetCreator = new User
        {
            Id = userId
        };

        var like = new Domain.Entities.Like
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var likeResult = new Domain.Entities.Like
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweetObj);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetObj.UserId))
            .ReturnsAsync(tweetCreator);

        _likeRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))))
            .ReturnsAsync(likeResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.IsLikedByCurrentUser;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldBeTrue_WhenRetweetedByCurrentUserIsExsist()
    {
        // Arrange
        var userId = "C7DEA3BE-4F66-46C9-8356-91A0E5CCAFDB";
        var currentUserId = "BF0F8DBE-8157-43B1-BDD7-B72D6CB54141";
        var tweetId = "49CFC69C-AF5B-4EEB-8C7F-86AE15284FE5";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweetObj = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = userId
        };

        var tweetCreator = new User
        {
            Id = userId
        };

        var retweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var retweetResult = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweetObj);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetObj.UserId))
            .ReturnsAsync(tweetCreator);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(retweet))))
            .ReturnsAsync(retweetResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldBeTrue_WhenRetweetedIsExsist()
    {
        // Arrange
        var userId = "4DBEFD12-95AE-4462-B13A-0EE88C4FE18D";
        var currentUserId = "C81941FD-8E3A-481C-B43E-1ED16712698E";
        var tweetId = "77DBEB27-35CD-4D27-B713-885B8B74676F";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweetObj = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = userId
        };

        var tweetCreator = new User
        {
            Id = userId
        };

        var retweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = userId
        };

        var retweetResult = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = userId
        };

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweetObj);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetObj.UserId))
            .ReturnsAsync(tweetCreator);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(retweet))))
            .ReturnsAsync(retweetResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.IsRetweeted;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetUserTimelineTweetsQueryResponseShouldBeTrue_WhenTweetUserIdAndCurrentUserIdIsEqual()
    {
        // Arrange
        var userId = "6E254624-9118-4626-AB68-C3CD68B927EF";
        var currentUserId = "0499DBC1-1CAF-4B26-862A-D9B83E56898E";
        var tweetId = "B42CDBFD-B86B-4800-84B4-40728637A90C";
        var pageNumber = 1;

        var user = new User
        {
            Id = userId
        };

        var userTimeline = new UserTimeline
        {
            UserId = userId
        };

        var userTimelineList = new List<UserTimeline>
        {
            new UserTimeline
            {
                UserId = userId,
                TweetId = tweetId
            }
        };

        var queryRequest = new PaginationQueryRequest
        {
            UserId = userId
        };

        var tweetObj = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var tweetCreator = new User
        {
            Id = userId
        };

        var retweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = userId
        };

        var query = new GetUserTimelineTweetsQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline)), pageNumber))
            .Returns(userTimelineList);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweetObj);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetObj.UserId))
            .ReturnsAsync(tweetCreator);

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.CanDelete;

        // Assert
        result.Should().BeTrue();
    }
}
