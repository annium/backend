using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Annium.Architecture.Base
{
    public class OperationStatus : Equatable<OperationStatus>
    {
        private static readonly IDictionary<string, OperationStatus> Statuses = new Dictionary<string, OperationStatus>();

        public static OperationStatus BadRequest { get; } = Register(nameof(BadRequest));
        public static OperationStatus Conflict { get; } = Register(nameof(Conflict));
        public static OperationStatus Forbidden { get; } = Register(nameof(Forbidden));
        public static OperationStatus NotFound { get; } = Register(nameof(NotFound));
        public static OperationStatus OK { get; } = Register(nameof(OK));
        public static OperationStatus UncaughtException { get; } = Register(nameof(UncaughtException));

        public static OperationStatus Register(string name)
        {
            if (Statuses.TryGetValue(name, out var status))
                return status;

            return Statuses[name] = new OperationStatus(name);
        }

        public static OperationStatus Get(string name)
        {
            if (!Statuses.TryGetValue(name, out var status))
                throw new Exception($"Operation status {name} is not registered.");

            return status;
        }

        private readonly string name;

        private OperationStatus(string name)
        {
            this.name = name;
        }

        public override int GetHashCode() => name.GetHashCode();

        public override string ToString() => name;
    }
}