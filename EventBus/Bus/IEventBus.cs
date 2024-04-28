using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Events;

namespace EventBus.Bus
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event)
            where TEvent : Event;

        void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
    }
}
