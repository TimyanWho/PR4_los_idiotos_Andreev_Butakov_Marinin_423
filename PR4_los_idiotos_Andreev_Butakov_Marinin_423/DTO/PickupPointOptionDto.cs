using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO
{
    public class PickupPointOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";

        public string DisplayName
        {
            get
            {
                return string.IsNullOrWhiteSpace(Name) || Name == Address
                    ? Address
                    : Name + " — " + Address;
            }
        }
    }
}
