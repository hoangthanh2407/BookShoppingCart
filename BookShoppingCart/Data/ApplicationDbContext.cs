using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookShoppingCart.Models;

namespace BookShoppingCart.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<BookShoppingCart.Models.Genre> Genre { get; set; } = default!;
        public DbSet<BookShoppingCart.Models.Book> Book { get; set; } = default!;

        public DbSet<ShoppingCart> ShoppingCart{ get; set; }
        public DbSet<CartDetail> CartDetail { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
    }
}