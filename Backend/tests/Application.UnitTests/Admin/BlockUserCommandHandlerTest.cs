using Application.IdentityManagement.Shared.Models;

namespace Application.UnitTests.Admin;

public class BlockUserCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<IdentityManagement.Admin.Commands.BlockUser.BlockUserCommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IdentityManagement.Admin.Commands.BlockUser.BlockUserCommandHandler _sut;

    public BlockUserCommandHandlerTest()
    {
        _userRepositoryMock = new();
        _mapperMock = new();
        _loggerMock = new();
        _currentUserServiceMock = new();

        _sut = new IdentityManagement.Admin.Commands.BlockUser.BlockUserCommandHandler(
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public void ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "3EB7DC24-341A-49A5-9395-E87EB80E0026";

        var command = new IdentityManagement.Admin.Commands.BlockUser.BlockUserCommand(userId);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BlockResponsShouldBeFalse_WhenUserIsBlocked()
    {
        // Arrange
        var userId = "07CC71DF-3A50-4E2E-B1B4-1F054342AAB2";

        var user = new User
        {
            Id = userId,
            IsBlockedByAdmin = true
        };

        var blockResponse = new AdminBlockResponse()
        {
            IsBlocked = false
        };

        var command = new IdentityManagement.Admin.Commands.BlockUser.BlockUserCommand(userId);

        var cts = new CancellationTokenSource();

        _userRepositoryMock.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(() => user);

        // Act
        var result = _sut.Handle(command, cts.Token).Result.Value!.IsBlocked;

        // Assert
        result.Should().BeFalse();

        _userRepositoryMock.Verify(x =>
            x.ReplaceOneAsync(user), Times.Once);
    }
}
