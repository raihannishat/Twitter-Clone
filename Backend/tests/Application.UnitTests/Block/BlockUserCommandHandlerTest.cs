using System.Linq.Expressions;

namespace Application.UnitTests.Block;

public class BlockUserCommandHandlerTest
{
    private readonly BlockUserCommandHandler _sut;
    private readonly Mock<IBlockRepository> _blockRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<BlockUserCommandHandler>> _loggerMock;

    public BlockUserCommandHandlerTest()
    {
        _blockRepositoryMock = new();
        _userRepositoryMock = new();
        _currentUserServiceMock = new();
        _loggerMock = new();
        _sut = new BlockUserCommandHandler(_blockRepositoryMock.Object,
            _userRepositoryMock.Object, _currentUserServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void BlockResponseShouldBeNull_WhenTargetUserDoesNotExsist()
    {
        //Arrange
        var targetUserId = "AD79D8A0-F97C-4539-AA76-43E25E235D94";
        var command = new BlockUserCommand(targetUserId);
        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(() => null!);

        //Act
        var result = _sut.Handle(command, cts.Token).Result;

        //Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BlockResponseShouldBeNull_WhenTargetUserAndCurrentUserAreSame()
    {
        //Arrange
        var targetUserId = "3BC3F8CD-52A7-48D6-B1A2-D75F2F72E5D3";
        var command = new BlockUserCommand(targetUserId);
        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(targetUserId);

        //Act
        var result = _sut.Handle(command, cts.Token).Result;

        //Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BlockReponseShouldBeTrue_WhenUserIsBlockedSuccessFully()
    {
        //Arrange
        var currentUserId = "1BC3F8CD-52A7-48D6-B1A2-D75F2F72E5D3";

        var targetUserId = "3BC3F8CD-52A7-48D6-B1A2-D75F2F72E5D3";

        var targetUser = new User
        {
            Id = targetUserId
        };

        var blocksEntity = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = targetUserId
        };

        var command = new BlockUserCommand(targetUserId);
        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(targetUser);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(blocksEntity))))
            .ReturnsAsync(() => null!);

        //Act
        var result = _sut.Handle(command, cts.Token).Result.Value!.IsBlocked;

        //Assert
        result.Should().BeTrue();

        _blockRepositoryMock.Verify(x =>
            x.InsertOneAsync(It.IsAny<Blocks>()));
    }

    [Fact]
    public void BlockReponseShouldBeFalse_WhenUserIsAlreadyBlocked()
    {
        //Arrange
        var currentUserId = "1BC3F8CD-52A7-48D6-B1A2-D75F2F72E5D3";

        var targetUserId = "3BC3F8CD-52A7-48D6-B1A2-D75F2F72E5D3";

        var targetUser = new User
        {
            Id = targetUserId
        };

        var block = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = targetUserId
        };

        var blocksResult = new Blocks
        {
            BlockedById = currentUserId,
            BlockedId = targetUserId
        };

        var command = new BlockUserCommand(targetUserId);
        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
           .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(targetUserId))
            .ReturnsAsync(targetUser);

        _blockRepositoryMock.Setup(x =>
            x.FindOneByMatchAsync(It.Is<Expression<Func<Blocks, bool>>>(y =>
            y.Compile()(block))))
            .ReturnsAsync(blocksResult);

        //Act
        var result = _sut.Handle(command, cts.Token).Result.Value!.IsBlocked;

        //Assert
        result.Should().BeFalse();

        _blockRepositoryMock.Verify(x =>
            x.DeleteByIdAsync(blocksResult.Id));
    }
}
