using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Application.Interfaces;

public interface IParentService
{
    Task<ServiceResult<List<ParentInfoDto>>> GetParentsAsync();
    Task<ServiceResult<Guid>> CreateParentAsync(CreateParentDto dto);
    Task<ServiceResult<object>> GetQrDataAsync(Guid parentId);
    Task<ServiceResult<ScanResultDto>> ScanQrCodeAsync(ScanDto dto);
}