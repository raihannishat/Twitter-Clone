using Application.Photos.Shared.Interfaces;
using Application.Photos.Shared.Models;

namespace Application.Photos.Commands.ProfilePhoto;

public class PhotoUploadCommandHandler : IRequestHandler<PhotoUploadCommand, Result<PhotoUploadResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IPhotoAccessor _photoAccessor;

    public PhotoUploadCommandHandler(ICurrentUserService currentUserService, IUserRepository userRepository, IPhotoAccessor photoAccessor)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _photoAccessor = photoAccessor;
    }

    public async Task<Result<PhotoUploadResponse>> Handle(PhotoUploadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var user = await _userRepository.FindByIdAsync(currentUserId);

        if (user == null)
        {
            return null!;
        }

        var photoUploadResult = await _photoAccessor.AddPhoto(request.File!);

        if (photoUploadResult == null)
        {
            return null!;
        }

        user.Image = photoUploadResult.Url;

        await _userRepository.ReplaceOneAsync(user);

        var response = new PhotoUploadResponse() { Image = user.Image };

        return Result<PhotoUploadResponse>.Success(response);
    }
}
