using Application.Follows.Commands.FollowUser;
using Application.Follows.Queries.GetFollowers;
using Application.Follows.Queries.GetFollowing;

namespace WebApi.Controllers;
[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FollowController : BaseApiController
{
    private readonly ILogger<UserController> _logger;

    public FollowController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpPost("id")]
    public async Task<IActionResult> Follow(string targetId)
    {
        return HandleResult(await Mediator.Send(new FollowUserCommand(targetId)));
    }


    [HttpGet("follower-list")]
    public async Task<IActionResult> GetAllFollower([FromQuery]PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetFollowersQuery(queryRequest)));
    }

    [HttpGet("following-list")]
    public async Task<IActionResult> GetAllFollowing([FromQuery]PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetFollowingQuery(queryRequest)));
    }
}