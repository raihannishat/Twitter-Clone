using Application.Photos.Commands.CoverPhoto;
using Application.Photos.Shared.Interfaces;

namespace Application.UnitTests.Photos;

public class CoverPhotoUploadCommandHandlerTest
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPhotoAccessor> _photoAccessorMock;
    private readonly CoverPhotoUploadCommandHandler _sut;

    public CoverPhotoUploadCommandHandlerTest()
    {
        _currentUserServiceMock = new();
        _userRepositoryMock = new();
        _photoAccessorMock = new();

        _sut = new CoverPhotoUploadCommandHandler(
            _currentUserServiceMock.Object,
            _userRepositoryMock.Object,
            _photoAccessorMock.Object);
    }

    [Fact]
    public void ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var currentUserId = "160E6DC9-E7E1-4D96-9DF4-C2DC520A8D78";

        var command = new CoverPhotoUploadCommand();

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }
}
