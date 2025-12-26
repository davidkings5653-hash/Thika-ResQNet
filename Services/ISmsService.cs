namespace ThikaResQNet.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(IEnumerable<string> toPhoneNumbers, string message);
        // Handle inbound SMS webhook payload
        Task HandleInboundSmsAsync(string from, string text);
    }
}