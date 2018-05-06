using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ctlg.Core
{
    public class SnapshotRecord
    {
        public string Hash { get; set; }
        public long Size { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }

        public SnapshotRecord(string snapshotFileLine)
        {
            var match = BackupLineRegex.Match(snapshotFileLine);

            if (!match.Success)
            {
                throw new Exception($"Unexpected list line {snapshotFileLine}.");
            }

            Hash = match.Groups["hash"].Value;
            Size = long.Parse(match.Groups["size"].Value);
            Date = DateTime.ParseExact(match.Groups["date"].Value, "O", CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            Name = match.Groups["name"].Value;
        }

        private static Regex BackupLineRegex = new Regex(@"^(?<hash>[a-h0-9]{64})\s(?<date>[0-9:.TZ-]{19,28})\s(?<size>[0-9]{1,10})\s(?<name>\S.*)$", RegexOptions.IgnoreCase);

    }
}
