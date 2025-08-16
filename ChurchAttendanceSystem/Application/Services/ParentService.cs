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

    public async Task<ServiceResult<Guid>> CreateParentAsync(CreateParentDto dto)
    {
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName.Trim(),
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
        var parent = await _db.Parents
            .Include(p => p.Children.Where(k => k.IsActive))
            .FirstOrDefaultAsync(p => p.Id == dto.Family && p.QrSecret == dto.S);

        if (parent is null)
            return ServiceResult<ScanResultDto>.Failure("Invalid QR code");

        var result = new ScanResultDto(
            new ParentInfoDto(parent.Id, parent.FullName),
            parent.Children.Select(c => new ChildInfoDto(c.Id, c.FullName, c.Group)).ToList()
        );

        return ServiceResult<ScanResultDto>.Success(result);
    }
}