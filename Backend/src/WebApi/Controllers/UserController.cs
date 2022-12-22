using Application.IdentityManagement.Shared.Interfaces;
using Application.IdentityManagement.User.Queries.GetUserEmail;
using Application.IdentityManagement.User.Queries.GetUsersByName;
using Bogus;
using Domain.Entities;

namespace WebApi.Controllers;

[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController : BaseApiController
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    public UserController(ILogger<UserController> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> Get([FromQuery] PaginationQueryRequest query)
    {
        return HandleResult(await Mediator.Send(new GetUsersQuery(query)));
    }

    [HttpPost("Create")]


    public async Task<IActionResult> Create(CreateUserCommand createUser)
    {
        return HandleResult(await Mediator.Send(createUser));
    }

    [HttpGet("GetById/{id:length(24)}")]
    public async Task<IActionResult> GetById(string id)
    {
        return HandleResult(await Mediator.Send(new GetUserByIdQuery(id)));
    }

    [HttpGet("GetByEmail/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        return HandleResult(await Mediator.Send(new GetUserByEmailQuery(email)));
    }

    [AllowAnonymous]
    [HttpGet("GetEmail/{email}")]
    public async Task<IActionResult> GetEmail(string email)
    {
        return HandleResult(await Mediator.Send(new GetUserEmailQuery(email)));
    }

    [HttpPut("Update/{id:length(24)}")]
    public async Task<IActionResult> Update(string id, UpdateUserCommand updateUser)
    {
        updateUser.Id = id;
        return HandleResult(await Mediator.Send(updateUser));
    }

    [HttpDelete("Delete/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteUserCommand(id)));
    }

    [HttpGet("Profile/{id}")]
    public async Task<IActionResult> GetUserProfile(string id)
    {
        return HandleResult(await Mediator.Send(new GetUserByIdQuery(id)));
    }

    [HttpGet("user_name")]
    public async Task<IActionResult> GetUserNames([FromQuery]string keyword)
    {
        return HandleResult(await Mediator.Send(new GetUsersByNameQuery(keyword)));
    }
}
