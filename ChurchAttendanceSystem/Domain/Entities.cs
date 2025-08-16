using System.ComponentModel.DataAnnotations;

public class Parent
{
    [Key] public Guid Id { get; set; }
    [Required] public string FullName { get; set; } = default!;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [Required] public string QrSecret { get; set; } = default!; // used to validate QR payload
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Child> Children { get; set; } = new List<Child>();
}

public class Child
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = default!;
    [Required] public string FullName { get; set; } = default!;
    public string Group { get; set; } = ""; // e.g., Nursery/Primary
    public bool IsActive { get; set; } = true;
}

public class StaffUser
{
    [Key] public Guid Id { get; set; }
    [Required] public string FullName { get; set; } = default!;
    [Required] public string Email { get; set; } = default!;
    [Required] public string PasswordHash { get; set; } = default!;
    [Required] public string Role { get; set; } = "Staff"; // Admin or Staff
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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