using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Application.Interfaces;

public interface IChildService
{
    Task<ServiceResult<Guid>> CreateChildAsync(Guid parentId, CreateChildDto dto);
}