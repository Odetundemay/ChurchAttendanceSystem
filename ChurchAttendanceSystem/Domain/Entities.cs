using System.ComponentModel.DataAnnotations;

namespace ChurchAttendanceSystem.Domain;

public class Parent
{
    [Key] public Guid Id { get; set; }
    [Required] public string FirstName { get; set; } = default!;
    [Required] public string LastName { get; set; } = default!;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [Required] public string QrSecret { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Child> Children { get; set; } = new List<Child>();
}

public class Child
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = default!;
    [Required] public string FirstName { get; set; } = default!;
    [Required] public string LastName { get; set; } = default!;
    public string DateOfBirth { get; set; } = "2020-01-01";
    public string? Allergies { get; set; }
    public string? EmergencyContact { get; set; }
    public string? MedicalNotes { get; set; }
    public string PhotoUrl { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public class StaffUser
{
    [Key] public Guid Id { get; set; }
    [Required] public string FirstName { get; set; } = default!;
    [Required] public string LastName { get; set; } = default!;
    [Required] public string Email { get; set; } = default!;
    [Required] public string PasswordHash { get; set; } = default!;
    [Required] public string Role { get; set; } = "Staff";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AttendanceRecord
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid ChildId { get; set; }
    public Child Child { get; set; } = default!;
    [Required] public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = default!;
    [Required] public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? CheckInNotes { get; set; }
    public string? CheckOutNotes { get; set; }
    [Required] public Guid CheckInStaffId { get; set; }
    public StaffUser CheckInStaff { get; set; } = default!;
    public Guid? CheckOutStaffId { get; set; }
    public StaffUser? CheckOutStaff { get; set; }
}

public class AttendanceLog
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid ChildId { get; set; }
    public Child Child { get; set; } = default!;
    [Required] public string Action { get; set; } = default!; // CheckIn | CheckOut
    [Required] public DateTime TimestampUtc { get; set; }
    [Required] public Guid HandledByUserId { get; set; }
    public StaffUser HandledBy { get; set; } = default!;
    [Required] public string SessionDate { get; set; } = default!; // YYYY-MM-DD
}