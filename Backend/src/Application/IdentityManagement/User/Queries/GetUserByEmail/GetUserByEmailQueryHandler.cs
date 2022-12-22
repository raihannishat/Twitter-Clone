using Microsoft.Extensions.Logging;

namespace Application.IdentityManagement.User.Queries.GetUserByEmail;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserViewModel>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserByEmailQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetUserByEmailQueryHandler(IUserRepository userRepository,
        IMapper mapper,
        ILogger<GetUserByEmailQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserViewModel>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var entity = await _userRepository.FindOneAsync(x => x.Email == request.Email);

        if (entity == null)
        {
            return null!;
        }

        var userViewModel = _mapper.Map<UserViewModel>(entity);

        return Result<UserViewModel>.Success(userViewModel);
    }
}
