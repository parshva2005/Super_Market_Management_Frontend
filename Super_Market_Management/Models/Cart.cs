using System;
using System.Collections.Generic;

namespace Super_Market_Management.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int ProductQuantity { get; set; }

    public DateTime AddedDate { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public bool IsCheckedOut { get; set; }

    public DateTime? CheckoutDate { get; set; }

    public string? SessionId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
    public List<UserDropDown>? Userlist { get; set; }
    public List<ProductDropDown>? Productlist { get; set; }
}
