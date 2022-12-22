using Application.Block.Shared.Interfaces;
using Application.Comments.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Comments.Queries.GetComments;

public class GetCommentQueryHandler : IRequestHandler<GetCommentQuery, Result<List<CommentViewModel>>>
{
    private readonly IMapper _mapper;
    private readonly ITweetRepository _tweetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetCommentQueryHandler> _logger;

    public GetCommentQueryHandler(IMapper mapper,
        ITweetRepository tweetRepository,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        ICommentRepository commentRepository,
        IBlockRepository blockRepository,
        ILogger<GetCommentQueryHandler> logger)
    {
        _mapper = mapper;
        _tweetRepository = tweetRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _commentRepository = commentRepository;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<List<CommentViewModel>>> Handle(GetCommentQuery request, CancellationToken cancellationToken)
    {
        var tweet = await _tweetRepository.FindByIdAsync(request.TweetId);

        if (tweet == null)
        {
            return null!;
        }

        var commentObj = await _commentRepository.GetCommentByDescendingTime(x =>
            x.TweetId == request.TweetId, request.PageQuery.PageNumber);

        var comments = new List<Comment>();

        foreach (var comment in commentObj)
        {
            var currentUserIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == _currentUserService.UserId && x.BlockedById == comment.UserId);

            var userIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedById == _currentUserService.UserId && x.BlockedId == comment.UserId);

            if (currentUserIsBlocked == null && userIsBlocked == null)
            {
                comments.Add(comment);
            }
        }

        var userCollection = _userRepository.GetCollection();

        var commentCollection = from comment in comments
                                join
                                user in userCollection on comment.UserId equals user.Id
                                select new CommentViewModel
                                {
                                    Id = comment.Id,
                                    Content = comment.Content,
                                    TweetId = comment.TweetId,
                                    UserId = user.Id,
                                    UserName = user.Name,
                                    Image = user.Image,
                                    CreatedTime = comment.CreatedTime
                                };

        var commentList = new List<CommentViewModel>();

        foreach (var comment in commentCollection)
        {
            if (comment.UserId == _currentUserService.UserId)
            {
                comment.CanDelete = true;
            }

            commentList.Add(comment);
        }

        return Result<List<CommentViewModel>>.Success(commentList.ToList());
    }
}
