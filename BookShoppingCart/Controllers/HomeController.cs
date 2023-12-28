using BookShoppingCart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookShoppingCart.Controllers
{
    namespace BookShoppingCartMvcUI.Controllers
    {
        public class HomeController : Controller
        {
            private readonly ILogger<HomeController> _logger;
            private readonly ApplicationDbContext _db;

            public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
            {
                _logger = logger;
                _db = db;
            }

            public async Task<IActionResult> Index(string sterm = "", int genreId = 0)
            {
                var books = await GetBooks(sterm, genreId);
                var genres = await GetGenres();

                var bookModel = new BookDisplayModel
                {
                    Books = books,
                    Genres = genres,
                    STerm = sterm,
                    GenreId = genreId
                };

                return View(bookModel);
            }

            public IActionResult Privacy()
            {
                return View();
            }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            private async Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0)
            {
                sTerm = sTerm.ToLower();
                var books = await (from book in _db.Book
                                   join genre in _db.Genre on book.GenreID equals genre.Id
                                   where string.IsNullOrWhiteSpace(sTerm) || (book != null && book.Title.ToLower().StartsWith(sTerm))
                                   select new Book
                                   {
                                       Id = book.Id,
                                       ImgLink = book.ImgLink,
                                       Author = book.Author,
                                       Title = book.Title,
                                       GenreID = book.GenreID,
                                       Price = book.Price,

                                   }).ToListAsync();

                if (genreId > 0)
                {
                    books = books.Where(a => a.GenreID == genreId).ToList();
                }
                return books;
            }

            private async Task<IEnumerable<Genre>> GetGenres()
            {
                return await _db.Genre.ToListAsync();
            }
        }
    }
}