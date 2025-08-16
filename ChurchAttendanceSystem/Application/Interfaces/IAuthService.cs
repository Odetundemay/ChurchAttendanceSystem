using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Application.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto dto);
    Task<ServiceResult<Guid>> RegisterStaffAsync(RegisterStaffDto dto);
}