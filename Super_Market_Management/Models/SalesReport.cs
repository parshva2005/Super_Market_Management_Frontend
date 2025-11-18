using System;
using System.Collections.Generic;

namespace Super_Market_Management.Models;

public partial class SalesReport
{
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalPrice { get; set; }

    public string? CustomerName { get; set; }

    public string? SalesPerson { get; set; }

    public int? ItemsSold { get; set; }

    public decimal? GrossSales { get; set; }
}
