using Application.Block.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.IdentityManagement.User.Queries.GetUsersByName;

public class GetUsersByNameQueryHandler : IRequestHandler<GetUsersByNameQuery, Result<List<SearchedUserViewModel>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetUsersByNameQueryHandler> _logger;

    public GetUsersByNameQueryHandler(IUserRepository userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IBlockRepository blockRepository,
        ILogger<GetUsersByNameQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<List<SearchedUserViewModel>>> Handle(GetUsersByNameQuery request, CancellationToken cancellationToken)
    {
        //var userList = _userRepository.GetUserNameByFuzzySearch(request.Name);
           
        var userList = _userRepository.GetUserNameWithRegex(request.Name);

        var users = new List<SearchedUserViewModel>();

        foreach (var user in userList)
        {
            var userEntity = _mapper.Map<SearchedUserViewModel>(user);

            var currentUserIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == _currentUserService.UserId && x.BlockedById == user.Id);

            var userIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedById == _currentUserService.UserId && x.BlockedId == user.Id);

            if (currentUserIsBlocked == null && userIsBlocked == null && (user.Email !="admin@gmail.com"))
            {
                users.Add(userEntity);
            }
        }

        return Result<List<SearchedUserViewModel>>.Success(users);
    }
}
