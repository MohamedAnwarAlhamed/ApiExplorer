namespace ApiExplorer.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; } // المفتاح الأساسي
        public int RestaurantId { get; set; } // المفتاح الخارجي
        public string? Name { get; set; } // اسم العنصر
        public string? Description { get; set; } // وصف العنصر
        public decimal Price { get; set; } // سعر العنصر
        public DateTime? CreatedAt { get; set; } // تاريخ الإنشاء (يمكن أن يكون null)

        // العلاقة مع جدول Restaurants (اختياري)
        public Restaurant? Restaurant { get; set; }
    }
}
