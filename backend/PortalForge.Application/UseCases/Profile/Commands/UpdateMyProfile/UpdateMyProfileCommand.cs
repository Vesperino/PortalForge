using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Profile.Commands.UpdateMyProfile;

public class UpdateMyProfileCommand : IRequest<UpdateMyProfileResult>, ITransactionalRequest
{
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
