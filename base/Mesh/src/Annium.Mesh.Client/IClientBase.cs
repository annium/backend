using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Mesh.Client;

public interface IClientBase : ILogSubject
{
    // broadcast/push
    IObservable<TNotification> Listen<TNotification>();

    // // event
    // void Notify<TEvent>(
    //     TEvent ev
    // )
    //     where TEvent : EventBaseObsolete;

    // request -> void
    Task<IStatusResult<OperationStatus>> SendAsync(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    );

    // request -> response
    Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    )
        where TData : notnull;

    // request -> response with default value
    Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        TData defaultValue,
        CancellationToken ct = default
    )
        where TData : notnull;

    // // subscription
    // Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
    //     TInit request,
    //     CancellationToken ct = default
    // )
    //     where TInit : SubscriptionInitRequestBaseObsolete;
}
