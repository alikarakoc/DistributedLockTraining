
using Dapper.Contrib.Extensions;

namespace DistributedLockTraining.Domain.Entities
{
    [Table("Products")]
    public class ProductEntity
    {
        [Key]
        public int ProductId { get; set; }
        public required string Name { get; set; }
        public required int TotalStock { get; set; }
        public required int MaxStockQuantity { get; set; }
        public required decimal Price { get; set; }
    }
}
