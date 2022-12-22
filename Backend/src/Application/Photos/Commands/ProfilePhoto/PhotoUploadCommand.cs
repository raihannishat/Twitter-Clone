using Application.Photos.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Photos.Commands.ProfilePhoto;

public class PhotoUploadCommand : IRequest<Result<PhotoUploadResponse>>
{
    public IFormFile? File { get; set; }
}
