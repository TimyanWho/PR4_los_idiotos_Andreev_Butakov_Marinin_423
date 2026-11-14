using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO
{
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string DeliverySummary { get; set; }
        public decimal TotalAmount { get; set; }
        public IReadOnlyList<OrderItemDto> Items { get; set; }
    }
}
