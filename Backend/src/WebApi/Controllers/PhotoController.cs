using Application.Photos.Commands.CoverPhoto;
using Application.Photos.Commands.ProfilePhoto;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers;
[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PhotoController : BaseApiController
{

    [HttpPost("upload-photo")]
    public async Task<IActionResult> Add([FromForm]PhotoUploadCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("upload-cover-photo")]
    public async Task<IActionResult> AddCoverPhoto([FromForm] CoverPhotoUploadCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

}