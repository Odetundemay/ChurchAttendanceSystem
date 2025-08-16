using ChurchAttendanceSystem.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ChurchAttendanceSystem.Application.Services;

public class TokenService : ITokenService
{
    private readonly IMemoryCache _cache;

    public TokenService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task BlacklistTokenAsync(string token)
    {
        _cache.Set($"blacklist_{token}", true, TimeSpan.FromDays(7));
        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return Task.FromResult(_cache.TryGetValue($"blacklist_{token}", out _));
    }
}