using Microsoft.Extensions.Logging;

namespace Application.IdentityManagement.Admin.Queries;
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<UserViewModel>>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAllUsersQueryHandler(IUserRepository userRepository,
        IMapper mapper, ICurrentUserService currentUserService,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<UserViewModel>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var userList = _userRepository.FindByMatchWithPagination(x =>
            x.Email != "admin@gmail.com", request.PageQuery.PageNumber); ;

        var users = new List<UserViewModel>();

        foreach (var user in userList)
        {
            users.Add(_mapper.Map<UserViewModel>(user));
        }

        return Result<List<UserViewModel>>.Success(users);
    }
}
