using System.Text.Json;
using QRCoder;

namespace ChurchAttendanceSystem.Application;

public interface IQrService { byte[] GeneratePng(object payload); }
public class QrService : IQrService
{
    public byte[] GeneratePng(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(json, QRCodeGenerator.ECCLevel.M);
        return new PngByteQRCode(data).GetGraphic(20);
    }
}