using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BookShoppingCart.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> AddItem(int bookId, int qty = 1, int redirect = 0)
        {
            string userId = await GetUserId();
            using var transaction = _db.Database.BeginTransaction();

            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");

                var cart = await GetCart(userId);

                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCart.Add(cart);
                }
                _db.SaveChanges();

                // cart detail section
                var cartItem = _db.CartDetail
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _db.Book.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };
                    _db.CartDetail.Add(cartItem);
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }

            var cartItemCount = await GetCartItemCount(userId);

            if (redirect == 0)
                return Ok(cartItemCount);

            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            string userId = await GetUserId();

            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");

                var cart = await GetCart(userId);

                if (cart is null)
                    throw new Exception("Invalid cart");

                // cart detail section
                var cartItem = _db.CartDetail
                    .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is null)
                    throw new Exception("Not items in cart");
                else if (cartItem.Quantity == 1)
                    _db.CartDetail.Remove(cartItem);
                else
                    cartItem.Quantity = cartItem.Quantity - 1;

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }

            var cartItemCount = await GetCartItemCount(userId);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var userId = await GetUserId();

            if (userId == null)
                throw new Exception("Invalid userid");

            var shoppingCart = await _db.ShoppingCart
                .Include(a => a.CartDetails)
                .ThenInclude(a => a.Book)
                .ThenInclude(a => a.Genres)
                .Where(a => a.UserId == userId).FirstOrDefaultAsync();

            return View(shoppingCart);
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await GetCartItemCount();
            return Ok(cartItem);
        }

        public async Task<IActionResult> Checkout()
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                // logic
                // move data from cartDetail to order and order detail then remove cart detail
                var userId = await GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");

                var cart = await GetCart(userId);

                if (cart is null)
                    throw new Exception("Invalid cart");

                var cartDetail = _db.CartDetail
                    .Where(a => a.ShoppingCartId == cart.Id).ToList();

                if (cartDetail.Count == 0)
                    throw new Exception("Cart is empty");

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                };
                _db.Order.Add(order);
                _db.SaveChanges();

                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _db.OrderDetail.Add(orderDetail);
                }

                _db.SaveChanges();
                // removing the cartdetails
                _db.CartDetail.RemoveRange(cartDetail);
                _db.SaveChanges();

                transaction.Commit();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                transaction.Rollback();
                // Handle exceptions
                throw new Exception("Something happened during checkout");
            }
        }

        private async Task<string> GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            return userId;
        }

        private async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = await GetUserId();
            }

            var data = await (from cart in _db.ShoppingCart
                              join cartDetail in _db.CartDetail
                              on cart.Id equals cartDetail.ShoppingCartId
                              select new { cartDetail.Id }
                        ).ToListAsync();

            return data.Count;
        }

        private async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }
    }
}
