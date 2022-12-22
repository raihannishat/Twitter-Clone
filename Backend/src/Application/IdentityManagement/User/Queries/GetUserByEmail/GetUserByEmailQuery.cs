﻿namespace Application.IdentityManagement.User.Queries.GetUserByEmail;

public class GetUserByEmailQuery : IRequest<Result<UserViewModel>>
{
    public string Email { get; set; } = string.Empty;

    public GetUserByEmailQuery(string email) => Email = email;
}
