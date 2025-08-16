using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Application.Interfaces;

public interface IChildService
{
    Task<ServiceResult<List<ChildInfoDto>>> GetChildrenAsync();
    Task<ServiceResult<Guid>> CreateChildAsync(Guid parentId, CreateChildDto dto);
    Task<ServiceResult> DeleteChildAsync(Guid childId);
}