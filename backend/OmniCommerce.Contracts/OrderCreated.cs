using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCommerce.Contracts
{
    public record OrderCreated
    (
        Guid OrderId,
        string CustomerId,
        decimal Amount,
        string Currency,
        DateTimeOffset CreatedAt
    );
}
