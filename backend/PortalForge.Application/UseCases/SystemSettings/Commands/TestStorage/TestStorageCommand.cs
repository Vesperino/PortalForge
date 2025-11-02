using MediatR;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.SystemSettings.Commands.TestStorage;

public class TestStorageCommand : IRequest<StorageTestResult>
{
}
