using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO
{
    public class CartEntryDto
    {
        public ProductResponseDto? Product { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
