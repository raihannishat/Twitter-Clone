using Application.Block.Commands;
using Application.Block.Queries;

namespace WebApi.Controllers;


[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BlockController : BaseApiController
{
    [HttpPost("block-user")]
    public async Task<IActionResult> Block([FromQuery] string userId)
    {
        return HandleResult(await Mediator.Send(new BlockUserCommand(userId)));
    }

    [HttpGet("get-users")]
    public async Task<IActionResult> GetBlockUser([FromQuery]PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetBlockUsersQuery(queryRequest)));
    }
}