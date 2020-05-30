using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Indexed;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Services
{
    public class SnapshotService : ISnapshotService
    {
        public SnapshotService(IIndex<string, ISnapshotFactory> snapshotFactories)
        {
            SnapshotFactories = snapshotFactories;
        }

        public ISnapshot FindSnapshot(Config config, string name, string timestampMask)
        {
            var factory = GetFactory(config.SnapshotServiceName);

            var allTimestamps = factory.GetTimestamps(config, name);
            if (allTimestamps.Count == 0)
            {
                return null;
            }

            var timestamp = SelectSnapshotByDate(allTimestamps, timestampMask);
            if (string.IsNullOrEmpty(timestamp))
            {
                return null;
            }

            return factory.GetSnapshot(config, name, timestamp);
        }

        public ISnapshot CreateSnapshot(Config config, string name, string timestamp)
        {
            return GetFactory(config.SnapshotServiceName).GetSnapshot(config, name, timestamp);
        }

        private ISnapshotFactory GetFactory(string name)
        {
            var canonicalName = name.ToUpperInvariant();
            if (!SnapshotFactories.TryGetValue(canonicalName, out ISnapshotFactory factory))
            {
                throw new Exception($"Unsupported snapshot type {name}");
            }

            return factory;
        }

        private string SelectSnapshotByDate(IEnumerable<string> timestamps, string timestampMask)
        {
            if (string.IsNullOrEmpty(timestampMask))
            {
                return timestamps.Last();
            }

            var found = timestamps
                .Where(s => s.StartsWith(timestampMask, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (found.Count > 1)
            {
                throw new Exception(
                    $"Provided snapshot date is ambiguous. {found.Count} snapshots exist: {string.Join(", ", found)}.");
            }

            return found.FirstOrDefault();
        }

        private IIndex<string, ISnapshotFactory> SnapshotFactories { get; }
    }
}
