using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Application.Interfaces;

public interface IAttendanceService
{
    Task<ServiceResult<AttendanceRecordDto>> CheckInAsync(CheckInDto dto, Guid staffId);
    Task<ServiceResult<AttendanceRecordDto>> CheckOutAsync(CheckOutDto dto, Guid staffId);
    Task<ServiceResult<List<AttendanceRecordDto>>> GetAttendanceRecordsAsync();
    Task<ServiceResult<int>> MarkAttendanceAsync(MarkAttendanceDto dto, Guid staffId);
    Task<ServiceResult<List<AttendanceReportDto>>> GetSessionReportAsync(string date);
}