using System.Linq.Expressions;

namespace Application.UnitTests.Like;

public class LikeCommandHandlerTest
{
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IRetweetRepository> _retweetRepositoryMock;
    private readonly Mock<ILogger<LikeCommandHandler>> _loggerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LikeCommandHandler _sut;

    public LikeCommandHandlerTest()
    {
        _likeRepositoryMock = new();
        _tweetRepositoryMock = new();
        _currentUserServiceMock = new();
        _notificationRepositoryMock = new();
        _retweetRepositoryMock = new();
        _loggerMock = new();
        _userRepositoryMock = new();

        _sut = new LikeCommandHandler(
            _likeRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _tweetRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _retweetRepositoryMock.Object,
            _loggerMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public void LikeResponseShouldBeNull_WhenTweetDoesNotExist()
    {
        // Arrange
        var tweeetId = "F308BE61-2192-456A-969A-B55AA83B4160";

        var command = new LikeCommand
        {
            TweetId = tweeetId
        };

        var cts = new CancellationTokenSource();

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweeetId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void LikeResponseShouldBeTrue_WhenLikeDoesNotExist()
    {
        // Arrange
        var tweeetId = "F308BE61-2192-456A-969A-B55AA83B4160";
        var curentUserId = "1F5C2D05-A242-4928-99CF-C6DE367C43C1";

        var command = new LikeCommand
        {
            TweetId = tweeetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweeetId
        };

        var like = new Domain.Entities.Like
        {
            UserId = curentUserId,
            TweetId = tweeetId
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(curentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweeetId))
            .ReturnsAsync(tweet);

        _likeRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.Value!
            .IsLikedByCurrentUser;

        // Assert
        result.Should().BeTrue();

        _likeRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Domain.Entities.Like>()), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(It.IsAny<Domain.Entities.Tweet>()), Times.Once);
    }

    [Fact]
    public void LikeResponseShouldInsertNotification_WhenTweetUserIdAndCurrentUserIdIsNotEqual()
    {
        // Arrange
        var tweeetId = "CE7FC3D5-81E1-4220-BE81-0A79C281D7BB";
        var curentUserId = "E64E22B3-16DD-494B-95F4-4F1E9A53815F";
        var tweetUserId = "6D75C8A3-1DDE-4CC6-8018-4B47D3C8513D";

        var command = new LikeCommand
        {
            TweetId = tweeetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweeetId,
            UserId = tweetUserId
        };

        var like = new Domain.Entities.Like
        {
            UserId = curentUserId,
            TweetId = tweeetId
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(curentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweeetId))
            .ReturnsAsync(tweet);

        _likeRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.Value!
            .IsLikedByCurrentUser;

        // Assert
        result.Should().BeTrue();

        _likeRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Domain.Entities.Like>()), Times.Once);

        _notificationRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Notification>()), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(It.IsAny<Domain.Entities.Tweet>()), Times.Once);
    }

    [Fact]
    public void LikeResponseShouldBeFalse_WhenLikeIsExist()
    {
        // Arrange
        var tweeetId = "91E7C069-7600-46CE-9A84-865743DBFAB0";
        var userId = "0A72795B-7FD8-436A-9060-972995B6636C";

        var command = new LikeCommand
        {
            TweetId = tweeetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweeetId,
            Likes = 1
        };

        var likeResult = new Domain.Entities.Like
        {
            UserId = userId,
            TweetId = tweeetId
        };

        var like = new Domain.Entities.Like
        {
            UserId = userId,
            TweetId = tweeetId
        };

        var likeDelete = new Domain.Entities.Like
        {
            UserId = userId,
            TweetId = tweeetId
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(userId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweeetId))
            .ReturnsAsync(tweet);

        _likeRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))))
            .ReturnsAsync(likeResult);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.Value!
            .IsLikedByCurrentUser;

        // Assert
        result.Should().BeFalse();

        _likeRepositoryMock.Verify(x =>
            x.DeleteOneAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(likeDelete))));

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(It.IsAny<Domain.Entities.Tweet>()), Times.Once);
    }
}
