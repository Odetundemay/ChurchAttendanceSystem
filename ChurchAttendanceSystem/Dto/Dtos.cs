
public record RegisterStaffDto(string FullName, string Email, string Password, string Role);
public record LoginDto(string Email, string Password);

public record CreateParentDto(string FullName, string Phone, string Email);
public record CreateChildDto(string FullName, string Group);

public record ScanDto(Guid Family, string S);
public record MarkAttendanceDto(List<Guid> ChildIds, string Action); // CheckIn | CheckOut