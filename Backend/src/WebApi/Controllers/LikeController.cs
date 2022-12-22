using Application.Likes.Commands;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi.Controllers;
[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LikeController : BaseApiController
{

    [HttpPost("like-tweet")]
    public async Task<IActionResult> LikeTweet(LikeCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

}