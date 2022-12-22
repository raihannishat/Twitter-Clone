namespace Application.UnitTests.Comment;

public class CreateCommentCommandHandlerTest
{
    private readonly CreateCommentCommandHandler _sut;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<ILogger<CreateCommentCommandHandler>> _loggerMock;

    public CreateCommentCommandHandlerTest()
    {
        _tweetRepositoryMock = new();
        _mapperMock = new();
        _currentUserServiceMock = new();
        _notificationRepositoryMock = new();
        _commentRepositoryMock = new();
        _loggerMock = new();

        _sut = new CreateCommentCommandHandler(
            _mapperMock.Object,
            _currentUserServiceMock.Object,
            _tweetRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _commentRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void CreateCommentResponseShouldBeNull_WhenTweetDoesNotExsist()
    {
        // Arrange
        var tweetId = "5FD90539-D482-4A54-B27C-DE916AF44632";

        var command = new CreateCommentCommand
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
    public void CreateCommentResponseShouldBeInsert_WhenTweetOwnerIdIsNotEqualCurrentUserId()
    {
        // Arrange
        var tweetId = "5FD90539-D482-4A54-B27C-DE916AF44632";

        var tweetUserId = "B7D7480C-E5C4-4361-8A70-93BDB4D36A40";

        var currentUserId = "8438BC21-A678-46B3-9D03-E68BE7D518F0";

        var commentId = "CACD68B1-6163-4D7A-BFEF-94921CE9F18A";

        var command = new CreateCommentCommand
        {
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = tweetUserId,
            Comments = 0
        };

        var comment = new Domain.Entities.Comment
        {
            Id = commentId,
        };

        var notification = new Notification
        {
            TweetId = tweetId,
            UserId = tweetUserId,
        };

        var commentRespons = new CommentResponse
        {
            TotalComments = 1
        };

        var cts = new CancellationTokenSource();

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => tweet);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(commentRespons);

        _notificationRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Notification>()), Times.Once);
    }
}
