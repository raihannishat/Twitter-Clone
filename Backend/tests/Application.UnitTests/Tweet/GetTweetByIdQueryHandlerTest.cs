using System.Linq.Expressions;

namespace Application.UnitTests.Tweet;

public class GetTweetByIdQueryHandlerTest
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<IRetweetRepository> _retweetRepositoryMock;
    private readonly Mock<ILogger<GetTweetByIdQueryHandler>> _loggerMock;
    private readonly GetTweetByIdQueryHandler _sut;

    public GetTweetByIdQueryHandlerTest()
    {
        _currentUserServiceMock = new();
        _tweetRepositoryMock = new();
        _userRepositoryMock = new();
        _likeRepositoryMock = new();
        _retweetRepositoryMock = new();
        _loggerMock = new();

        _sut = new GetTweetByIdQueryHandler(
            _currentUserServiceMock.Object,
            _tweetRepositoryMock.Object,
            _userRepositoryMock.Object,
            _likeRepositoryMock.Object,
            _retweetRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetTweetByIdQueryResponseShouldBeNull_WhenTweetDoesNotExist()
    {
        // Arrange
        var tweetId = "C5EC84AB-4581-40F5-A6C2-346D44F8106C";
        var tweetOwnerId = "0A9C63E4-98B4-47E6-A17C-660D36BD032E";

        var command = new GetTweetByIdQuery(tweetId, tweetOwnerId);

        var cts = new CancellationTokenSource();

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => null!)
            .Verifiable();

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTweetByIdQueryResponseShouldBeTrue_WhenLikedByCurrentUserIsExist()
    {
        // Arrange
        var tweetId = "51130074-86D4-4210-A311-5C06519E2FBA";
        var tweetOwnerId = "76137641-BDCC-4F99-92F5-22DAEECFEAE0";
        var currentUserId = "57581096-1B4A-4D93-9761-967F439C9CAD";

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var tweetOwner = new User
        {
            Id = tweetOwnerId
        };

        var tweetCreator = new User
        {
            Id = currentUserId
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

        var command = new GetTweetByIdQuery(tweetId, tweetOwnerId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet)
            .Verifiable();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetOwnerId))
            .ReturnsAsync(tweetOwner);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(tweetCreator);

        _likeRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))))
            .ReturnsAsync(likeResult);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsLikedByCurrentUser;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetTweetByIdQueryResponseShouldBeTrue_WhenRetweetedByCurrentUserIsExist()
    {
        // Arrange
        var tweetId = "92AE773E-9BF9-490F-AFBA-74722D097721";
        var tweetOwnerId = "A7B449CB-8451-43E4-AAF8-0A68EB1AECA4";
        var currentUserId = "8ED1ED77-9712-4009-8EA2-D7BE52F4D777";

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var tweetOwner = new User
        {
            Id = tweetOwnerId
        };

        var tweetCreator = new User
        {
            Id = currentUserId
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

        var command = new GetTweetByIdQuery(tweetId, tweetOwnerId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet)
            .Verifiable();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetOwnerId))
            .ReturnsAsync(tweetOwner);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(tweetCreator);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(retweet))))
            .ReturnsAsync(retweetResult);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetTweetByIdQueryResponseShouldBeTrue_WhenRetweetedIsExist()
    {
        // Arrange
        var tweetId = "92AE773E-9BF9-490F-AFBA-74722D097721";
        var tweetOwnerId = "A7B449CB-8451-43E4-AAF8-0A68EB1AECA4";
        var currentUserId = "8ED1ED77-9712-4009-8EA2-D7BE52F4D777";

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var tweetOwner = new User
        {
            Id = tweetOwnerId
        };

        var tweetCreator = new User
        {
            Id = currentUserId
        };

        var retweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = tweetOwnerId
        };

        var retweetResult = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = tweetOwnerId
        };

        var command = new GetTweetByIdQuery(tweetId, tweetOwnerId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet)
            .Verifiable();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweetOwnerId))
            .ReturnsAsync(tweetOwner);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(tweetCreator);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(retweet))))
            .ReturnsAsync(retweetResult);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweeted;

        // Assert
        result.Should().BeTrue();
    }
}
