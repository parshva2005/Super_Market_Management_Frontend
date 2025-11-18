using System;
using System.Collections.Generic;

namespace Super_Market_Management.Models;

public partial class ProductImage
{
    public int ProductImageId { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }

    public virtual Product Product { get; set; } = null!;
}
