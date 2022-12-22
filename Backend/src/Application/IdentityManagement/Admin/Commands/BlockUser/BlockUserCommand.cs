namespace Application.IdentityManagement.Admin.Commands.BlockUser;

public class BlockUserCommand : IRequest<Result<AdminBlockResponse>>
{
    public string Id { get; set; } = string.Empty;

    public BlockUserCommand(string id) => Id = id;
}
