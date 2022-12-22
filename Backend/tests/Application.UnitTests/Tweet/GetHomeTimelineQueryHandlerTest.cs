using Application.Tweets.Queries.GetHomeTimelineTweets;
using System.Linq.Expressions;

namespace Application.UnitTests.Tweet;

public class GetHomeTimelineQueryHandlerTest
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IHomeTimelineRepository> _homeTimelineRepositoryMock;
    private readonly Mock<IRetweetRepository> _retweetRepositoryMock;
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IBlockRepository> _blockRepositoryMock;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<ILogger<GetHomeTimelineQueryHandler>> _loggerMock;
    private readonly GetHomeTimelineQueryHandler _sut;

    public GetHomeTimelineQueryHandlerTest()
    {
        _currentUserServiceMock = new();
        _homeTimelineRepositoryMock = new();
        _retweetRepositoryMock = new();
        _likeRepositoryMock = new();
        _userRepositoryMock = new();
        _blockRepositoryMock = new();
        _tweetRepositoryMock = new();
        _loggerMock = new();

        _sut = new GetHomeTimelineQueryHandler(
            _currentUserServiceMock.Object,
            _homeTimelineRepositoryMock.Object,
            _retweetRepositoryMock.Object,
            _likeRepositoryMock.Object,
            _userRepositoryMock.Object,
            _blockRepositoryMock.Object,
            _tweetRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetHomeTimelineQueryResponseShouldEmptyTweetList_WhenHomeTimelineDoesNotHaveAnyItem()
    {
        // Arrange
        var currentUserId = "32E2C56E-B919-4B79-AF21-867941EE4C61";
        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>();

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModelList);
    }

    [Fact]
    public void GetHomeTimelineQueryResponseShouldContinue_WhenTweetDoesNotExsist()
    {
        // Arrange
        var currentUserId = "D0F5FADC-078D-4FCF-B5CB-8B5BE232E163";

        var tweetId = "3E21D50B-A239-4447-96C2-3C1E5DCB35A6";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = currentUserId
            }
        };

        var Blocks = new Blocks
        {
            BlockedId = currentUserId
        };

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModelList);

        _blockRepositoryMock.Verify(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(Blocks))), Times.Never);
    }

    [Fact]
    public void ShouldContinue_WhenCurrentUserIsBlockedExsist()
    {
        // Arrange
        var currentUserId = "6011D11D-A11E-49DE-8EEC-A09A53990E82";

        var tweetOwnerId = "BFA89942-06C2-495A-B2A0-80B29906D118";

        var tweetId = "75800647-D4A1-4536-864D-1D59D6AA3B97";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = tweetOwnerId
            }
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var blocks = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = tweetOwnerId
        };

        var blocksResult = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = tweetOwnerId
        };

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocks))))
            .ReturnsAsync(blocksResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModelList);

        _userRepositoryMock.Verify(x => x.FindByIdAsync(currentUserId), Times.Never);
    }

    [Fact]
    public void ShouldContinue_WhenTweetOwnerIsBlockedExsist()
    {
        // Arrange
        var currentUserId = "F1E98BBA-DDAD-4502-88DA-2F3EA09E8255";

        var tweetOwnerId = "EBCE50BE-6CA1-40FD-911F-72AFC91302EB";

        var tweetId = "C1E280A9-C5D1-469A-B411-E6C54E74B13E";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = tweetOwnerId
            }
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var blocks = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var blocksResult = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocks))))
            .ReturnsAsync(blocksResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModelList);

        _userRepositoryMock.Verify(x => x.FindByIdAsync(currentUserId), Times.Never);
    }

    [Fact]
    public void ShouldContinue_WhenTweetOwnerDoesNotExsist()
    {
        // Arrange
        var currentUserId = "582DC12E-BD4D-478D-8349-1BEEC6E0F6B3";

        var tweetOwnerId = "965494B2-DB31-4A72-AC1E-E67A28A362C4";

        var tweetId = "4E02579B-22C4-436E-9F09-049BC4BA0EBA";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId,
            TweetOwnerId = tweetOwnerId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = tweetOwnerId
            }
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId
        };

        var blocks = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var blocksResult = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var like = new Domain.Entities.Like
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocks))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(homeTimeline.TweetOwnerId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModelList);

        _likeRepositoryMock.Verify(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))), Times.Never);
    }

    [Fact]
    public void ShouldContinue_WhenTweetCreatorDoesNotExsist()
    {
        // Arrange
        var currentUserId = "DC7FD15B-9767-4E44-B578-BE10F1FA0351";

        var tweetOwnerId = "9EF1C29B-1F84-499C-9A12-7C72DE7D8D69";

        var tweetId = "10A7AF74-A80E-4DBC-BDCC-04A3F77309F8";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId,
            TweetOwnerId = tweetOwnerId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = tweetOwnerId
            }
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var blocks = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var blocksResult = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var like = new Domain.Entities.Like
        {
            TweetId = tweetId,
            UserId = currentUserId
        };

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocks))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweet.UserId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(tweetViewModelList);

        _likeRepositoryMock.Verify(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))), Times.Never);
    }

    [Fact]
    public void ShouldSetLikedByCurrentUserTrue_WhenLikedByCurrentUserIsExsist()
    {
        // Arrange
        var currentUserId = "E6F6C7E5-8FD5-4327-A345-4A8A13BFEFC7";

        var tweetOwnerId = "E6F6C7E5-8FD5-4327-A345-4A8A13BFEFC7";

        var tweetId = "8F4E4E1A-522D-4654-A1DA-4605F40B462C";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId,
            TweetOwnerId = tweetOwnerId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = tweetOwnerId
            }
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var blocks = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var blocksResult = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
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

        var tweetOwnerUser = new User();

        var tweetCreatorUser = new User();

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocks))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(homeTimeline.TweetOwnerId))
            .ReturnsAsync(tweetOwnerUser);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweet.UserId))
            .ReturnsAsync(tweetCreatorUser);

        _likeRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Like, bool>>>(y =>
            y.Compile()(like))))
            .ReturnsAsync(likeResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!.FirstOrDefault();

        // Assert
        result!.IsLikedByCurrentUser.Should().BeTrue();
    }

    [Fact]
    public void ShouldSetRetweetedByCurrentUserTrue_WhenRetweetedByCurrentUserIsExsist()
    {
        // Arrange
        var currentUserId = "CB1F9697-7823-49FD-B55D-3F62316139C4";

        var tweetOwnerId = "DCAD3C96-269C-4158-B654-FB1F3F1ED003";

        var tweetId = "2C283761-4721-4C19-9E4D-94C88936B95C";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId,
            TweetOwnerId = tweetOwnerId,
            TweetId = tweetId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = tweetOwnerId
            }
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var blocks = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var blocksResult = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = homeTimeline.TweetId,
            UserId = currentUserId
        };

        var reTweetResult = new Domain.Entities.Retweet
        {
            TweetId = homeTimeline.TweetId,
            UserId = currentUserId
        };

        var tweetOwnerUser = new User();

        var tweetCreatorUser = new User();

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocks))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(homeTimeline.TweetOwnerId))
            .ReturnsAsync(tweetOwnerUser);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweet.UserId))
            .ReturnsAsync(tweetCreatorUser);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(reTweetResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!.FirstOrDefault();

        // Assert
        result!.IsRetweetedByCurrentUser.Should().BeTrue();
    }

    [Fact]
    public void ShouldSetRetweetedTrue_WhenRetweetedIsExsist()
    {
        // Arrange
        var currentUserId = "2DCD8E48-7409-4667-BC9C-CA7F89A8DBDB";

        var tweetOwnerId = "E96863AC-D0A9-47CB-B199-C31E58FC5E71";

        var tweetId = "97BB2400-8F71-4875-813C-7AD6B78F4AFC";

        var pageNumber = 1;

        var homeTimeline = new HomeTimeline
        {
            UserId = currentUserId,
            TweetOwnerId = tweetOwnerId,
            TweetId = tweetId
        };

        var tweetViewModelList = new List<TweetViewModel>();

        var homeTimelineTweets = new List<HomeTimeline>
        {
            new HomeTimeline()
            {
                TweetId = tweetId,
                TweetOwnerId = tweetOwnerId
            }
        };

        var tweet = new Domain.Entities.Tweet
        {
            Id = tweetId,
            UserId = currentUserId
        };

        var blocks = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var blocksResult = new Blocks
        {
            BlockedId = tweetOwnerId,
            BlockedById = currentUserId
        };

        var reTweet = new Domain.Entities.Retweet
        {
            TweetId = homeTimeline.TweetId,
            UserId = homeTimeline.TweetOwnerId
        };

        var reTweetResult = new Domain.Entities.Retweet
        {
            TweetId = homeTimeline.TweetId,
            UserId = homeTimeline.TweetOwnerId
        };

        var tweetOwnerUser = new User();

        var tweetCreatorUser = new User();

        var queryRequest = new PaginationQueryRequest();

        var query = new GetHomeTimelineQuery(queryRequest);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _homeTimelineRepositoryMock.Setup(x =>
            x.GetTweetByDescendingTime(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(homeTimeline)), pageNumber))
            .Returns(homeTimelineTweets);

        _tweetRepositoryMock.Setup(x => x.FindByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocks))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(homeTimeline.TweetOwnerId))
            .ReturnsAsync(tweetOwnerUser);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(tweet.UserId))
            .ReturnsAsync(tweetCreatorUser);

        _retweetRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Retweet, bool>>>(y =>
            y.Compile()(reTweet))))
            .ReturnsAsync(reTweetResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!.FirstOrDefault();

        // Assert
        result!.IsRetweeted.Should().BeTrue();
    }
}
