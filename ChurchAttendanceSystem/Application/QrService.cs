using System.Text.Json;
using QRCoder;

namespace ChurchAttendanceSystem.Application;

public interface IQrService { byte[] GeneratePng(object payload); }
public class QrService : IQrService
{
    public byte[] GeneratePng(object payload)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(payload, options);
        
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(json, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(10, new byte[] { 0, 0, 0 }, new byte[] { 255, 255, 255 });
    }
}