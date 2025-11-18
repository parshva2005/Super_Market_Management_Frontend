using System;
using System.Collections.Generic;

namespace Super_Market_Management.Models;

public partial class ProductLog
{
    public int ProductLogId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public DateTime LogDate { get; set; }

    public string LogType { get; set; } = null!;

    public string? LogDetails { get; set; }

    public int QuantityChange { get; set; }

    public string? Reference { get; set; }

    public string? Notes { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
