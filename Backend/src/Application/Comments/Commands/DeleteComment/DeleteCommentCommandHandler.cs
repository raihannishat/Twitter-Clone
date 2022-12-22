using Application.Comments.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<CommentResponse>>
{
    private readonly ITweetRepository _tweetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<DeleteCommentCommandHandler> _logger;
    private readonly IMapper _mapper;

    public DeleteCommentCommandHandler(ITweetRepository tweetCommentRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ICommentRepository commentRepository,
        ILogger<DeleteCommentCommandHandler> logger)
    {
        _tweetRepository = tweetCommentRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<Result<CommentResponse>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var tweet = await _tweetRepository.FindByIdAsync(request.TweetId);

        if (tweet == null)
        {
            return null!;
        }

        var comment = await _commentRepository.FindByIdAsync(request.CommentId);

        if (comment.UserId != _currentUserService.UserId)
        {
            return null!;
        }

        await _commentRepository.DeleteByIdAsync(comment.Id);

        tweet.Comments--;

        await _tweetRepository.ReplaceOneAsync(tweet);

        var commentResponse = new CommentResponse()
        {
            TotalComments = tweet.Comments
        };

        return Result<CommentResponse>.Success(commentResponse);
    }
}
