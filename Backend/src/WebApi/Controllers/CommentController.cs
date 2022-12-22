namespace WebApi.Controllers;

[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CommentController : BaseApiController
{
    [HttpGet("Get")]
    public async Task<IActionResult> Get([FromQuery] string tweetId, [FromQuery]PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetCommentQuery(queryRequest, tweetId)));
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(CreateCommentCommand command)
    {
        
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromQuery]DeleteCommentCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}
