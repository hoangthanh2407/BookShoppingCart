namespace BookShoppingCart.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public int OrderStatusId { get; set; }
        public bool IsDeleted { get; set; } = false;

        public List<OrderDetail> OrderDetail { get; set; }
    }
}
