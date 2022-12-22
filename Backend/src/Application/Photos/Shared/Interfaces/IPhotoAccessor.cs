using Application.Photos.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Photos.Shared.Interfaces;

public interface IPhotoAccessor
{
    Task<PhotoUploadResult> AddPhoto(IFormFile file);
    Task<PhotoUploadResult> AddCoverPhoto(IFormFile file);
}
