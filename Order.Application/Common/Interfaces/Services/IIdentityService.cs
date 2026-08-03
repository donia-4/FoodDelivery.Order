namespace Order.Application.Common.Interfaces.Services;

public interface IIdentityService
{
    Task<(string Name, string Phone)?> GetUserAsync(Guid userId, CancellationToken ct = default);
}