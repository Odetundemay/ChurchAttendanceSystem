using System;
using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;
using ChurchAttendanceSystem.Infrastructure;

namespace ChurchAttendanceSystem.Application.Services;

public class ChildService : IChildService
{
    private readonly AppDb _db;
    private readonly IEncryptionService _encryption;

    public ChildService(AppDb db, IEncryptionService encryption)
    {
        _db = db;
        _encryption = encryption;
    }

    public async Task<ServiceResult<List<ChildInfoDto>>> GetChildrenAsync()
    {
        var children = await _db.Children
            .Where(c => c.IsActive)
            .Include(c => c.Parent)
            .Select(c => new ChildInfoDto(
                c.Id,
                _encryption.Decrypt(c.FirstName),
                _encryption.Decrypt(c.LastName),
                c.DateOfBirth,
                new List<string> { c.ParentId.ToString() },
                _encryption.Decrypt(c.Allergies ?? ""),
                _encryption.Decrypt(c.EmergencyContact ?? ""),
                _encryption.Decrypt(c.MedicalNotes ?? ""),
                string.IsNullOrEmpty(c.PhotoUrl) ? $"https://via.placeholder.com/150?text={Uri.EscapeDataString(_encryption.Decrypt(c.FirstName))}" : c.PhotoUrl
            ))
            .ToListAsync();

        return ServiceResult<List<ChildInfoDto>>.Success(children);
    }

    public async Task<ServiceResult<Guid>> CreateChildAsync(Guid parentId, CreateChildDto dto)
    {
        if (!await _db.Parents.AnyAsync(x => x.Id == parentId))
            return ServiceResult<Guid>.NotFound("Parent not found");

        var child = new Child
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            FirstName = _encryption.Encrypt(dto.FirstName),
            LastName = _encryption.Encrypt(dto.LastName),
            DateOfBirth = dto.DateOfBirth ?? "2020-01-01",
            Allergies = _encryption.Encrypt(dto.Allergies ?? ""),
            EmergencyContact = _encryption.Encrypt(dto.EmergencyContact ?? ""),
            MedicalNotes = _encryption.Encrypt(dto.MedicalNotes ?? ""),
            PhotoUrl = $"https://via.placeholder.com/150?text={Uri.EscapeDataString(dto.FirstName)}",
            IsActive = true
        };

        _db.Children.Add(child);
        await _db.SaveChangesAsync();

        return ServiceResult<Guid>.Success(child.Id, 201);
    }

    public async Task<ServiceResult> DeleteChildAsync(Guid childId)
    {
        var child = await _db.Children.FindAsync(childId);
        if (child is null)
            return ServiceResult.NotFound("Child not found");

        _db.Children.Remove(child);
        await _db.SaveChangesAsync();

        return ServiceResult.Success();
    }
}