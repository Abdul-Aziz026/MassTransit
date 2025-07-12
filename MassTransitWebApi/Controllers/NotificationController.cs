using MassTransitWebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace MassTransitWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    public readonly INotificationService notificationService;

    public NotificationController(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }
    [HttpPost("email")]
    public async Task<IActionResult> SendEmail(string message = "This is a test email")
    {
        var result = await notificationService.SendEmailAsync(message);

        return Ok(new
        {
            message = result,
            pattern = "SEND (1-to-1)",
            note = "Check logs - ONLY email service receives this command"
        });
    }

    [HttpPost("sms")]
    public async Task<IActionResult> SendSms(string message = "This is a test SMS")
    {
        var result = await notificationService.SendSmsAsync(message);

        return Ok(new
        {
            message = result,
            pattern = "SEND (1-to-1)",
            note = "Check logs - ONLY SMS service receives this command"
        });
    }
}
