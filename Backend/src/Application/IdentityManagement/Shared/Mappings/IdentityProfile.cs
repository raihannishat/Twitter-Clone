namespace Application.IdentityManagement.Shared.Mappings;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<Domain.Entities.User, UserViewModel>().ReverseMap();
        CreateMap<Domain.Entities.User, UpdateUserCommand>().ReverseMap();
        CreateMap<Domain.Entities.User, CreateUserCommand>().ReverseMap();
        CreateMap<Domain.Entities.User, SignUpCommand>().ReverseMap();
        CreateMap<Domain.Entities.User, SearchedUserViewModel>().ReverseMap();
        CreateMap<RefreshToken, RefreshTokenDTO>().ReverseMap();
        CreateMap<ResetPasswordCommand, ResetPasswordDto>().ReverseMap();
    }
}
