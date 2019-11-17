using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Annium.Architecture.Base
{
    public class OperationStatus : Equatable<OperationStatus>
    {
        private static readonly List<OperationStatus> statuses = new List<OperationStatus>();

        public static OperationStatus BadRequest { get; } = Register(nameof(BadRequest));
        public static OperationStatus Conflict { get; } = Register(nameof(Conflict));
        public static OperationStatus Forbidden { get; } = Register(nameof(Forbidden));
        public static OperationStatus NotFound { get; } = Register(nameof(NotFound));
        public static OperationStatus OK { get; } = Register(nameof(OK));
        public static OperationStatus UncaughtException { get; } = Register(nameof(UncaughtException));

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

        public override IEnumerable<int> GetComponentHashCodes()
        {
            yield return name.GetHashCode();
        }
    }
}