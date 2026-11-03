using System;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models
{
    public class ProductReviewModel
    {
        public string AuthorName { get; set; } = "Покупатель";
        public int Rating { get; set; }
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}