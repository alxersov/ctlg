using System;
using System.Security.Cryptography;
using Autofac;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Service.FileStorage;
using Ctlg.Service.Services;
using Ctlg.Service.Utils;
using Ctlg.UnitTests.Fixtures;
using Ctlg.UnitTests.TestDoubles;
using Moq;
using NUnit.Framework;

namespace Ctlg.UnitTests.Tests.Services
{
    public class SimpleFileStorageTests : AutoMockTestFixture
    {
        private readonly string HashString = "185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969";
        private readonly Hash Hash1;

        private VirtualFileSystem VirtualFileSystem;

        public SimpleFileStorageTests()
        {
            Hash1 = new Hash(HashAlgorithmId.SHA256, FormatBytes.ToByteArray(HashString));
            VirtualFileSystem = new VirtualFileSystem();
        }

        [Test]
        public void AddsFileFromOtherStorage()
        {
            var sourceStorageMock = new Mock<IFileStorage>();
            sourceStorageMock
                .Setup(s => s.CopyFileTo(Hash1.ToString(), It.IsAny<string>()))
                .Callback((string hash, string path) => VirtualFileSystem.SetFile(path, "Hello"));

            var storage = AutoMock.Create<SimpleFileStorage>(new[] { new NamedParameter("backupRoot", "foo") });

            var file = new File();
            file.Hashes.Add(Hash1);
            storage.AddFileFromStorage(file, sourceStorageMock.Object);

            Assert.That(VirtualFileSystem.GetFileAsString($"foo/file_storage/18/{HashString}"), Is.EqualTo("Hello"));
        }

        protected override void ConfigureDependencies(ContainerBuilder builder)
        {
            builder.RegisterInstance<IFilesystemService>(VirtualFileSystem);
            builder.RegisterType<HashingService>().As<IHashingService>().InstancePerLifetimeScope();
            builder.RegisterCryptographyHashFunction<SHA256Cng>("SHA-256", HashAlgorithmId.SHA256);
        }
    }
}
