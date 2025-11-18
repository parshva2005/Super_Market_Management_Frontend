using System;
using System.Collections.Generic;

namespace Super_Market_Management.Models;

public partial class UserLog
{
    public int UserLogId { get; set; }

    public int UserId { get; set; }

    public DateTime LogDate { get; set; }

    public string LogType { get; set; } = null!;

    public string? LogDetails { get; set; }

    public string? IpAddress { get; set; }

    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
