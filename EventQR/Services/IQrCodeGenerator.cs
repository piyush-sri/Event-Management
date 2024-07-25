namespace EventQR.Services
{
    public interface IQrCodeGenerator
    { 
        public string GenerateQRCode(Guid guestId, Guid eventId);
    }
}
