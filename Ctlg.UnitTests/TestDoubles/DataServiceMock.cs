using System;
using System.Collections.Generic;
using System.Linq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;

namespace Ctlg.UnitTests.TestDoubles
{
    public class DataServiceMock : IDataService
    {
        public IList<HashAlgorithm> HashAlgorithms { get; }

        public DataServiceMock()
        {
            HashAlgorithms = new List<HashAlgorithm>(new[]
            {
                new HashAlgorithm() { HashAlgorithmId = 1000, Name = "SHA-256", Length = 32 }
            });
        }

        public void AddDirectory(File directory)
        {
            throw new NotImplementedException();
        }

        public void ApplyDbMigrations()
        {
            throw new NotImplementedException();
        }

        public File GetCatalogEntry(int catalogEntryId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<File> GetFiles()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<File> GetFiles(Hash hash, long? size, string namePattern)
        {
            throw new NotImplementedException();
        }

        public HashAlgorithm GetHashAlgorithm(string name)
        {
            var canonicalName = name.ToUpperInvariant();

            return HashAlgorithms.First(a => a.Name == canonicalName);
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }
}
