using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Annium.Architecture.Base
{
    public class OperationStatus : Equatable<OperationStatus>
    {
        private static List<OperationStatus> statuses = new List<OperationStatus>();

        public static OperationStatus BadRequest { get; } = OperationStatus.Register(nameof(BadRequest));
        public static OperationStatus Forbidden { get; } = OperationStatus.Register(nameof(Forbidden));
        public static OperationStatus NotFound { get; } = OperationStatus.Register(nameof(NotFound));
        public static OperationStatus OK { get; } = OperationStatus.Register(nameof(OK));
        public static OperationStatus UncaughtException { get; } = OperationStatus.Register(nameof(UncaughtException));

        public static OperationStatus Register(string name)
        {
            var status = new OperationStatus(name);

            if (statuses.FindIndex(e => e.name == name) < 0)
                statuses.Add(status);

            return status;
        }

        public static OperationStatus Get(string name) => statuses.Find(e => e.name == name) ??
        throw new Exception($"Operation status {name} is not registered.");

        private readonly string name;

        private OperationStatus(string name)
        {
            this.name = name;
        }

        public override string ToString() => name;

        public override int GetHashCode() => name.GetHashCode();
    }
}