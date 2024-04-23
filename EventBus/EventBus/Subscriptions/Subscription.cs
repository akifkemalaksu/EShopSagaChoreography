using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Subscriptions
{
    public class Subscription
    {
        public Subscription(Type eventType, Type handlerType)
        {
            EventType = eventType;
            HandlerType = handlerType;
        }

        public Type EventType { get; set; }
        public Type HandlerType { get; set; }
    }
}
