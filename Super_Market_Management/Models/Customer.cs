using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Super_Market_Management.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public int? UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? Address { get; set; }

    public DateTime RegistrationDate { get; set; }

    public bool IsActive { get; set; }
    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual User? User { get; set; }
}
public class CustomerDropDown
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = null!;
}