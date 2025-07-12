using MassTransitRequestResponseWebApi2.Service;
using Microsoft.AspNetCore.Mvc;

namespace MassTransitRequestResponseWebApi2.Controllers;

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
    public async Task<IActionResult> SendEmail(string message = "Hello from API!")
    {
        var result = await notificationService.SendEmailNotificationAsync(message);

        return Ok(new
        {
                message = result,
                pattern = "SEND (ISendEndpointProvider)",
                note = "Only email consumer will receive this command"
        });
    }
    
    [HttpPost("sms")]
    public async Task<IActionResult> SendSms(string message = "Hello from API!")
    {
        var result = await notificationService.SendSmsNotificationAsync(message);

        return Ok(new
        {
            message = result,
            pattern = "SEND (ISendEndpointProvider)",
            note = "Only SMS consumer will receive this command"
        });
    }
}
