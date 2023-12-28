using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookShoppingCart.Data;
using BookShoppingCart.Models;

namespace BookShoppingCart.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment env;


        public BooksController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            this.env = env;
        }
        // Add a method to delete the existing image file
        private void DeleteExistingImage(int bookId)
        {
            var existingBook = _context.Book.Find(bookId);
            if (existingBook != null && !string.IsNullOrEmpty(existingBook.ImgLink))
            {
                string filePath = Path.Combine(env.WebRootPath, "images", existingBook.ImgLink);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return _context.Book != null ?
                        View(await _context.Book.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Book'  is null.");
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genre, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadFile(book);
                Book emp = new Book
                {
                    Title = book.Title,
                    GenreID = book.GenreID,
                    Author = book.Author,
                    DatePublished = book.DatePublished,
                    Price = book.Price,
                    ImgLink = uniqueFileName
                };
                _context.Add(emp);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["GenreId"] = new SelectList(_context.Genre, "Id", "Name", book.GenreID);
            return View(book);
        }

        private string UploadFile(Book model)
        {
            string uniqueFileName = null;
            if (model.UrlImg != null)
            {
                string uploadsFolder = Path.Combine(env.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.UrlImg.FileName;
                string filepath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    model.UrlImg.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["GenreId"] = new SelectList(_context.Genre, "Id", "Name", book.GenreID);
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Description,DatePublished,Price,ImgLink,UrlImg")] Book model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Book.FindAsync(id);

                    // Update properties of the existing entity
                    existingBook.Title = model.Title;
                    existingBook.Author = model.Author;
                    existingBook.DatePublished = model.DatePublished;
                    existingBook.Price = model.Price;

                    // Check if a new image is uploaded
                    if (model.UrlImg != null)
                    {
                        // Delete the existing image file
                        DeleteExistingImage(existingBook.Id);

                        // Upload the new image
                        string uniqueFileName = UploadFile(model);
                        existingBook.ImgLink = uniqueFileName;
                    }

                    // Save changes
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }



        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Book == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Book'  is null.");
            }
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return (_context.Book?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
