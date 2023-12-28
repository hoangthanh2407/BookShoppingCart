using System.ComponentModel.DataAnnotations.Schema;

namespace BookShoppingCart.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime DatePublished { get; set; }
        public double Price { get; set; }
        public int GenreID { get; set; }
        public virtual Genre? Genres { get; set; }

        public string? ImgLink { get; set; }

        [NotMapped]
        public IFormFile UrlImg { get; set; }
    }
}
