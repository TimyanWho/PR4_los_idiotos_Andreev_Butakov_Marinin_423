using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO
{
    public class ReviewItemDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Author { get; set; }
    }
}
