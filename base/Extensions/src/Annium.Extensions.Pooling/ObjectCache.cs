// namespace Annium.Extensions.Pooling
// {
//     public class ObjectCache<T>
//     {
//         private readonly IConnectorFactory connectorFactory;
//         private readonly IDictionary<Guid, IConnector> connectors = new Dictionary<Guid, IConnector>();
//         private readonly IDictionary<Guid, ManualResetEventSlim> gates = new Dictionary<Guid, ManualResetEventSlim>();

//         public ConnectorPool(
//             IConnectorFactory connectorFactory
//         )
//         {
//             this.connectorFactory = connectorFactory;
//         }

//         public async Task<IConnector> Acquire(Guid id, Configuration configuration)
//         {
//             // whether current thread is creator of connector
//             var isCreator = false;
//             ManualResetEventSlim gate;

//             lock (connectors)
//             {
//                 // if connector is ready - just return it
//                 if (connectors.TryGetValue(id, out var connector))
//                     return connector;

//                 // if gate doesn't exist - create new one and act as connector creator
//                 if (!gates.TryGetValue(id, out gate!))
//                 {
//                     isCreator = true;
//                     gate = gates[id] = new ManualResetEventSlim(initialState: false);
//                 }
//             }

//             // if creator - call actual factory and send signal, when ready
//             if (isCreator)
//             {
//                 var connector = await connectorFactory.CreateAsync(configuration);
//                 lock (connectors)
//                     connectors[id] = connector;
//                 gate.Set();
//                 gate.Dispose();
//                 gates.Remove(id);

//                 return connector;
//             }
//             // else - wait for gate to be set and return connector, existing by the moment
//             else
//             {
//                 gate.Wait();

//                 return connectors[id];
//             }
//         }

//         public void Release(Guid id)
//         {
//             IConnector connector;
//             lock (connectors)
//             {
//                 if (connectors.TryGetValue(id, out connector!))
//                     connectors.Remove(id);
//                 else
//                     return;
//             }
//             connector.Dispose();
//         }
//     }
// }