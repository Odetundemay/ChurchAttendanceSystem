using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;
using ChurchAttendanceSystem.Infrastructure;

namespace ChurchAttendanceSystem.Application.Services;

public class ChildService : IChildService
{
    private readonly AppDb _db;

    public ChildService(AppDb db)
    {
        _db = db;
    }

    public async Task<ServiceResult<Guid>> CreateChildAsync(Guid parentId, CreateChildDto dto)
    {
        if (!await _db.Parents.AnyAsync(x => x.Id == parentId))
            return ServiceResult<Guid>.NotFound("Parent not found");

        var child = new Child
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            FullName = dto.FullName,
            Group = dto.Group,
            IsActive = true
        };

        _db.Children.Add(child);
        await _db.SaveChangesAsync();

        return ServiceResult<Guid>.Success(child.Id, 201);
    }
}