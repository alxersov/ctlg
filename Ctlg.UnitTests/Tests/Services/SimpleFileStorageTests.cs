using System;
using Autofac;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.FileStorage;
using Ctlg.Service.Utils;
using Ctlg.UnitTests.Fixtures;
using Ctlg.UnitTests.TestDoubles;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class SimpleFileStorageTests : CommonDependenciesFixture
    {
        private readonly string HashString = "185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969";
        private Hash Hash1;

        [SetUp]
        public void Setup()
        {
            Hash1 = new Hash(DataServiceMock.GetHashAlgorithm("SHA-256"), FormatBytes.ToByteArray(HashString));
        }

        [Test]
        public void AddsFileFromOtherStorage()
        {
            var file = new File();
            file.Hashes.Add(Hash1);

            var sourceStorageMock = new Mock<IFileStorage>();
            sourceStorageMock
                .Setup(s => s.CopyFileTo(file, It.IsAny<string>()))
                .Callback((File f, string path) => FS.SetFile(path, "Hello"));

            var hashingService = AutoMock.Create<IHashingService>();
            var hashCalculator = hashingService.CreateHashCalculator("SHA-256");
            var storage = AutoMock.Create<SimpleFileStorage>(new NamedParameter("backupRoot", "foo"),
                new NamedParameter("hashCalculator", hashCalculator));

            storage.AddFileFromStorage(file, sourceStorageMock.Object);

            Assert.That(FS.GetFileAsString($"foo/file_storage/18/{HashString}"), Is.EqualTo("Hello"));
        }
    }
}
