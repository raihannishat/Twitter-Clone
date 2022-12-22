using Application.Common.Interfaces;

namespace WebApi.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly ILogger<UserController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(ILogger<UserController> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    [HttpPost, Route("sign-up")]
    public async Task<IActionResult> Registration(SignUpCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost, Route("sign-in")]
    public async Task<IActionResult> Login(SignInCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost, Route("sign-out")]
    public async Task<IActionResult> Logout(string userId)
    {
        try
        {
            var res = _currentUserService.ClearSession();

            return Ok(res);
        }
        catch
        {
            return BadRequest();
        }
        
    }

    [HttpPost, Route("verify-account")]
    public async Task<IActionResult> VerifyAccount([FromQuery]string token)
    {
        return HandleResult(await Mediator.Send(new VerifyAccountCommand(token)));
    }

    [HttpPost, Route("forget-password")]
    public async Task<IActionResult> ForgetPassword(string email)
    {
        return HandleResult(await Mediator.Send(new ForgetPasswordCommand(email)));
    }

    [HttpPost, Route("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost, Route("refreshToken")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}
