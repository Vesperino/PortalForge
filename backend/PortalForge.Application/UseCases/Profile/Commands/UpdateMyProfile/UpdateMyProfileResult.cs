namespace PortalForge.Application.UseCases.Profile.Commands.UpdateMyProfile;

public class UpdateMyProfileResult
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
