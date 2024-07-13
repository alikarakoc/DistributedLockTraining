using Dapper.Contrib.Extensions;

namespace DistributedLockTraining.Domain.Entities
{
    [Table("Orders")]
    public class OrderEntity
    {
        [Key]
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string OrderNumber { get; set; }
        public decimal OrderTotal { get; set; }
        public int Quantity { get; set; }
    }
}
