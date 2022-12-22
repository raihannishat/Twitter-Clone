﻿namespace Application.IdentityManagement.User.Queries.GetUsersByName;

public class GetUsersByNameQuery : IRequest<Result<List<SearchedUserViewModel>>>
{
    public string Name { get; set; } = string.Empty;
    public GetUsersByNameQuery(string name)
    {
        Name = name;
    }
}
