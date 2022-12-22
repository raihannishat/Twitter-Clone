using Application.Tweets.Commands.DeleteTweet;
using System.Linq.Expressions;

namespace Application.UnitTests.Tweet;

public class DeleteTweetCommandHandlerTest
{
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<IUserTimelineRepository> _userTimelineRepositoryMock;
    private readonly Mock<ILogger<DeleteTweetCommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly DeleteTweetCommandHandler _sut;

    public DeleteTweetCommandHandlerTest()
    {
        _tweetRepositoryMock = new();
        _userTimelineRepositoryMock = new();
        _loggerMock = new();
        _currentUserServiceMock = new();
        _mapperMock = new();

        _sut = new DeleteTweetCommandHandler(
            _mapperMock.Object,
            _currentUserServiceMock.Object,
            _tweetRepositoryMock.Object,
            _userTimelineRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void DeleteTweetCommandResponseShouldBeNull_WhenTweetDoesNotExist()
    {
        // Arrange
        var tweetId = "E5EE35D0-F804-41CB-9D34-2324FDD240DB";

        var command = new DeleteTweetCommand(tweetId);

        var cts = new CancellationTokenSource();

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DeleteTweetCommandResponseShouldBeNull_WhenUserIdAndTweetIdIsNotEqual()
    {
        // Arrange
        var tweetUserId = "2A1CE2B4-AE34-4BFC-ACEC-CBA625062447";

        var currentUserId = "DD72C73D-7E41-44BA-8970-2D25FBB3D337";

        var tweet = new Domain.Entities.Tweet
        {
            UserId = tweetUserId
        };

        var command = new DeleteTweetCommand(tweetUserId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetUserId))
            .ReturnsAsync(tweet);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DeleteTweetCommandResponseShouldDeleteTweetAndUserTimeline_WhenTweetIsExsistWithUserIdAndTweetIdIsEqual()
    {
        // Arrange
        var tweetId = "BA7F1B77-361F-41F3-90EA-1A3B8EBD3C0D";

        var tweetUserId = "2A1CE2B4-AE34-4BFC-ACEC-CBA625062447";

        var currentUserId = "DD72C73D-7E41-44BA-8970-2D25FBB3D337";

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var userTimeline = new UserTimeline
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var command = new DeleteTweetCommand(tweetUserId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetUserId))
            .ReturnsAsync(tweet);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.IsSuccess;

        // Assert
        result.Should().BeTrue();

        _tweetRepositoryMock.Verify(x =>
            x.DeleteByIdAsync(tweet.Id), Times.Once);

        _userTimelineRepositoryMock.Verify(x =>
            x.DeleteOneAsync(It.Is<Expression<Func<UserTimeline, bool>>>(y =>
            y.Compile()(userTimeline))));
    }
}
