using Application.Block.Shared.Interfaces;
using Application.Follows.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.IdentityManagement.User.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<List<SearchedUserViewModel>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetUsersQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFollowRepository _followRepository;

    public GetUsersQueryHandler(IUserRepository userRepository,
        IMapper mapper, ICurrentUserService currentUserService,
        IFollowRepository followRepository,
        IBlockRepository blockRepository,
        ILogger<GetUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _followRepository = followRepository;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<List<SearchedUserViewModel>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserService.UserId;

        //return all except admin, CurrentUser and currentuser block list users

        var userList = _userRepository.FindByMatchWithPagination(x =>
            x.Id != currentUser && x.Email != "admin@gmail.com", request.PageQuery.PageNumber); ;

        var users = new List<Domain.Entities.User>();

        foreach (var user in userList)
        {
            var currentUserIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == currentUser && x.BlockedById == user.Id);

            var userIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedById == currentUser && x.BlockedId == user.Id);

            //var isFollowing = await _followRepository.FindOneByMatchAsync(x =>
            //    x.FollowerId == currentUser && x.FollowedId == user.Id);

            //if (currentUserIsBlocked == null && userIsBlocked == null && isFollowing == null)
            //{
            //    users.Add(user);
            //}

            if (currentUserIsBlocked == null && userIsBlocked == null)
            {
                users.Add(user);
            }
        }

        var resultedUser = new List<SearchedUserViewModel>();

        foreach (var user in users)
        {
            var isFollowing = await _followRepository.FindOneByMatchAsync(x =>
                x.FollowerId == currentUser && x.FollowedId == user.Id);

            var userEntity = new SearchedUserViewModel()
            {
                Id = user.Id,
                Name = user.Name,
                Image = user.Image,
                IsCurrentUser = false,
                IsBlocked = false
            };

            if (isFollowing != null) userEntity.IsFollowing = true;

            resultedUser.Add(userEntity);
        }

        return Result<List<SearchedUserViewModel>>.Success(resultedUser);
    }
}
