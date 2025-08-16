namespace ChurchAttendanceSystem.Application.Interfaces;

public interface ITokenService
{
    Task BlacklistTokenAsync(string token);
    Task<bool> IsTokenBlacklistedAsync(string token);
}