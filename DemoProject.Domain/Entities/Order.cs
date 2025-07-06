using DemoProject.Domain.Enums;

namespace DemoProject.Domain.Entities
{
    public class Order : BaseEntity<Guid>
    {
        public string Name { get; set; }           // for search/filter
        public decimal Amount { get; set; }
        public EOrderStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }


    }
}
