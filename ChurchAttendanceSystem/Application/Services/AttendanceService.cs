using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;
using ChurchAttendanceSystem.Infrastructure;

namespace ChurchAttendanceSystem.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDb _db;

    public AttendanceService(AppDb db)
    {
        _db = db;
    }

    public async Task<ServiceResult<int>> MarkAttendanceAsync(MarkAttendanceDto dto, Guid staffId)
    {
        if (dto.Action is not ("CheckIn" or "CheckOut"))
            return ServiceResult<int>.Failure("Invalid action. Must be CheckIn or CheckOut");

        var now = DateTime.UtcNow;
        var session = now.ToString("yyyy-MM-dd");

        var entries = dto.ChildIds.Select(id => new AttendanceLog
        {
            Id = Guid.NewGuid(),
            ChildId = id,
            Action = dto.Action,
            TimestampUtc = now,
            HandledByUserId = staffId,
            SessionDate = session
        });

        await _db.AttendanceLogs.AddRangeAsync(entries);
        await _db.SaveChangesAsync();

        return ServiceResult<int>.Success(dto.ChildIds.Count);
    }

    public async Task<ServiceResult<List<AttendanceReportDto>>> GetSessionReportAsync(string date)
    {
        var logs = await _db.AttendanceLogs
            .Where(x => x.SessionDate == date)
            .Include(x => x.Child).ThenInclude(c => c.Parent)
            .OrderBy(x => x.TimestampUtc)
            .Select(x => new AttendanceReportDto(
                x.Action,
                x.TimestampUtc,
                x.Child.FullName,
                x.Child.Parent.FullName,
                x.Child.Group
            ))
            .ToListAsync();

        return ServiceResult<List<AttendanceReportDto>>.Success(logs);
    }
}