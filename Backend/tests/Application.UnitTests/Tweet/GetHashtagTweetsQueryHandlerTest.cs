using Application.Tweets.Queries.GetHashtagTweets;
using System.Linq.Expressions;

namespace Application.UnitTests.Tweet;

public class GetHashtagTweetsQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBlockRepository> _blockRepositoryMock;
    private readonly Mock<IHashtagRepository> _hashtagRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<IRetweetRepository> _retweetRepositoryMock;
    private readonly Mock<ILogger<GetHashtagTweetsQueryHandler>> _loggerMock;
    private readonly GetHashtagTweetsQueryHandler _sut;

    public GetHashtagTweetsQueryHandlerTest()
    {
        _mapperMock = new();
        _tweetRepositoryMock = new();
        _currentUserServiceMock = new();
        _blockRepositoryMock = new();
        _hashtagRepositoryMock = new();
        _userRepositoryMock = new();
        _likeRepositoryMock = new();
        _retweetRepositoryMock = new();
        _loggerMock = new();

        _sut = new GetHashtagTweetsQueryHandler(
            _mapperMock.Object,
            _tweetRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _blockRepositoryMock.Object,
            _hashtagRepositoryMock.Object,
            _userRepositoryMock.Object,
            _likeRepositoryMock.Object,
            _retweetRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetHashtagTweetsQueryResponseShouldBeNull_WhenHashTagIsEmpty()
    {
        // Arrange
        var query = new GetHashtagTweetsQuery(
            new PaginationQueryRequest { Keyword = string.Empty });

        var cts = new CancellationTokenSource();

        // Act
        var result = _sut.Handle(query, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetHashtagTweetsQueryResponsShouldBeNull_WhenHashTagDoesNotExist()
    {
        // Arrange
        var paginationQueryRequest = new PaginationQueryRequest();

        var query = new GetHashtagTweetsQuery(paginationQueryRequest);

        var cts = new CancellationTokenSource();

        // Act
        var result = _sut.Handle(query, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetHashtagTweetsQueryResponsShouldEmptyTweetList_WhenHashTagsDoesNotExist()
    {
        // Arrange
        var hashTag = new Hashtag();

        var hashTagResult = new List<Hashtag>();

        var tweetViewModelList = new List<TweetViewModel>();

        var pageNumber = 1;

        var paginationQueryRequest = new PaginationQueryRequest()
        {
            Keyword = "#learnathon"
        };

        var query = new GetHashtagTweetsQuery(paginationQueryRequest);

        var cts = new CancellationTokenSource();

        _hashtagRepositoryMock.Setup(x =>
            x.GetHashtagTweetByDescendingTime(It.Is<Expression<Func<Hashtag, bool>>>(y =>
           y.Compile()(hashTag)), pageNumber))
            .Returns(hashTagResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModelList);
    }

    [Fact]
    public void GetHashtagTweetsQueryResponsShouldAddHashTag_WhenCurrentUserBlockAndUserBlockDoesNotExist()
    {
        // Arrange
        var userId = "C75036EE-91F6-485B-B40F-020F26501590";
        var tweetId = "A0800C25-2B4E-4DBC-8438-4CFBDF7A1779";
        var currentUserId = "4B82889A-A9D5-46BF-A79B-77EFB38F74CA";

        var hashTag = new Hashtag
        {
            TweetId = tweetId,
            TagName = "#learnathon",
            UserId = userId
        };

        var hashTagResult = new List<Hashtag>
        {
            new Hashtag
            {
                TweetId = tweetId,
                TagName = "#learnathon",
                UserId = userId
            }
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var pageNumber = 1;

        var paginationQueryRequest = new PaginationQueryRequest()
        {
            Keyword = "#learnathon"
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = userId
        };

        var currentUserBlockResult = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = userId
        };

        var userBlock = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = userId
        };

        var userBlockResult = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = userId
        };

        var tweetCollection = new List<Domain.Entities.Tweet>
        {
            new Domain.Entities.Tweet
            {
                Id = tweetId,
                UserId = userId
            }
        };

        var userCollection = new List<User>
        {
            new User
            {
                Id = userId
            }
        };

        var query = new GetHashtagTweetsQuery(paginationQueryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _hashtagRepositoryMock.Setup(x =>
            x.GetHashtagTweetByDescendingTime(It.Is<Expression<Func<Hashtag, bool>>>(y =>
           y.Compile()(hashTag)), pageNumber))
            .Returns(hashTagResult);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(userBlock))))
            .ReturnsAsync(() => null!);

        _tweetRepositoryMock.Setup(x => x.GetCollection())
            .Returns(tweetCollection.AsQueryable());

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userCollection.AsQueryable());

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!
            .FirstOrDefault()!.UserId;

        // Assert
        result.Should().BeEquivalentTo(userId);
    }

    [Fact]
    public void GetHashtagTweetsQueryResponsShouldBeTrue_WhenLikedByCurrentUserIsExist()
    {
        // Arrange
        var userId = "BA770F8F-7465-40C1-B482-FB3A6E64736D";
        var tweetId = "B9A812F8-DBA6-423C-BD9D-14B97192A7AE";
        var currentUserId = "9D906073-CF78-4AA4-9E4A-1484814558AA";

        var hashTag = new Hashtag
        {
            TweetId = tweetId,
            TagName = "#learnathon",
            UserId = userId
        };

        var hashTagResult = new List<Hashtag>
        {
            new Hashtag
            {
                TweetId = tweetId,
                TagName = "#learnathon",
                UserId = userId
            }
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var pageNumber = 1;

        var paginationQueryRequest = new PaginationQueryRequest()
        {
            Keyword = "#learnathon"
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = userId
        };

        var currentUserBlockResult = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = userId
        };

        var userBlock = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = userId
        };

        var userBlockResult = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = userId
        };

        var tweetCollection = new List<Domain.Entities.Tweet>
        {
            new Domain.Entities.Tweet
            {
                Id = tweetId,
                UserId = userId
            }
        };

        var userCollection = new List<User>
        {
            new User
            {
                Id = userId
            }
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

        var query = new GetHashtagTweetsQuery(paginationQueryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _hashtagRepositoryMock.Setup(x =>
            x.GetHashtagTweetByDescendingTime(It.Is<Expression<Func<Hashtag, bool>>>(y =>
           y.Compile()(hashTag)), pageNumber))
            .Returns(hashTagResult);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(userBlock))))
            .ReturnsAsync(() => null!);

        _tweetRepositoryMock.Setup(x => x.GetCollection())
            .Returns(tweetCollection.AsQueryable());

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userCollection.AsQueryable());

        _likeRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))))
            .ReturnsAsync(likeResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!
            .FirstOrDefault()!.IsLikedByCurrentUser;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetHashtagTweetsQueryResponsShouldBeTrue_WhenRetweetedByCurrentUserIsExist()
    {
        // Arrange
        var userId = "BA770F8F-7465-40C1-B482-FB3A6E64736D";
        var tweetId = "B9A812F8-DBA6-423C-BD9D-14B97192A7AE";
        var currentUserId = "9D906073-CF78-4AA4-9E4A-1484814558AA";

        var hashTag = new Hashtag
        {
            TweetId = tweetId,
            TagName = "#learnathon",
            UserId = userId
        };

        var hashTagResult = new List<Hashtag>
        {
            new Hashtag
            {
                TweetId = tweetId,
                TagName = "#learnathon",
                UserId = userId
            }
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var pageNumber = 1;

        var paginationQueryRequest = new PaginationQueryRequest()
        {
            Keyword = "#learnathon"
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = userId
        };

        var currentUserBlockResult = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = userId
        };

        var userBlock = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = userId
        };

        var userBlockResult = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = userId
        };

        var tweetCollection = new List<Domain.Entities.Tweet>
        {
            new Domain.Entities.Tweet
            {
                Id = tweetId,
                UserId = userId
            }
        };

        var userCollection = new List<User>
        {
            new User
            {
                Id = userId
            }
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

        var query = new GetHashtagTweetsQuery(paginationQueryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _hashtagRepositoryMock.Setup(x =>
            x.GetHashtagTweetByDescendingTime(It.Is<Expression<Func<Hashtag, bool>>>(y =>
           y.Compile()(hashTag)), pageNumber))
            .Returns(hashTagResult);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(userBlock))))
            .ReturnsAsync(() => null!);

        _tweetRepositoryMock.Setup(x => x.GetCollection())
            .Returns(tweetCollection.AsQueryable());

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userCollection.AsQueryable());

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(retweet))))
            .ReturnsAsync(retweetResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!
            .FirstOrDefault()!.IsRetweetedByCurrentUser;

        // Assert
        result.Should().BeTrue();
    }
}
