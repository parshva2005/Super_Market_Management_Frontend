// In Super_Market_Management/Models/Product.cs
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Super_Market_Management.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal ProductPrice { get; set; }

    public string ProductDescription { get; set; } = null!;

    public int ProductStock { get; set; }

    public string? ProductFeatures { get; set; }

    public string? ProductSpecifications { get; set; }

    public string? ProductIngredients { get; set; }

    public string? ProductUsageInstructions { get; set; }

    public string? ProductWarrantyInformation { get; set; }

    public string? ProductAdditionalInformation { get; set; }

    public int UserId { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public bool? IsRemoved { get; set; }

    public string? Sku { get; set; }

    public string? Barcode { get; set; }

    public decimal? CostPrice { get; set; }

    public int? ReorderLevel { get; set; }

    public bool TrackInventory { get; set; }
    public string? ProductImage1 { get; set; }

    public string? ProductImage2 { get; set; }

    public string? ProductImage3 { get; set; }
    [JsonIgnore]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public virtual Category? Category { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    [JsonIgnore]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    [JsonIgnore]
    public virtual ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public List<CategoryDropDown>? Categorylist { get; set; }
    public List<ReviewDropDown>? Reviewlist { get; set; }
    public virtual User? User { get; set; } = null!;
}
public class InventoryAdjustment
{
    public int UserId { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class ProductDropDown
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
}