using Microsoft.AspNetCore.Mvc;
using ThikaResQNet.Services;

namespace ThikaResQNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsWebhookController : ControllerBase
    {
        private readonly ISmsService _sms;

        public SmsWebhookController(ISmsService sms)
        {
            _sms = sms;
        }

        [HttpPost("inbound")]
        public async Task<IActionResult> Inbound([FromForm] InboundSmsForm form)
        {
            // Africa's Talking posts inbound SMS as form data; adapt fields as per provider
            var from = form.From ?? form.from ?? string.Empty;
            var text = form.Text ?? form.text ?? string.Empty;
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(text)) return BadRequest();

            await _sms.HandleInboundSmsAsync(from, text);
            return Ok();
        }
    }

    public class InboundSmsForm
    {
        // Africa's Talking sends different keys; include common variants
        public string? From { get; set; }
        public string? Text { get; set; }
        public string? from { get; set; }
        public string? text { get; set; }
    }
}