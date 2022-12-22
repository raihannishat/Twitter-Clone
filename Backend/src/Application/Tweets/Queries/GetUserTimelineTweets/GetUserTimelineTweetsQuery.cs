using Application.Tweets.Shared.Models;

namespace Application.Tweets.Queries.GetUserTimelineTweets;

public class GetUserTimelineTweetsQuery : IRequest<Result<List<TweetViewModel>>>
{
    public PaginationQueryRequest PageQuery { get; set; }

    public GetUserTimelineTweetsQuery(PaginationQueryRequest paginationQuery) => PageQuery = paginationQuery;
}
