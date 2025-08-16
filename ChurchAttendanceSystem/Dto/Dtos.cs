
using System.ComponentModel.DataAnnotations;

namespace ChurchAttendanceSystem.Dto;

public record RegisterStaffDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email, 
    [Required, MinLength(8)] string Password, 
    string Role = "Staff");

public record LoginDto(
    [Required, EmailAddress] string Email, 
    [Required] string Password);

public record CreateParentDto(
    [Required] string FirstName,
    [Required] string LastName,
    string Phone, 
    [EmailAddress] string Email);

public record CreateChildDto(
    [Required] string FirstName,
    [Required] string LastName,
    string DateOfBirth,
    string? Allergies,
    string? EmergencyContact,
    string? MedicalNotes);

public record ScanDto([Required] string QrData);

public record CheckInDto([Required] string ChildId, string? Notes);
public record CheckOutDto([Required] string RecordId, string? Notes);
public record CheckOutByParentDto([Required] Guid ParentId, string? Notes);
public record CheckOutByChildDto([Required] Guid ChildId, string? Notes);

public record MarkAttendanceDto(
    [Required] List<Guid> ChildIds, 
    [Required] string Action); // CheckIn | CheckOut

public record ScanResultDto(ParentInfoDto Parent, List<ChildInfoDto> Children);
public record ParentInfoDto(Guid Id, string FirstName, string LastName, string Phone, string Email, string QrCode, List<string> ChildIds);
public record ChildInfoDto(Guid Id, string FirstName, string LastName, string DateOfBirth, List<string> ParentIds, string? Allergies, string? EmergencyContact, string? MedicalNotes, string PhotoUrl);

public record AttendanceRecordDto(
    string Id,
    string ChildId, 
    string ParentId,
    DateTime CheckInTime,
    DateTime? CheckOutTime,
    string CheckInStaffId,
    string? CheckOutStaffId,
    string? Notes,
    string Date);

public record LoginResponseDto(string Token, StaffDto User);
public record StaffDto(string Id, string FirstName, string LastName, string Email, string Role, bool IsActive);

public record AttendanceReportDto(string Action, DateTime TimestampUtc, string Child, string Parent, string Group);

public record QrRequestDto([Required] Guid Id);
public record SessionReportDto([Required] string Date);