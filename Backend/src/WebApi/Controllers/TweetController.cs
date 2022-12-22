using Application.Tweets.Commands.CreateTweet;
using Application.Tweets.Commands.DeleteTweet;
using Application.Tweets.Commands.UpdateTweet;
using Application.Tweets.Queries.GetUserTimelineTweets;
//using Infrastructure.Persistence.RedisCaching;
using Application.Common.Interfaces;
using Application.Tweets.Queries.GetHomeTimelineTweets;
using Application.Tweets.Queries.GetTweetById;
using Application.Tweets.Queries.GetHashtagTweets;
using Application.Tweets.Queries.GetTrendingHashtag;

namespace WebApi.Controllers;
[ApiController, Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TweetController : BaseApiController
{
    private readonly ILogger<TweetController> _logger;
    public TweetController(ILogger<TweetController> logger)
    {
        _logger = logger;
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById([FromQuery]string tweetId, [FromQuery]string tweetOwnerId)
    {
        return HandleResult(await Mediator.Send(new GetTweetByIdQuery(tweetId, tweetOwnerId)));
    }

    [HttpGet("Gethashtags")]
    public async Task<IActionResult> GetHashtags([FromQuery] PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetTrendingHashtagQuery(queryRequest)));
    }

    [HttpGet("GethashtagTweets")]
    public async Task<IActionResult> GetHashtagTweets([FromQuery]PaginationQueryRequest queryRequest)
    {
        queryRequest.Keyword = '#' + queryRequest.Keyword;

        return HandleResult(await Mediator.Send(new GetHashtagTweetsQuery(queryRequest)));
    }


    [HttpPost("Create")]
    public async Task<IActionResult> Create(CreateTweetCommand createTweet)
    {
        return HandleResult(await Mediator.Send(createTweet));
    }


    [HttpPut("Update/{id:length(24)}")]
    public async Task<IActionResult> Update(string id, UpdateTweetCommand updateTweet)
    {
        updateTweet.Id = id;
        return HandleResult(await Mediator.Send(updateTweet));
    }

    [HttpDelete("Delete/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteTweetCommand(id)));
    }


    [HttpGet("home-timeline")]
    public async Task<IActionResult> GetHomeTimeline([FromQuery]PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetHomeTimelineQuery(queryRequest)));
    }

    [HttpGet("user-timeline")]
    public async Task<IActionResult> GetUserTimeline([FromQuery]PaginationQueryRequest queryRequest)
    {
        return HandleResult(await Mediator.Send(new GetUserTimelineTweetsQuery(queryRequest)));
    }
}
