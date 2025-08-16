using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;
using ChurchAttendanceSystem.Infrastructure;

namespace ChurchAttendanceSystem.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDb _db;
    private readonly IEncryptionService _encryption;

    public AttendanceService(AppDb db, IEncryptionService encryption)
    {
        _db = db;
        _encryption = encryption;
    }

    public async Task<ServiceResult<AttendanceRecordDto>> CheckInAsync(CheckInDto dto, Guid staffId)
    {
        if (!Guid.TryParse(dto.ChildId, out var childId))
            return ServiceResult<AttendanceRecordDto>.Failure("Invalid child ID");

        var child = await _db.Children.Include(c => c.Parent).FirstOrDefaultAsync(c => c.Id == childId);
        if (child == null)
            return ServiceResult<AttendanceRecordDto>.Failure("Child not found");

        // Check if child is already checked in today
        var today = DateTime.UtcNow.Date;
        var existingRecord = await _db.AttendanceRecords
            .FirstOrDefaultAsync(r => r.ChildId == childId && 
                                    r.CheckInTime.Date == today && 
                                    r.CheckOutTime == null);

        if (existingRecord != null)
            return ServiceResult<AttendanceRecordDto>.Failure("Child is already checked in");

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            ChildId = childId,
            ParentId = child.ParentId,
            CheckInTime = DateTime.UtcNow,
            CheckInNotes = dto.Notes,
            CheckInStaffId = staffId
        };

        _db.AttendanceRecords.Add(record);
        await _db.SaveChangesAsync();

        var recordDto = new AttendanceRecordDto(
            record.Id.ToString(),
            record.ChildId.ToString(),
            record.ParentId.ToString(),
            record.CheckInTime,
            record.CheckOutTime,
            record.CheckInStaffId.ToString(),
            record.CheckOutStaffId?.ToString(),
            record.CheckInNotes,
            record.CheckInTime.ToString("yyyy-MM-dd")
        );

        return ServiceResult<AttendanceRecordDto>.Success(recordDto);
    }

    public async Task<ServiceResult<AttendanceRecordDto>> CheckOutAsync(CheckOutDto dto, Guid staffId)
    {
        if (!Guid.TryParse(dto.RecordId, out var recordId))
            return ServiceResult<AttendanceRecordDto>.Failure("Invalid record ID");

        var record = await _db.AttendanceRecords.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record == null)
            return ServiceResult<AttendanceRecordDto>.Failure("Attendance record not found");

        if (record.CheckOutTime != null)
            return ServiceResult<AttendanceRecordDto>.Failure("Child is already checked out");

        record.CheckOutTime = DateTime.UtcNow;
        record.CheckOutNotes = dto.Notes;
        record.CheckOutStaffId = staffId;

        await _db.SaveChangesAsync();

        var recordDto = new AttendanceRecordDto(
            record.Id.ToString(),
            record.ChildId.ToString(),
            record.ParentId.ToString(),
            record.CheckInTime,
            record.CheckOutTime,
            record.CheckInStaffId.ToString(),
            record.CheckOutStaffId?.ToString(),
            record.CheckOutNotes,
            record.CheckInTime.ToString("yyyy-MM-dd")
        );

        return ServiceResult<AttendanceRecordDto>.Success(recordDto);
    }

    public async Task<ServiceResult<List<AttendanceRecordDto>>> GetAttendanceRecordsAsync()
    {
        var records = await _db.AttendanceRecords
            .OrderByDescending(r => r.CheckInTime)
            .Select(r => new AttendanceRecordDto(
                r.Id.ToString(),
                r.ChildId.ToString(),
                r.ParentId.ToString(),
                r.CheckInTime,
                r.CheckOutTime,
                r.CheckInStaffId.ToString(),
                r.CheckOutStaffId != null ? r.CheckOutStaffId.ToString() : null,
                r.CheckInNotes ?? r.CheckOutNotes,
                r.CheckInTime.ToString("yyyy-MM-dd")
            ))
            .ToListAsync();

        return ServiceResult<List<AttendanceRecordDto>>.Success(records);
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
        var logsData = await _db.AttendanceLogs
            .Where(x => x.SessionDate == date)
            .Include(x => x.Child).ThenInclude(c => c.Parent)
            .OrderBy(x => x.TimestampUtc)
            .ToListAsync();

        var logs = logsData.Select(x => new AttendanceReportDto(
            x.Action,
            x.TimestampUtc,
            $"{_encryption.Decrypt(x.Child.FirstName)} {_encryption.Decrypt(x.Child.LastName)}",
            $"{x.Child.Parent.FirstName} {x.Child.Parent.LastName}",
            "General" // Default group since we removed Group field
        )).ToList();

        return ServiceResult<List<AttendanceReportDto>>.Success(logs);
    }
}