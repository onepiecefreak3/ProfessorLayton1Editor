using System;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Aspects;
using CrossCutting.Core.Contract.EventBrokerage.Exceptions;

namespace CrossCutting.Core.Contract.EventBrokerage;

[MapException(typeof(EventBrokerageException))]
public interface IEventBroker
{
    void Subscribe<THandler, TMessage>(Action<THandler, TMessage> handler, Func<TMessage, bool>? filter = null);
    void Subscribe<THandler, TMessage>(Func<THandler, TMessage, Task> handler, Func<TMessage, bool>? filter = null);
    void Subscribe<TMessage>(Action<TMessage> handler, Func<TMessage, bool>? filter = null);
    void Subscribe<TMessage>(Func<TMessage, Task> handler, Func<TMessage, bool>? filter = null);
    void Raise<TMessage>(TMessage message);
    Task RaiseAsync<TMessage>(TMessage message);
    void SetResolverCallback(Func<Type, object> resolverCallback);
}