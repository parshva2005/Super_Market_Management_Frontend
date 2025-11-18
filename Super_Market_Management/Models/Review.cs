namespace Super_Market_Management.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        public int? OrderId { get; set; }

        public int? Rating { get; set; }

        public string? Review1 { get; set; }

        public bool? IsOrdered { get; set; }

        public virtual Order? Order { get; set; }

        public virtual Product Product { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
public class ReviewDropDown
{
    public int ReviewId { get; set; }
    public string Review1 { get; set; } = null!;
    public int Rating { get; set; }
}