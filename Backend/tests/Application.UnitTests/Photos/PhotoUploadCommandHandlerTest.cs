using Application.Photos.Commands.ProfilePhoto;
using Application.Photos.Shared.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Application.UnitTests.Photos;

public class PhotoUploadCommandHandlerTest
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPhotoAccessor> _photoAccessorMock;
    private readonly PhotoUploadCommandHandler _sut;

    public PhotoUploadCommandHandlerTest()
    {
        _currentUserServiceMock = new();
        _userRepositoryMock = new();
        _photoAccessorMock = new();

        _sut = new PhotoUploadCommandHandler(
            _currentUserServiceMock.Object,
            _userRepositoryMock.Object,
            _photoAccessorMock.Object);
    }

    [Fact]
    public void ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var currentUserId = "2B122B8E-B6F9-4D6B-B3C0-0463097E4861";

        var command = new PhotoUploadCommand();

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

    // [Fact]
    public void ShouldReturnNull_WhenPhotoUploadFailed()
    {
        // Arrange
        var currentUserId = "E6838740-9270-4E49-BC08-15E3256C639C";

        var photoMock = new Mock<IFormFile>().Object;

        var user = new User
        {
            Id = currentUserId
        };

        var command = new PhotoUploadCommand
        {
            File = photoMock
        };

        var cts = new CancellationTokenSource();

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(currentUserId);

        _userRepositoryMock.Setup(x => x.FindByIdAsync(currentUserId))
            .ReturnsAsync(user);

        _photoAccessorMock.Setup(x => x.AddPhoto(command.File!))
            .Returns(() => null!);

        // Act
        var result = _sut.Handle(command, cts.Token).Result;

        // Assert
        result.Should().BeNull();
    }
}
