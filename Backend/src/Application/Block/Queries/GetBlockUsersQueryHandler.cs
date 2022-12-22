using Application.Block.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Block.Queries;

public class GetBlockUsersQueryHandler : IRequestHandler<GetBlockUsersQuery, Result<List<SearchedUserViewModel>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetBlockUsersQueryHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetBlockUsersQueryHandler(ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IMapper mapper,
        IBlockRepository blockRepository,
        ILogger<GetBlockUsersQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _mapper = mapper;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<List<SearchedUserViewModel>>> Handle(GetBlockUsersQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var blockedUsersObj = _blockRepository.FindByMatchWithPagination(x =>
            x.BlockedById == currentUserId, request.PageQuery.PageNumber);

        var userCollection = _userRepository.GetCollection();

        var blockedUsers = from blockuser in blockedUsersObj
                            join user in userCollection on blockuser.BlockedId equals user.Id
                            select new SearchedUserViewModel
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Image = user.Image,
                                IsBlocked = true
                            };

        return Result<List<SearchedUserViewModel>>.Success(blockedUsers.ToList());
        
    }
}
