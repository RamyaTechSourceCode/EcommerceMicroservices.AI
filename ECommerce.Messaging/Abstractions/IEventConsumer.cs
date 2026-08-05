using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Messaging.Abstractions
{
    public interface IEventConsumer<T>
    {
        Task Handle(T message, CancellationToken cancellationToken);
    }
}
