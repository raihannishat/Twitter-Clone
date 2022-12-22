using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRedisTokenService _redisTokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<IdentityService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly string _secretKey;

    public IdentityService(IUserRepository userRepository,
        IAuthRepository authRepository,
        IMapper mapper,
        IConfiguration configuration,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        ILogger<IdentityService> logger,
        ICurrentUserService currentUserService,
        IRedisTokenService redisTokenService)
    {
        _userRepository = userRepository;
        _authRepository = authRepository;
        _mapper = mapper;
        _secretKey = configuration.GetSection("JwtSettings:Secret").Value;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _logger = logger;
        _currentUserService = currentUserService;
        _redisTokenService = redisTokenService;
    }

    public async Task<Result<Unit>> Registration(SignUpCommand request)
    {
        var userEmailExist = await _userRepository.FindOneByMatchAsync(x => x.Email == request.Email);

        if (userEmailExist != null)
        {
            return Result<Unit>.Failure("Email is already used");
        }

        request.Password = SHA_256.ComputeHash(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            VerificationToken = CreateRandomString(8),
            Role = "User"
        };

        await _userRepository.InsertOneAsync(user);

        var name = new List<string> { request.Name };

        var emailDto = new EmailDto
        {
            Subject = "Twitter Account Confirmation",
            Message = $"<h2> Hey {user.Name},</h2>"
            + "<h3>Welcome to Twitter. (DIU_Raizor)</h3>"
            + "<p>Your Account Verfication Code </p>"
                + "<h2>" + user.VerificationToken + "</h2>"
                + "<p>For more information. Contact with us </p> </br>"
            + "<p>Asif Abdullah [01755808860], Raihan Nishat [01710512211] </p> </br>"
            + "<p>Department of Software Engineering.</p> </br>"
            + "<p>Daffodil Internation University.</p> </br>"
        };

        _emailService.SendMail(request.Email, emailDto.Subject, emailDto.Message);

        _logger.LogInformation("Registration", $"{request.Email} - is successfully registered");

        return Result<Unit>.Success(Unit.Value);

    }

    public async Task<Result<AuthResult>> Login(SignInCommand request)
    {
        var password = SHA_256.ComputeHash(request.Password);

        var existingUser = await _userRepository.FindOneByMatchAsync(x =>
            x.Email == request.Email && x.Password == password);

        if (existingUser == null)
        {
            return Result<AuthResult>.Failure("Invalid email/password");
        }

        if (existingUser.VerifiedAt == null)
        {
            return Result<AuthResult>.Failure("Your account is not verified");
        }

        if (existingUser.IsBlockedByAdmin)
        {
            return Result<AuthResult>.Failure("You are blocked. Please Contact with Admin");
        }

        var tokenResult = _jwtTokenService.GenerateToken(existingUser, _secretKey);

        //await _authRepository.InsertOneAsync(_mapper.Map<RefreshToken>(tokenResult.RefreshToken));

        await _redisTokenService.SetKeywithTTL(tokenResult.RefreshToken.Token, tokenResult.AccessToken, 1);

        return Result<AuthResult>.Success(new AuthResult
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken.Token,
            User = _mapper.Map<UserViewModel>(existingUser)
        });

    }

    public async Task<Result<AuthResult>> RefreshToken(RefreshTokenCommand request)
    {
        var jwtToken = _jwtTokenService.GetPrincipalFromToken(request.AccessToken, _secretKey);

        var storedRefreshToken = await _redisTokenService.GetKey(request.RefreshToken);

        if (storedRefreshToken.IsNullOrEmpty())
        {
            return Result<AuthResult>.Failure("This refresh token doesn't exist");
        }

        //finding user id
        var id = jwtToken.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;

        var user = await _userRepository.FindByIdAsync(id);

        var tokenResult = _jwtTokenService.GenerateToken(user, _secretKey);

        //await _authRepository.InsertOneAsync(_mapper.Map<RefreshToken>(tokenResult.RefreshToken));

        await _redisTokenService.UpdateKey(request.RefreshToken, tokenResult.AccessToken);

        return Result<AuthResult>.Success(new AuthResult
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = request.RefreshToken,
            User = _mapper.Map<UserViewModel>(user)
        });
    }

    public async Task<Result<Unit>> VerifyAccount(string token)
    {
        var user = await _userRepository.FindOneAsync(x => x.VerificationToken == token);

        if (user == null)
        {
            return Result<Unit>.Failure("Invalid token");
        }

        user.VerifiedAt = DateTime.Now;

        await _userRepository.ReplaceOneAsync(user);

        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> ForgetPassword(string email)
    {
        var user = await _userRepository.FindOneByMatchAsync(x => x.Email == email);

        if (user == null)
        {
            return Result<Unit>.Failure("email doesn't exit");
        }

        if (user.VerifiedAt == null)
        {
            return Result<Unit>.Failure("Your account is not verified yet");
        }

        user.PasswordResetToken = CreateRandomString(8);
        user.ResetTokenExpires = DateTime.Now.AddDays(1);

        await _userRepository.ReplaceOneAsync(user);

        var emailDto = new EmailDto
        {
            Subject = "Password reset",
            Message = $"<h2>Hey {user.Name},</h2>" +
            "<p> We received a request to reset the password on your Twitter Account</p>"
            + $"<h2>{user.PasswordResetToken} </h2>" +
            "<p>Enter this code to complete the reset </p> </br"
                + "<p>For more information. Contact with us </p> </br>"
            + "<p>Asif Abdullah [01755808860], Raihan Nishat [01710512211] </p> </br>"
            + "<p>Department of Software Engineering.</p> </br>"
            + "<p>Daffodil Internation University.</p> </br>"
        };

        _emailService.SendMail(user.Email, emailDto.Subject, emailDto.Message);

        return Result<Unit>.Success(Unit.Value);

    }

    public async Task<Result<Unit>> ResetPassword(ResetPasswordDto request)
    {
        var user = await _userRepository
            .FindOneByMatchAsync(x => x.PasswordResetToken == request.Token);

        if (user == null || user.ResetTokenExpires < DateTime.Now)
        {
            return Result<Unit>.Failure("Invalid token");
        }

        user.Password = SHA_256.ComputeHash(request.Password);
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;

        await _userRepository.ReplaceOneAsync(user);

        return Result<Unit>.Success(Unit.Value);

    }

    private static string CreateRandomString(int length)
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";

        return new string(Enumerable.Repeat(chars, length)
            .Select(x => x[random.Next(x.Length)]).ToArray());
    }
}
