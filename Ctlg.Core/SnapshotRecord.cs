using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Ctlg.Service.Utils;

namespace Ctlg.Core
{
    public class SnapshotRecord
    {
        public Hash Hash { get; set; }
        public long Size { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }

        public SnapshotRecord(Hash hash, DateTime date, long size, string name)
        {
            Hash = hash;
            Date = date;
            Size = size;
            Name = name;
        }

        public SnapshotRecord(string snapshotFileLine, HashAlgorithm hashAlgorithm)
        {
            var match = BackupLineRegex.Match(snapshotFileLine);

            if (!match.Success)
            {
                throw new Exception($"Unexpected list line {snapshotFileLine}.");
            }

            Hash = new Hash(hashAlgorithm, FormatBytes.ToByteArray(match.Groups["hash"].Value));
            Size = long.Parse(match.Groups["size"].Value);
            Date = DateTime.ParseExact(match.Groups["date"].Value, "O", CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            Name = match.Groups["name"].Value;
        }

        public override string ToString()
        {
            return $"{Hash} {Date:o} {Size} {Name}";
        }

        private static Regex BackupLineRegex = new Regex(@"^(?<hash>[a-h0-9]{64,})\s(?<date>[0-9:.TZ-]{19,28})\s(?<size>[0-9]{1,10})\s(?<name>\S.*)$", RegexOptions.IgnoreCase);

    }
}
