namespace Application.UnitTests.Comment;

public class CreateCommentCommandValidatorTest
{
    private readonly AbstractValidator<CreateCommentCommand> _validator;

    public CreateCommentCommandValidatorTest()
    {
        _validator = new CreateCommentCommandValidator();
    }

    public void ShouldHaveError_WhenNameisEmpty()
    {
        var model = new CreateCommentCommand { Content = string.Empty };

        // var result = _validator.Should().
    }
}
