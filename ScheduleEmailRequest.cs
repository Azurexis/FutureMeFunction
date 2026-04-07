using System;
using System.Collections.Generic;
using System.Text;

namespace FutureMeFunction;

public class ScheduleEmailRequest
{
    public string? RecipientEmail { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public DateTimeOffset ScheduledForUtc { get; set; }
}