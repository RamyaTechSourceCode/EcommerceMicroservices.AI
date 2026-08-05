using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Messaging.Abstractions
{
    public interface IEventBus
    {
        Task PublishAsync<T>(string topic, T @event);
    }
}
