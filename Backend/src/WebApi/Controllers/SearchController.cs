using Application.Common.Interfaces;
using Application.IdentityManagement.User.Queries.GetUsersByName;
using Application.Tweets.Queries.GetHashtag;
using Application.Tweets.Queries.GetHashtagTweets;

namespace WebApi.Controllers;
[ApiController, Route("api/[controller]")]

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SearchController : BaseApiController
{
    private readonly ILogger<SearchController> _logger;
    public SearchController(ILogger<SearchController> logger)
    {
        _logger = logger;
    }

    [HttpGet("hashtag")]
    public async Task<IActionResult> GetSuggestion([FromQuery] string hashtag)
    {
        var tag = '#' + hashtag;

        return HandleResult(await Mediator.Send(new GetHashtagQuery(tag)));
    }

    [HttpGet("name")]
    public async Task<IActionResult> GetPosts([FromQuery] string name)
    {
        return HandleResult(await Mediator.Send(new GetUsersByNameQuery(name)));

        //if (queryRequest.Keyword!.StartsWith('#'))
        //{
        //    return HandleResult(await Mediator.Send(new GetHashtagTweetsQuery(queryRequest)));
        //}
        //else
        //{
        //    return HandleResult(await Mediator.Send(new GetUsersByNameQuery(queryRequest.Keyword)));
        //}
    }

}
