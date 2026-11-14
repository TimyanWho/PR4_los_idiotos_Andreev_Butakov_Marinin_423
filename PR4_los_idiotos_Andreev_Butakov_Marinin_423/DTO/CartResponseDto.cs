using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO
{
    public class CartResponseDto
    {
        public List<CartEntryDto>? Items { get; set; }
        public decimal Total { get; set; }
    }
}
