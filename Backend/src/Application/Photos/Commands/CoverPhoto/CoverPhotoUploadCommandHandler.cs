using Application.Photos.Shared.Interfaces;
using Application.Photos.Shared.Models;

namespace Application.Photos.Commands.CoverPhoto;

public class CoverPhotoUploadCommandHandler : IRequestHandler<CoverPhotoUploadCommand, Result<CoverPhotoUploadResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IPhotoAccessor _photoAccessor;

    public CoverPhotoUploadCommandHandler(ICurrentUserService currentUserService, IUserRepository userRepository, IPhotoAccessor photoAccessor)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _photoAccessor = photoAccessor;
    }

    public async Task<Result<CoverPhotoUploadResponse>> Handle(CoverPhotoUploadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var user = await _userRepository.FindByIdAsync(currentUserId);

        if (user == null)
        {
            return null!;
        }

        var photoUploadResult = await _photoAccessor.AddCoverPhoto(request.File!);

        if (photoUploadResult == null)
        {
            return null!;
        }

        user.CoverImage = photoUploadResult.Url;

        await _userRepository.ReplaceOneAsync(user);

        var response = new CoverPhotoUploadResponse() { Image = user.CoverImage };

        return Result<CoverPhotoUploadResponse>.Success(response);
    }
}
