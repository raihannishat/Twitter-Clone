using System.Linq.Expressions;

namespace Application.UnitTests.Comment;

public class GetCommentQueryHandlerTest
{
    private readonly GetCommentQueryHandler _sut;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IBlockRepository> _blockRepositoryMock;
    private readonly Mock<ILogger<GetCommentQueryHandler>> _loggerMock;

    public GetCommentQueryHandlerTest()
    {
        _mapperMock = new();
        _tweetRepositoryMock = new();
        _currentUserServiceMock = new();
        _userRepositoryMock = new();
        _commentRepositoryMock = new();
        _blockRepositoryMock = new();
        _loggerMock = new();

        _sut = new GetCommentQueryHandler(
            _mapperMock.Object,
            _tweetRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _userRepositoryMock.Object,
            _commentRepositoryMock.Object,
            _blockRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetCommentResponseShouldBeNull_WhenTweetDoesNotExist()
    {
        // Arrange
        var tweetId = "7A3E36AE-68F5-4756-B025-08F6305EF77D";

        var pageNumber = 1;

        var paging = new PaginationQueryRequest
        {
            PageNumber = pageNumber
        };

        var query = new GetCommentQuery(paging, tweetId);

        var cts = new CancellationTokenSource();

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCommentResponseShouldBeAddComment_WhenCurrentUserBlockAndUserBlockDoesNotExist()
    {
        // Arrange
        var tweetId = "D5B4F60E-D949-4789-8F74-920DE0943D0B";

        var currentUserId = "15286D3A-012A-4FDC-8D5E-C42CC7EC9105";

        var pageNumber = 1;

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var comment = new Domain.Entities.Comment
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = comment.UserId
        };

        var userBlock = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = comment.UserId
        };

        var commentList = new List<Domain.Entities.Comment>
        {
            new Domain.Entities.Comment
            {
                TweetId = tweetId,
                UserId = currentUserId
            },
        };

        var paging = new PaginationQueryRequest
        {
            PageNumber = pageNumber
        };

        var users = new List<User>
        {
            new User
            {
                Id = currentUserId,
                Name = "Raihan Nishat",
                Image = "photo.jpg"
            }
        };

        var query = new GetCommentQuery(paging, tweetId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _commentRepositoryMock.Setup(x =>
            x.GetCommentByDescendingTime(It.Is<Expression<Func<Domain.Entities.Comment, bool>>>(y =>
            y.Compile()(comment)), pageNumber))
            .ReturnsAsync(commentList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(userBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(users.AsQueryable());

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.UserId;

        // Assert
        result.Should().BeEquivalentTo(currentUserId);
    }

    [Fact]
    public void GetCommentResponseDeleteStatusShouldBeTrue_WhenCommentUserIdAndCurrentUserIdIsEqual()
    {
        // Arrange
        var tweetId = "D0465137-3BDA-42ED-962D-82D4169C9556";

        var currentUserId = "C6ECF545-CF09-45DF-B480-035F4887BBED";

        var pageNumber = 1;

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var comment = new Domain.Entities.Comment
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = comment.UserId
        };

        var userBlock = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = comment.UserId
        };

        var commentList = new List<Domain.Entities.Comment>
        {
            new Domain.Entities.Comment
            {
                TweetId = tweetId,
                UserId = currentUserId
            },
        };

        var paging = new PaginationQueryRequest
        {
            PageNumber = pageNumber
        };

        var users = new List<User>
        {
            new User
            {
                Id = currentUserId,
                Name = "Raihan Nishat",
                Image = "photo.jpg"
            }
        };

        var query = new GetCommentQuery(paging, tweetId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _commentRepositoryMock.Setup(x =>
            x.GetCommentByDescendingTime(It.Is<Expression<Func<Domain.Entities.Comment, bool>>>(y =>
            y.Compile()(comment)), pageNumber))
            .ReturnsAsync(commentList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(userBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(users.AsQueryable());

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.CanDelete;

        // Assert
        result.Should().BeTrue();
    }
}
