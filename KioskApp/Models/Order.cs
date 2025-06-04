using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KioskApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderTime { get; set; }
        public int TotalPrice { get; set; }
        public string PaymentType { get; set; }
        public string Status { get; set; }
    }
}
