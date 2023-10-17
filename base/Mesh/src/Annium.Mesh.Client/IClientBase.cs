using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Client;

public interface IClientBase : ILogSubject
{
    // broadcast/push
    IObservable<TNotification> Listen<TNotification>()
        where TNotification : NotificationBaseObsolete;

    // event
    void Notify<TEvent>(
        TEvent ev
    )
        where TEvent : EventBaseObsolete;

    // request -> void
    Task<IStatusResult<OperationStatus>> SendAsync(
        RequestBaseObsolete request,
        CancellationToken ct = default
    );

    // request -> response
    Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TResponse>(
        RequestBaseObsolete request,
        CancellationToken ct = default
    );

    // request -> response with default value
    Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TResponse>(
        RequestBaseObsolete request,
        TResponse defaultValue,
        CancellationToken ct = default
    );

    // subscription
    Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
        TInit request,
        CancellationToken ct = default
    )
        where TInit : SubscriptionInitRequestBaseObsolete;
}