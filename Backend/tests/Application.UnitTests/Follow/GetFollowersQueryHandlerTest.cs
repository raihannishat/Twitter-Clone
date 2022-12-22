using System.Linq.Expressions;

namespace Application.UnitTests.Follow;

public class GetFollowersQueryHandlerTest
{
    private readonly Mock<IFollowRepository> _followRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBlockRepository> _blockRepositoryMock;
    private readonly Mock<ILogger<GetFollowersQueryHandler>> _loggerMock;
    private readonly GetFollowersQueryHandler _sut;

    public GetFollowersQueryHandlerTest()
    {
        _followRepositoryMock = new();
        _userRepositoryMock = new();
        _currentUserServiceMock = new();
        _blockRepositoryMock = new();
        _loggerMock = new();

        _sut = new GetFollowersQueryHandler(
            _followRepositoryMock.Object,
            _userRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _blockRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetFollowersResponseShouldBeNull_WhenTargetUserDoesNotExist()
    {
        // Arrange
        var targetUserId = "8D7D4079-05D9-4F0E-8AA4-955332D4F55F";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId
        };

        var query = new GetFollowersQuery(request);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(query, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFollowersResponseShouldBeAddFollower_WhenCurrentUserDoesNotBlockFollowUser()
    {
        // Arrange
        var targetUserId = "E8C6C744-54FD-4662-B3C9-5A858E5A548B";
        var currentUserId = "BC2EFCF9-B598-496F-86EB-C7B13DCD1C90";
        var followerId = "4BEF90C4-91FA-447F-9012-0EF91E15E16A";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId,
            PageNumber = 1
        };

        var user = new User
        {
            Id = targetUserId
        };

        var follow = new Domain.Entities.Follow
        {
            FollowedId = targetUserId
        };

        var followList = new List<Domain.Entities.Follow>
        {
            new Domain.Entities.Follow
            {
                FollowerId = followerId
            }
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = followerId
        };

        var followedUserBlock = new Blocks
        {
            BlockedId = followerId,
            BlockedById = currentUserId
        };

        var userList = new List<User>
        {
            new User
            {
                Id = followerId
            }
        };

        var query = new GetFollowersQuery(request);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);

        _followRepositoryMock.Setup(x =>
            x.FindByMatchWithPagination(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(follow)), request.PageNumber))
            .Returns(followList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(followedUserBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userList.AsQueryable());

        // Act
        var result = _sut.Handle(query, cts.Token).Result
            .Value!.FirstOrDefault()!.Id;

        // Assert
        result.Should().BeEquivalentTo(followerId);
    }

    [Fact]
    public void GetFollowersResponseIsFollowingShouldBeTrue_WhenCurrentUserIsAlreadyFollowing()
    {
        // Arrange
        var targetUserId = "8D7D4079-05D9-4F0E-8AA4-955332D4F55F";
        var currentUserId = "6DFF732F-9D3B-4EF6-9EB2-D66577E903B3";
        var followerId = "503346FC-A472-4A73-97D9-51E2A809AF04";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId,
            PageNumber = 1
        };

        var user = new User
        {
            Id = currentUserId
        };

        var follow = new Domain.Entities.Follow
        {
            FollowedId = targetUserId
        };

        var followList = new List<Domain.Entities.Follow>
        {
            new Domain.Entities.Follow
            {
                FollowerId = followerId
            }
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = followerId
        };

        var followedUserBlock = new Blocks
        {
            BlockedId = followerId,
            BlockedById = currentUserId
        };

        var userList = new List<User>
        {
            new User
            {
                Id = followerId
            }
        };

        var following = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = followerId
        };

        var followingResult = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = followerId
        };

        var query = new GetFollowersQuery(request);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);

        _followRepositoryMock.Setup(x =>
            x.FindByMatchWithPagination(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(follow)), request.PageNumber))
            .Returns(followList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(followedUserBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userList.AsQueryable());

        _followRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(following))))
            .ReturnsAsync(followingResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!
            .FirstOrDefault()!.IsFollowing;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetFollowersResponseIsCurrentUserShouldBeTrue_WhenCurrentUserIsAlreadyFollowing()
    {
        // Arrange
        var targetUserId = "6D7DDBBC-C168-410C-B97C-0E4E4F972EEF";
        var currentUserId = "997178E1-17A3-43C8-B524-EBD8CF8AF54D";
        var followerId = "993FF8C2-8FD1-4D5F-B5D4-93EB623B0DC7";

        var request = new PaginationQueryRequest
        {
            UserId = targetUserId,
            PageNumber = 1
        };

        var user = new User
        {
            Id = currentUserId
        };

        var follow = new Domain.Entities.Follow
        {
            FollowedId = targetUserId
        };

        var followList = new List<Domain.Entities.Follow>
        {
            new Domain.Entities.Follow
            {
                FollowerId = currentUserId
            }
        };

        var currentUserBlock = new Blocks
        {
            BlockedId = currentUserId,
            BlockedById = followerId
        };

        var followedUserBlock = new Blocks
        {
            BlockedId = followerId,
            BlockedById = currentUserId
        };

        var userList = new List<User>
        {
            new User
            {
                Id = currentUserId
            }
        };

        var following = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId
        };

        var followingResult = new Domain.Entities.Follow
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId
        };

        var query = new GetFollowersQuery(request);

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(user);

        _followRepositoryMock.Setup(x =>
            x.FindByMatchWithPagination(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(follow)), request.PageNumber))
            .Returns(followList);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(currentUserBlock))))
            .ReturnsAsync(() => null!);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(followedUserBlock))))
            .ReturnsAsync(() => null!);

        _userRepositoryMock.Setup(x => x.GetCollection())
            .Returns(userList.AsQueryable());

        _followRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Domain.Entities.Follow, bool>>>(y =>
            y.Compile()(following))))
            .ReturnsAsync(followingResult);

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value!
            .FirstOrDefault()!.IsCurrentUser;

        // Assert
        result.Should().BeTrue();
    }
}
