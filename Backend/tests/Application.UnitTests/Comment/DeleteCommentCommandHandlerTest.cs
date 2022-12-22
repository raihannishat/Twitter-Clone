namespace Application.UnitTests.Comment;

public class DeleteCommentCommandHandlerTest
{
    private readonly DeleteCommentCommandHandler _sut;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<ILogger<DeleteCommentCommandHandler>> _loggerMock;

    public DeleteCommentCommandHandlerTest()
    {
        _tweetRepositoryMock = new();
        _mapperMock = new();
        _currentUserServiceMock = new();
        _commentRepositoryMock = new();
        _loggerMock = new();

        _sut = new DeleteCommentCommandHandler(
            _tweetRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object,
            _commentRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void DeleteCommentResponseShouldBeNull_WhenTweetDoesNotExist()
    {
        // Arrange
        var tweetId = "C4452BB7-495E-4CFB-ABEA-A6190D9B4D05";

        var command = new DeleteCommentCommand
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
    public void DeleteCommentResponseShouldBeNull_WheneCommentAndUserAreNotSame()
    {
        // Arrange
        var commentId = "40009347-338A-407F-BA9D-961D338BA217";
        var tweetId = "792F2288-0297-4B85-839D-B0F18A412303";
        var currentUserId = "52F41AC7-AEC2-45AC-B92A-729F772E7FB4";
        var commentUserId = "EC87F58E-966B-4C7D-9D27-C0DBC54E60C0";

        var command = new DeleteCommentCommand
        {
            CommentId = commentId,
            TweetId = tweetId
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var comment = new Domain.Entities.Comment
        {
            UserId = commentUserId
        };

        var cts = new CancellationTokenSource();

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _commentRepositoryMock.Setup(x => x.FindByIdAsync(commentId))
            .ReturnsAsync(comment);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }
}
