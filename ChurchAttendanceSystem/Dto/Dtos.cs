
using System.ComponentModel.DataAnnotations;

namespace ChurchAttendanceSystem.Dto;

public record RegisterStaffDto(
    [Required] string FullName, 
    [Required, EmailAddress] string Email, 
    [Required, MinLength(8)] string Password, 
    string Role = "Staff");

public record LoginDto(
    [Required, EmailAddress] string Email, 
    [Required] string Password);

public record CreateParentDto(
    [Required] string FullName, 
    string Phone, 
    [EmailAddress] string Email);

public record CreateChildDto(
    [Required] string FullName, 
    string Group);

public record ScanDto(Guid Family, string S);

public record MarkAttendanceDto(
    [Required] List<Guid> ChildIds, 
    [Required] string Action); // CheckIn | CheckOut

public record ScanResultDto(ParentInfoDto Parent, List<ChildInfoDto> Children);
public record ParentInfoDto(Guid Id, string FullName);
public record ChildInfoDto(Guid Id, string FullName, string Group);

public record AttendanceReportDto(string Action, DateTime TimestampUtc, string Child, string Parent, string Group);