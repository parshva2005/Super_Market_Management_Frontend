using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Super_Market_Management.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public string? Notes { get; set; }

    public string? PaymentMethod { get; set; }

    public int? SalesPersonId { get; set; }

    public virtual Customer? Customer { get; set; }
    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    [JsonIgnore]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? SalesPerson { get; set; }

    public virtual User? User { get; set; } = null!;
    public List<UserDropDown>? Userlist { get; set; }
    public List<CustomerDropDown>? Customerlist { get; set; }
}
public class OrderCheckoutModel
{
    public int CustomerId { get; set; }
    public int UserId { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public List<OrderItemModel> Items { get; set; }
}

public class OrderItemModel
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}