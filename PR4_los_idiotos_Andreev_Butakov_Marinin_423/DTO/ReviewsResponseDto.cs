using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO
{
    public class ReviewsResponseDto
    {
        public int ProductId { get; set; }
        public double Average { get; set; }
        public List<ReviewItemDto>? Reviews { get; set; }
    }
}
