using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KioskApp.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MenuId { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
        public string OptionText { get; set; }
    }

}
