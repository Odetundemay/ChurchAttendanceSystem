using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Application.Interfaces;

public interface IAttendanceService
{
    Task<ServiceResult<int>> MarkAttendanceAsync(MarkAttendanceDto dto, Guid staffId);
    Task<ServiceResult<List<AttendanceReportDto>>> GetSessionReportAsync(string date);
}