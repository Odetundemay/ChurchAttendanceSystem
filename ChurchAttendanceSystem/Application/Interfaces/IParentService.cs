using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Application.Interfaces;

public interface IParentService
{
    Task<ServiceResult<Guid>> CreateParentAsync(CreateParentDto dto);
    Task<ServiceResult<byte[]>> GenerateQrCodeAsync(Guid parentId);
    Task<ServiceResult<ScanResultDto>> ScanQrCodeAsync(ScanDto dto);
}