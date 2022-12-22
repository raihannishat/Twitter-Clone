using Application.Tweets.Shared.Interfaces;
using Application.Tweets.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Tweets.Queries.GetTrendingHashtag;

public class GetTrendingHashtagQueryHandler : IRequestHandler<GetTrendingHashtagQuery, Result<List<HashtagVM>>>
{
    private readonly ISearchRepository _searchRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTrendingHashtagQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetTrendingHashtagQueryHandler(ISearchRepository searchRepository,
        IMapper mapper,
        ILogger<GetTrendingHashtagQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _searchRepository = searchRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<HashtagVM>>> Handle(GetTrendingHashtagQuery request, CancellationToken cancellationToken)
    {

        var tagName = request.PageQuery.Keyword;

        var hashtags = await _searchRepository.GetHashtagWithPagination(request.PageQuery.PageNumber);

        var hashtagsList = new List<HashtagVM>();

        foreach (var hashtag in hashtags)
        {
            hashtagsList.Add(_mapper.Map<HashtagVM>(hashtag));
        }

        return Result<List<HashtagVM>>.Success(hashtagsList);
    }
}
