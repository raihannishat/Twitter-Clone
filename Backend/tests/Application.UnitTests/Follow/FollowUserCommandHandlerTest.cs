using System.Linq.Expressions;

namespace Application.UnitTests.Follow;

public class FollowUserCommandHandlerTest
{
    private readonly FollowUserCommandHandler _sut;
    private readonly Mock<IFollowRepository> _followRepositoryMock;
    private readonly Mock<IRepository<Domain.Entities.Follow>> _repositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<IHomeTimelineRepository> _homeTimelineRepositorymock;
    private readonly Mock<ILogger<FollowUserCommandHandler>> _loggerMock;

    public FollowUserCommandHandlerTest()
    {
        _followRepositoryMock = new();
        _repositoryMock = new();
        _currentUserServiceMock = new();
        _userRepositoryMock = new();
        _tweetRepositoryMock = new();
        _homeTimelineRepositorymock = new();
        _loggerMock = new();

        _sut = new FollowUserCommandHandler(
            _followRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _userRepositoryMock.Object,
            _tweetRepositoryMock.Object,
            _homeTimelineRepositorymock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void FollowResponseShouldBeNull_WhenUserIdAndTargetUserIdIsEqual()
    {
        // Arrange
        var targetUserId = "CAB7621F-D1D4-4BB2-A744-A2B1E4AFF3C6";

        var userId = new string(targetUserId);

        var command = new FollowUserCommand(targetUserId);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FollowResponseShouldBeNull_WhenTargetUserDoesNotExist()
    {
        // Arrange
        var targetUserId = "652BBA00-3963-48C2-972D-A10D0608C591";

        var command = new FollowUserCommand(targetUserId);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FollowResponseIsFollowingShouldBeTrue_WhenTargetUserIsNotFollowing()
    {
        //Arrange
        var currentUserId = "325E2A90-CFA0-42E8-B0DD-9787FE6668F4";
        var targetUserId = "7C617BEA-5150-4BA9-BBC7-61FF258C2EBF";

        var currentUser = new User
        {
            Id = currentUserId
        };

        var targetUser = new User
        {
            Id = targetUserId
        };

        var follow = new Domain.Entities.Follow()
        {
            FollowerId = currentUser.Id,
            FollowedId = targetUser.Id
        };

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(currentUser);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(targetUser);

        _followRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(follow))))
            .ReturnsAsync(() => null!);

        var command = new FollowUserCommand(targetUserId);

        var cts = new CancellationTokenSource();

        //Act
        var result = _sut.Handle(command, cts.Token).Result.Value!.IsFollowing;

        //Assert
        result.Should().BeTrue();

        _followRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Domain.Entities.Follow>()), Times.Once);

        _userRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(It.IsAny<User>()), Times.Exactly(2));
    }

    [Fact]
    public void FollowResponseShouldBeInsertHomeTimeline_WhenUserTweetsIsExsist()
    {
        //Arrange
        var currentUserId = "5B5D9688-5BB3-4B70-A124-1A48DB634D7E";
        var targetUserId = "DCDBA45D-B6A5-4BA1-9619-AE38F7D4CFCA";

        var currentUser = new User
        {
            Id = currentUserId
        };

        var targetUser = new User
        {
            Id = targetUserId
        };

        var follow = new Domain.Entities.Follow()
        {
            FollowerId = currentUser.Id,
            FollowedId = targetUser.Id
        };

        var tweetList = new List<Domain.Entities.Tweet>
        {
            new Domain.Entities.Tweet
            {
                UserId = currentUserId
            }
        };

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(currentUser);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(targetUser);

        _followRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(follow))))
            .ReturnsAsync(() => null!);

        _tweetRepositoryMock.Setup(x => x.GetUserTweet(targetUserId))
            .Returns(tweetList);

        var command = new FollowUserCommand(targetUserId);

        var cts = new CancellationTokenSource();

        //Act
        var result = _sut.Handle(command, cts.Token).Result.Value!.IsFollowing;

        //Assert
        result.Should().BeTrue();

        _homeTimelineRepositorymock.Verify(x =>
            x.InsertOneAsync(It.IsAny<HomeTimeline>()), Times.Once);
    }

    [Fact]
    public void FollowResponseIsFollowingShouldBeFalse_WhenTargetUserIsAlreadyFollowing()
    {
        //Arrange
        var currentUserId = "325E2A90-CFA0-42E8-B0DD-9787FE6668F4";
        var targetUserId = "7C617BEA-5150-4BA9-BBC7-61FF258C2EBF";

        var currentUser = new User
        {
            Id = currentUserId
        };

        var targetUser = new User
        {
            Id = targetUserId
        };

        var follow = new Domain.Entities.Follow()
        {
            FollowerId = currentUser.Id,
            FollowedId = targetUser.Id
        };

        var followResult = new Domain.Entities.Follow()
        {
            FollowerId = currentUser.Id,
            FollowedId = targetUser.Id
        };

        var hometimeline = new HomeTimeline
        {
            UserId = currentUserId,
            TweetOwnerId = targetUserId
        };

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(currentUser);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(targetUser);

        _followRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(follow))))
            .ReturnsAsync(followResult);

        var command = new FollowUserCommand(targetUserId);

        var cts = new CancellationTokenSource();

        //Act
        var result = _sut.Handle(command, cts.Token).Result.Value!.IsFollowing;

        //Assert
        result.Should().BeFalse();

        _followRepositoryMock.Verify(x =>
            x.DeleteByIdAsync(It.IsAny<string>()), Times.Exactly(1));

        _userRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(It.IsAny<User>()), Times.Exactly(2));

        _homeTimelineRepositorymock.Verify(x =>
            x.DeleteUserHomeTimeline(It.Is<Expression<Func<HomeTimeline, bool>>>(y =>
            y.Compile()(hometimeline))));
    }
}
