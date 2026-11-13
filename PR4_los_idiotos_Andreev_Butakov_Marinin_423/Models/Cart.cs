#nullable disable
using System;
using System.Collections.Generic;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Products Product { get; set; }

    public virtual Users User { get; set; }
}