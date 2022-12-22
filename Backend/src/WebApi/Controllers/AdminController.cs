using Application.IdentityManagement.Admin.Commands.BlockUser;
using Application.IdentityManagement.Admin.Queries;

namespace WebApi.Controllers;

[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly ILogger<UserController> _logger;

    public AdminController(ILogger<UserController> logger)
    {
        _logger = logger;
    }


    [HttpGet("GetAllUser")]
    public async Task<IActionResult> Get([FromQuery] PaginationQueryRequest query)
    {
        return HandleResult(await Mediator.Send(new GetAllUsersQuery(query)));
    }


    [HttpGet("GetUserById/{id:length(24)}")]
    public async Task<IActionResult> GetById(string id)
    {
        return HandleResult(await Mediator.Send(new GetUserByIdQuery(id)));
    }


    [HttpGet("GetUserByEmail/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        return HandleResult(await Mediator.Send(new GetUserByEmailQuery(email)));
    }


    [HttpPut("UpdateUser/{id:length(24)}")]
    public async Task<IActionResult> Update(string id, UpdateUserCommand updateUser)
    {
        updateUser.Id = id;
        return HandleResult(await Mediator.Send(updateUser));
    }

    [HttpPost("blockUser")]
    public async Task<IActionResult> Block([FromQuery]string id)
    {
        return HandleResult(await Mediator.Send(new BlockUserCommand(id)));
    }


    [HttpDelete("DeleteUser/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteUserCommand(id)));
    }
}
