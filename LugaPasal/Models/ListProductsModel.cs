using LugaPasal.Entities;

namespace LugaPasal.Models
{
    public class ListProductsModel
    {
        public List<Products> products { get; set; } = new List<Products>();
        public List<User> users { get; set; } = new List<User>();

    }
}
