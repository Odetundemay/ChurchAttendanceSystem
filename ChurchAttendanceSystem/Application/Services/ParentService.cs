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

    public ParentService(AppDb db, IQrService qrService)
    {
        _db = db;
        _qrService = qrService;
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

    public async Task<ServiceResult<byte[]>> GenerateQrCodeAsync(Guid parentId)
    {
        var parent = await _db.Parents.FindAsync(parentId);
        if (parent is null)
            return ServiceResult<byte[]>.NotFound("Parent not found");

        var payload = new { family = parent.Id, s = parent.QrSecret };
        var qrCode = _qrService.GeneratePng(payload);

        return ServiceResult<byte[]>.Success(qrCode);
    }

    public async Task<ServiceResult<ScanResultDto>> ScanQrCodeAsync(ScanDto dto)
    {
        try
        {
            // Parse the QR data JSON
            var qrData = System.Text.Json.JsonSerializer.Deserialize<QrPayload>(dto.QrData);
            if (qrData == null)
                return ServiceResult<ScanResultDto>.Failure("Invalid QR code format");

            var parent = await _db.Parents
                .Include(p => p.Children.Where(k => k.IsActive))
                .FirstOrDefaultAsync(p => p.Id == qrData.family && p.QrSecret == qrData.s);

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
                    c.FirstName,
                    c.LastName,
                    c.DateOfBirth,
                    new List<string> { parent.Id.ToString() },
                    c.Allergies,
                    c.EmergencyContact,
                    c.MedicalNotes,
                    string.IsNullOrEmpty(c.PhotoUrl) ? $"https://via.placeholder.com/150?text={Uri.EscapeDataString(c.FirstName)}" : c.PhotoUrl
                )).ToList()
            );

            return ServiceResult<ScanResultDto>.Success(result);
        }
        catch (System.Text.Json.JsonException)
        {
            return ServiceResult<ScanResultDto>.Failure("Invalid QR code format");
        }
    }

    private record QrPayload(Guid family, string s);
}