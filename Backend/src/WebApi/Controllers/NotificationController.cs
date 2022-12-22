using Application.Notifications.Queries;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationController : BaseApiController
{

    [HttpGet("getNotifications")]
    public async Task<IActionResult> GetAllNotification([FromQuery]PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetNotificationQuery(queryRequest)));
    }
}