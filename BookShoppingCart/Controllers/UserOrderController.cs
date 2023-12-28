using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCart.Controllers
{
    namespace BookShoppingCartMvcUI.Controllers
    {
        [Authorize]
        public class UserOrderController : Controller
        {
            private readonly ApplicationDbContext _db;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly UserManager<IdentityUser> _userManager;

            public UserOrderController(
                ApplicationDbContext db,
                UserManager<IdentityUser> userManager,
                IHttpContextAccessor httpContextAccessor)
            {
                _db = db;
                _userManager = userManager;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<IActionResult> UserOrders()
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");

                var orders = await _db.Order
                                .Include(x => x.OrderDetail)
                                .ThenInclude(x => x.Book)
                                .ThenInclude(x => x.Genres)
                                .Where(a => a.UserId == userId)
                                .ToListAsync();

                return View(orders);
            }

            private string GetUserId()
            {
                var principal = _httpContextAccessor.HttpContext.User;
                string userId = _userManager.GetUserId(principal);
                return userId;
            }
        }
    }
}
