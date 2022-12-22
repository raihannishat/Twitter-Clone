using System.Linq.Expressions;

namespace Application.UnitTests.Retweet;

public class RetweetCommandHandlerTest
{
    private readonly Mock<IRetweetRepository> _retweetRepositoryMock;
    private readonly Mock<IUserTimelineRepository> _userTimelineRepositoryMock;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<ILogger<RetweetCommandHandler>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RetweetCommandHandler _sut;

    public RetweetCommandHandlerTest()
    {
        _retweetRepositoryMock = new();
        _userTimelineRepositoryMock = new();
        _tweetRepositoryMock = new();
        _currentUserServiceMock = new();
        _notificationRepositoryMock = new();
        _loggerMock = new();
        _mapperMock = new();

        _sut = new RetweetCommandHandler(
            _retweetRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _tweetRepositoryMock.Object,
            _userTimelineRepositoryMock.Object,
            _mapperMock.Object,
            _notificationRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void RetweetResponseShouldBeNull_WhenTweetDoesNotExist()
    {
        // Arrange
        var tweetId = "A8F6A51C-3475-4A7B-A5CE-8E3D8B2277E0";

        var command = new RetweetCommand
        {
            TweetId = tweetId
        };

        var cts = new CancellationTokenSource();

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RetweetResponseShouldBeTrue_WhenReTweetDoesNotExist()
    {
        // Arrange
        var tweetId = "ED3C4562-EFC6-44E7-9F8B-881D5C0812DB";
        var userId = "12EDD332-34B2-45CE-9C8F-FCD7DF33104D";

        var command = new RetweetCommand
        {
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = userId
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(userId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeTrue();

        _retweetRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Domain.Entities.Retweet>()), Times.Once());

        _userTimelineRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<UserTimeline>()), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(tweet), Times.Once);
    }

    [Fact]
    public void RetweetResponseShouldInsertNotification_WhenTweetUserIdAndCurrentUserIdIsNotEqual()
    {
        // Arrange
        var tweetId = "9A7D99DA-3DE8-4482-9372-2EE1EBAC24DA";
        var currentUserId = "6B473F56-5178-4DFC-9C18-50682CBF0712";
        var tweetUserId = "392F26A8-B178-4A38-A7A8-DE99EA7FD21E";

        var command = new RetweetCommand
        {
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = tweetUserId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeTrue();

        _retweetRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Domain.Entities.Retweet>()), Times.Once());

        _notificationRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Notification>()), Times.Once);

        _userTimelineRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<UserTimeline>()), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(tweet), Times.Once);
    }

    [Fact]
    public void RetweetResponseShouldDeleteUserTimeline_WhenTweetUserIdAndCurrentUserIdIsEqual()
    {
        // Arrange
        var tweetId = "9A7D99DA-3DE8-4482-9372-2EE1EBAC24DA";
        var currentUserId = "6B473F56-5178-4DFC-9C18-50682CBF0712";
        // var tweetUserId = "392F26A8-B178-4A38-A7A8-DE99EA7FD21E";

        var command = new RetweetCommand
        {
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var userTimeline = new UserTimeline
        {
            UserId = currentUserId,
            TweetId = tweetId,
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeTrue();

        _retweetRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Domain.Entities.Retweet>()), Times.Once());

        _userTimelineRepositoryMock.Verify(x =>
            x.DeleteOneAsync(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline))), Times.Once);

        _userTimelineRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<UserTimeline>()), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(tweet), Times.Once);
    }

    [Fact]
    public void RetweetResponseShouldBeFalse_WhenReTweetIsExist()
    {
        // Arrange
        var tweetId = "E73A950C-8E16-4300-B4A4-5FDE4DB77FD7";
        var userId = "8E48AAE7-9D05-42FA-8F06-EE9D9527A17F";

        var command = new RetweetCommand
        {
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = userId
        };

        var deleteReTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = userId
        };

        var reTweetResult = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = userId
        };

        var retweetResponse = new RetweetResponse
        {
            IsRetweetedByCurrentUser = false,
            Retweets = -1
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(userId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(reTweetResult);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeFalse();

        _retweetRepositoryMock.Verify(x =>
            x.DeleteOneAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(deleteReTweet))), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(tweet), Times.Once);
    }

    [Fact]
    public void RetweetResponseShouldReplaceUserTimeline_WhenReTweetUserIdAndCurrentUserIdIsEqual()
    {
        // Arrange
        var tweetId = "E73A950C-8E16-4300-B4A4-5FDE4DB77FD7";
        var currentUserId = "8E48AAE7-9D05-42FA-8F06-EE9D9527A17F";

        var command = new RetweetCommand
        {
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var deleteReTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var reTweetResult = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var userTimeline = new UserTimeline
        {
            UserId = currentUserId,
            TweetId = tweetId
        };

        var userTimelineResult = new UserTimeline
        {
            UserId = currentUserId,
            TweetId = tweetId
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(reTweetResult);

        _userTimelineRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline))))
            .ReturnsAsync(userTimelineResult);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeFalse();

        _retweetRepositoryMock.Verify(x =>
            x.DeleteOneAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(deleteReTweet))), Times.Once);

        _userTimelineRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(userTimelineResult), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(tweet), Times.Once);
    }

    [Fact]
    public void RetweetResponseShouldDeleteUserTimeline_WhenReTweetUserIdAndCurrentUserIdIsNotEqual()
    {
        // Arrange
        var tweetId = "E73A950C-8E16-4300-B4A4-5FDE4DB77FD7";
        var tweetUserId = "F20D59AA-B8A4-4224-BC03-0309887C12D4";
        var currentUserId = "8E48AAE7-9D05-42FA-8F06-EE9D9527A17F";

        var command = new RetweetCommand
        {
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = tweetUserId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var deleteReTweet = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var reTweetResult = new Domain.Entities.Retweet
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var userTimeline = new UserTimeline
        {
            UserId = currentUserId,
            TweetId = tweetId
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(reTweetResult);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.
            Value!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeFalse();

        _retweetRepositoryMock.Verify(x =>
            x.DeleteOneAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(deleteReTweet))), Times.Once);

        _userTimelineRepositoryMock.Verify(x =>
            x.DeleteOneAsync(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline))), Times.Once);

        _tweetRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(tweet), Times.Once);
    }
}
