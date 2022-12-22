﻿using Microsoft.Extensions.Logging;

namespace Application.IdentityManagement.User.Queries.GetUserEmail;

public class GetUserEmailQueryHandler : IRequestHandler<GetUserEmailQuery, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserEmailQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetUserEmailQueryHandler(IUserRepository userRepository,
        ILogger<GetUserEmailQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(GetUserEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return null!;
        }

        var emailExit = await _userRepository.FindOneByMatchAsync(x => x.Email == request.Email);

        var result = true;

        if (emailExit == null)
        {
            result = false;
        }

        return Result<bool>.Success(result);
    }
}
