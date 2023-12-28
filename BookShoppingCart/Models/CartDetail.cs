namespace BookShoppingCart.Models
{
    public class CartDetail
    {
        public int Id { get; set; }
        public int ShoppingCartId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public Book Book { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
    }
}
