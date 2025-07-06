using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Domain.Entities
{
    public class Invoice : BaseEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset IssuedAt { get; set; }
    }
}
