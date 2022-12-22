using Microsoft.Extensions.Configuration;

namespace Infrastructure.UnitTests.Identity;

public class IdentityServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRedisTokenService> _redisTokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<IdentityService>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IdentityService _sut;

    public IdentityServiceTests()
    {
        _userRepositoryMock = new();
        _authRepositoryMock = new();
        _jwtTokenServiceMock = new();
        _redisTokenServiceMock = new();
        _emailServiceMock = new();
        _loggerMock = new();
        _currentUserServiceMock = new();
        _mapperMock = new();
        _configurationMock = new();

        _sut = new IdentityService(
            _userRepositoryMock.Object,
            _authRepositoryMock.Object,
            _mapperMock.Object,
            _configurationMock.Object,
            _jwtTokenServiceMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _redisTokenServiceMock.Object);
    }
}
