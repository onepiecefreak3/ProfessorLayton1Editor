using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.EventBrokerage.Exceptions;

namespace CrossCutting.Core.EventBrokerage;

public class EventBroker : IEventBroker
{
    private readonly Dictionary<Type, List<Subscription>> _messageSubscriptions = [];
    private Func<Type, object>? _resolverCallback;

    public void Subscribe<THandler, TMessage>(Action<THandler, TMessage> handler, Func<TMessage, bool>? filter)
    {
        AddSubscription<TMessage>(filter, handler, typeof(THandler));
    }

    public void Subscribe<THandler, TMessage>(Func<THandler, TMessage, Task> handler, Func<TMessage, bool>? filter)
    {
        AddSubscription<TMessage>(filter, handler, typeof(THandler));
    }

    public void Subscribe<TMessage>(Action<TMessage> handler, Func<TMessage, bool>? filter)
    {
        AddSubscription<TMessage>(filter, handler, null);
    }

    public void Subscribe<TMessage>(Func<TMessage, Task> handler, Func<TMessage, bool>? filter)
    {
        AddSubscription<TMessage>(filter, handler, null);
    }

    private void AddSubscription<TMessage>(Delegate? filter, Delegate handler, Type? handlerType)
    {
        ArgumentNullException.ThrowIfNull(handler);

        Subscription subscription = new(handler);

        if (filter != null)
        {
            subscription.Filter = filter;
        }

        if (handlerType != null)
        {
            subscription.HandlerType = handlerType;
        }

        AddSubscription<TMessage>(subscription);
    }

    private void AddSubscription<TMessage>(Subscription subscription)
    {
        Type messageType = typeof(TMessage);

        if (!_messageSubscriptions.TryGetValue(messageType, out var subscriptions))
        {
            subscriptions = [];
            _messageSubscriptions.Add(messageType, subscriptions);
        }

        bool isHandlerAlreadyRegistered = subscriptions.Any(s => s.Handler == subscription.Handler);
        if (isHandlerAlreadyRegistered)
        {
            throw new DuplicatedHandlerException("Handler was already registered");
        }

        subscriptions.Add(subscription);
    }

    public void Raise<TMessage>(TMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = typeof(TMessage);
        bool isSomeoneInterested = _messageSubscriptions.ContainsKey(messageType) && _messageSubscriptions[messageType].Count > 0;
        if (!isSomeoneInterested)
        {
            return;
        }

        List<Subscription> subscriptions = _messageSubscriptions[messageType];
        EnsureResolveCallbackIsSetIfNeeded(subscriptions);

        foreach (Subscription subscription in subscriptions)
        {
            RaiseForSubscription(message, subscription);
        }
    }

    public async Task RaiseAsync<TMessage>(TMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = typeof(TMessage);
        bool isSomeoneInterested = _messageSubscriptions.ContainsKey(messageType) && _messageSubscriptions[messageType].Count > 0;
        if (!isSomeoneInterested)
        {
            return;
        }

        List<Subscription> subscriptions = _messageSubscriptions[messageType];
        EnsureResolveCallbackIsSetIfNeeded(subscriptions);

        foreach (Subscription subscription in subscriptions)
        {
            await RaiseForSubscriptionAsync(message, subscription);
        }
    }

    private void EnsureResolveCallbackIsSetIfNeeded(List<Subscription> subscriptions)
    {
        bool hasAnyActivationSubscription = subscriptions.Any(s => s.HandlerType != null);
        bool hasResolveCallbackSet = _resolverCallback != null;
        if (hasAnyActivationSubscription && !hasResolveCallbackSet)
        {
            throw new NoResolveCallbackException("Can't activate handler, no resolve callback set.");
        }
    }

    private void RaiseForSubscription(object message, Subscription subscription)
    {
        try
        {
            if (subscription.Filter != null)
            {
                bool isFilterMatched = (bool)subscription.Filter.DynamicInvoke(message)!;
                if (!isFilterMatched)
                {
                    return;
                }
            }

            object? handlerResult;
            if (subscription.HandlerType != null)
            {
                Type handlerType = subscription.HandlerType!;
                object handler = _resolverCallback!(handlerType);

                handlerResult = subscription.Handler.DynamicInvoke(handler, message);
            }
            else
            {
                handlerResult = subscription.Handler.DynamicInvoke(message);
            }

            if (handlerResult is Task handlerTask)
                handlerTask.Wait();
        }
        catch (Exception e)
        {
            throw new EventBrokerageException("Error raising for subscription", e);
        }
    }

    private async Task RaiseForSubscriptionAsync(object message, Subscription subscription)
    {
        try
        {
            if (subscription.Filter != null)
            {
                bool isFilterMatched = (bool)subscription.Filter.DynamicInvoke(message)!;
                if (!isFilterMatched)
                {
                    return;
                }
            }

            object? handlerResult;
            if (subscription.HandlerType != null)
            {
                Type handlerType = subscription.HandlerType!;
                object handler = _resolverCallback!(handlerType);

                handlerResult = subscription.Handler.DynamicInvoke(handler, message);
            }
            else
            {
                handlerResult = subscription.Handler.DynamicInvoke(message);
            }

            if (handlerResult is Task handlerTask)
                await handlerTask;
        }
        catch (Exception e)
        {
            throw new EventBrokerageException("Error raising for subscription", e);
        }
    }

    public void SetResolverCallback(Func<Type, object> resolverCallback)
    {
        _resolverCallback = resolverCallback;
    }
}