﻿using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Subscriptions
{
    public interface IEventBusSubscriptionManager
    {
        #region Event Handlers

        event EventHandler<string> OnEventRemoved;

        #endregion

        #region Status

        bool IsEmpty { get; }

        bool HasSubscriptionsForEvent(string eventName);

        #endregion

        #region Events info

        string GetEventIdentifier<TEvent>();

        Type GetEventTypeByName(string eventName);

        IEnumerable<Subscription> GetHandlersForEvent(string eventName);

        Dictionary<string, List<Subscription>> GetAllSubscriptions();

        #endregion

        #region Subscription management 
        void AddSubscription<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void RemoveSubscription<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void Clear();

        #endregion
    }
}
