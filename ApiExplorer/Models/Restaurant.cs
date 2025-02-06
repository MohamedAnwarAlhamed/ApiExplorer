namespace ApiExplorer.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        // العلاقة مع جدول MenuItems (اختياري)
        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
