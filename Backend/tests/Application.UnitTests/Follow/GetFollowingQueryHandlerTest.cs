using System.Linq.Expressions;

namespace Application.UnitTests.Follow;

public class GetFollowingQueryHandlerTest
{
    private readonly Mock<IFollowRepository> _followRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBlockRepository> _blockRepositoryMock;
    private readonly Mock<ILogger<GetFollowingQueryHandler>> _loggerMock;
    private readonly GetFollowingQueryHandler _sut;

    public GetFollowingQueryHandlerTest()
    {
        _followRepositoryMock = new();
        _userRepositoryMock = new();
        _currentUserServiceMock = new();
        _blockRepositoryMock = new();
        _loggerMock = new();

        _sut = new GetFollowingQueryHandler(
            _followRepositoryMock.Object,
            _userRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _blockRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetFollowingQueryResponseShouldBeNull_WhenTargetUserDoesNotExist()
    {
        // Arrange
        var targetUserId = "F7AB3876-D8C5-42E7-A799-D7C1C32614D7";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId
        };

        var query = new GetFollowingQuery(request);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFollowingQueryResponseShouldBeAddFollowing_WhenCurrentUserDoesNotBlockFollowUser()
    {
        // Arrange
        var targetUserId = "66922066-95C1-403B-8F78-EAC6F71465EB";
        var currentUserId = "22F1DA45-5007-42D7-9D01-4EF0BAA4396E";
        var followingId = "196C2C5B-847C-4A37-B2A0-11FD82A6F7EB";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId,
            PageNumber = 1
        };

        var user = new User
        {
            Id = targetUserId
        };

        var following = new Domain.Entities.Follow
        {
            FollowerId = targetUserId
        };

        var followingList = new List<Domain.Entities.Follow>
        {
            new Domain.Entities.Follow
            {
                FollowerId = targetUserId,
                FollowedId = followingId
            }
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = followingId
        };

        var followingUserBlock = new Blocks
        {
            BlockedId = followingId,
            BlockedById = currentUserId
        };

        var userList = new List<User>
        {
            new User
            {
                Id = followingId
            }
        };

        var query = new GetFollowingQuery(request);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);

        _followRepositoryMock.Setup(x =>
            x.FindByMatchWithPagination(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(following)), request.PageNumber))
            .Returns(followingList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(followingUserBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userList.AsQueryable());

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.Id;

        // Assert
        result.Should().BeEquivalentTo(followingId);
    }

    [Fact]
    public void GetFollowingQueryResponseIsFollowingShouldBeTrue_WhenCurrentUserIsAlreadyFollowing()
    {
        // Arrange
        var targetUserId = "EC36F5A6-841F-4C69-8E7C-B430BBD60A2A";
        var currentUserId = "BE026C0A-CEDF-4693-95F3-DFE695BB1BD7";
        var followingId = "C69A90E5-8B44-4F3D-9F61-700328065C5C";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId,
            PageNumber = 1
        };

        var user = new User
        {
            Id = targetUserId
        };

        var following = new Domain.Entities.Follow
        {
            FollowerId = targetUserId
        };

        var followingExpression = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = followingId
        };

        var followingResult = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = followingId
        };

        var followingList = new List<Domain.Entities.Follow>
        {
            new Domain.Entities.Follow
            {
                FollowerId = targetUserId,
                FollowedId = followingId
            }
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = followingId
        };

        var followingUserBlock = new Blocks
        {
            BlockedId = followingId,
            BlockedById = currentUserId
        };

        var userList = new List<User>
        {
            new User
            {
                Id = followingId
            }
        };

        var query = new GetFollowingQuery(request);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);

        _followRepositoryMock.Setup(x =>
            x.FindByMatchWithPagination(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(following)), request.PageNumber))
            .Returns(followingList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(followingUserBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userList.AsQueryable());

        _followRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(followingExpression))))
            .ReturnsAsync(followingResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.IsFollowing;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetFollowingQueryResponseIsCurrentUserShouldBeTrue_WhenCurrentUserIsAlreadyFollowing()
    {
        // Arrange
        var targetUserId = "0C17C213-1384-418B-BC8D-E2A81B81E70A";
        var currentUserId = "D47DCDF3-A938-47A4-98C9-221029693EEA";
        var followingId = "B49D4716-F3C6-4DDA-A7D4-BA1610F01588";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId,
            PageNumber = 1
        };

        var user = new User
        {
            Id = targetUserId
        };

        var following = new Domain.Entities.Follow
        {
            FollowerId = targetUserId,
            FollowedId = currentUserId
        };

        var followingExpression = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = followingId
        };

        var followingResult = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = followingId
        };

        var followingList = new List<Domain.Entities.Follow>
        {
            new Domain.Entities.Follow
            {
                FollowerId = targetUserId,
                FollowedId = currentUserId
            }
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = followingId
        };

        var followingUserBlock = new Blocks
        {
            BlockedId = followingId,
            BlockedById = currentUserId
        };

        var userList = new List<User>
        {
            new User
            {
                Id = currentUserId
            }
        };

        var query = new GetFollowingQuery(request);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);

        _followRepositoryMock.Setup(x =>
            x.FindByMatchWithPagination(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(following)), request.PageNumber))
            .Returns(followingList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(followingUserBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userList.AsQueryable());

        _followRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(followingExpression))))
            .ReturnsAsync(followingResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.IsCurrentUser;

        // Assert
        result.Should().BeTrue();
    }
}
