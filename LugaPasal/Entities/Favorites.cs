using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LugaPasal.Entities
{
    public class Favorites
    {
        public Guid FavoritesID { get; set; }
        public string ?UserID { get; set; }
        public User User { get; set; }
        public Guid  ?ProductID { get; set; }
        public Products Product { get; set; }
    }
}
