using Application.Tweets.Shared.Interfaces;
using Application.Tweets.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Tweets.Queries.GetHashtag;

public class GetHashtagQueryHandler : IRequestHandler<GetHashtagQuery, Result<List<HashtagVM>>>
{
    private readonly ISearchRepository _searchRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetHashtagQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetHashtagQueryHandler(ISearchRepository searchRepository,
        IMapper mapper,
        ILogger<GetHashtagQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _searchRepository = searchRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<HashtagVM>>> Handle(GetHashtagQuery request, CancellationToken cancellationToken)
    {
     
        //var tagNames = _searchRepository.GetHashtagByFuzzySearch(request.TagName);

        var tagNames = await _searchRepository.GetHashtagWithRegex(request.TagName);

        if (tagNames == null)
        {
            return Result<List<HashtagVM>>.Success(new List<HashtagVM>());
        }

        var taglist = new List<HashtagVM>();

        foreach (var tag in tagNames)
        {
            taglist.Add(_mapper.Map<HashtagVM>(tag));
        }

        return Result<List<HashtagVM>>.Success(taglist);
    }
}
