﻿namespace Application.IdentityManagement.Auth.Commands.ResetPassword;

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
