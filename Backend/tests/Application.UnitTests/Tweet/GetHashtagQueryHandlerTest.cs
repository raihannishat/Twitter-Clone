using Application.Tweets.Queries.GetHashtag;

namespace Application.UnitTests.Tweet;

public class GetHashtagQueryHandlerTest
{
    private readonly Mock<ISearchRepository> _searchRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetHashtagQueryHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetHashtagQueryHandler _sut;

    public GetHashtagQueryHandlerTest()
    {
        _searchRepositoryMock = new();
        _mapperMock = new();
        _loggerMock = new();
        _currentUserServiceMock = new();

        _sut = new GetHashtagQueryHandler(
            _searchRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public void ShouldReturnEmptyHashTagList_WhenTagDoesNotExsist()
    {
        // Arrange
        var tagName = "Learnathon";

        var respons = new List<HashtagVM>();

        _searchRepositoryMock.Setup(x => x.GetHashtagWithRegex(tagName))
            .ReturnsAsync(() => null!);

        var query = new GetHashtagQuery(tagName);

        var cts = new CancellationTokenSource();

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(respons);
    }

    [Fact]
    public void ShouldReturnHashTagList_WhenTagNameIsExsist()
    {
        // Arrange
        var tagName = "Learnathon";

        var tagList = new List<Search>
        {
            new Search { },
            new Search { },
            new Search { }
        };

        var respons = new List<HashtagVM>
        {
            new HashtagVM { },
            new HashtagVM { },
            new HashtagVM { }
        };

        _searchRepositoryMock.Setup(x => x.GetHashtagWithRegex(tagName))
            .ReturnsAsync(tagList);

        _mapperMock.Setup(x => x.Map<HashtagVM>(It.IsAny<Search>()))
            .Returns(new HashtagVM())
            .Verifiable();

        var query = new GetHashtagQuery(tagName);

        var cts = new CancellationTokenSource();

        // Act
        var result = _sut.Handle(query, cts.Token).Result.Value;

        // Assert
        result.Should().BeEquivalentTo(respons);

        _mapperMock.VerifyAll();
    }
}
