using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;
using ChurchAttendanceSystem.Infrastructure;

namespace ChurchAttendanceSystem.Application.Services;

public class ParentService : IParentService
{
    private readonly AppDb _db;
    private readonly IQrService _qrService;
    private readonly IEncryptionService _encryption;

    public ParentService(AppDb db, IQrService qrService, IEncryptionService encryption)
    {
        _db = db;
        _qrService = qrService;
        _encryption = encryption;
    }

    public async Task<ServiceResult<List<ParentInfoDto>>> GetParentsAsync()
    {
        var parentsData = await _db.Parents
            .Include(p => p.Children)
            .ToListAsync();

        var parents = parentsData.Select(p => new ParentInfoDto(
            p.Id,
            p.FirstName,
            p.LastName,
            p.Phone,
            p.Email,
            System.Text.Json.JsonSerializer.Serialize(new { family = p.Id, s = p.QrSecret }),
            p.Children.Where(c => c.IsActive).Select(c => c.Id.ToString()).ToList()
        )).ToList();

        return ServiceResult<List<ParentInfoDto>>.Success(parents);
    }

    public async Task<ServiceResult<Guid>> CreateParentAsync(CreateParentDto dto)
    {
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Phone = dto.Phone,
            Email = dto.Email,
            QrSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
            CreatedAt = DateTime.UtcNow
        };

        _db.Parents.Add(parent);
        await _db.SaveChangesAsync();

        return ServiceResult<Guid>.Success(parent.Id, 201);
    }

    public async Task<ServiceResult<object>> GetQrDataAsync(Guid parentId)
    {
        var parent = await _db.Parents.FindAsync(parentId);
        if (parent is null)
            return ServiceResult<object>.NotFound("Parent not found");

        var qrData = new { family = parent.Id, s = parent.QrSecret };
        return ServiceResult<object>.Success(qrData);
    }

    public async Task<ServiceResult<ScanResultDto>> ScanQrCodeAsync(ScanDto dto)
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            
            // Parse the QR data JSON
            var qrData = System.Text.Json.JsonSerializer.Deserialize<QrPayload>(dto.QrData, options);
            if (qrData == null)
                return ServiceResult<ScanResultDto>.Failure("Invalid QR code format");

            if (!Guid.TryParse(qrData.family, out var familyId))
                return ServiceResult<ScanResultDto>.Failure("Invalid family ID format");

            var parent = await _db.Parents
                .Include(p => p.Children.Where(k => k.IsActive))
                .FirstOrDefaultAsync(p => p.Id == familyId && p.QrSecret == qrData.s);

            if (parent is null)
                return ServiceResult<ScanResultDto>.Failure("Invalid QR code");

            var result = new ScanResultDto(
                new ParentInfoDto(
                    parent.Id, 
                    parent.FirstName,
                    parent.LastName,
                    parent.Phone, 
                    parent.Email, 
                    dto.QrData,
                    parent.Children.Where(c => c.IsActive).Select(c => c.Id.ToString()).ToList()
                ),
                parent.Children.Where(c => c.IsActive).Select(c => new ChildInfoDto(
                    c.Id, 
                    _encryption.Decrypt(c.FirstName),
                    _encryption.Decrypt(c.LastName),
                    c.DateOfBirth,
                    new List<string> { parent.Id.ToString() },
                    _encryption.Decrypt(c.Allergies ?? ""),
                    _encryption.Decrypt(c.EmergencyContact ?? ""),
                    _encryption.Decrypt(c.MedicalNotes ?? ""),
                    string.IsNullOrEmpty(c.PhotoUrl) ? $"https://via.placeholder.com/150?text={Uri.EscapeDataString(_encryption.Decrypt(c.FirstName))}" : c.PhotoUrl
                )).ToList()
            );

            return ServiceResult<ScanResultDto>.Success(result);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return ServiceResult<ScanResultDto>.Failure($"Invalid QR code format: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteParentAsync(Guid parentId)
    {
        var parent = await _db.Parents.Include(p => p.Children).FirstOrDefaultAsync(p => p.Id == parentId);
        if (parent is null)
            return ServiceResult.NotFound("Parent not found");

        _db.Children.RemoveRange(parent.Children);
        _db.Parents.Remove(parent);
        await _db.SaveChangesAsync();

        return ServiceResult.Success();
    }

    private record QrPayload(string family, string s);
}