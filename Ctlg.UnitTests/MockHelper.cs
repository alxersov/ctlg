using System;
using System.IO;
using Autofac.Extras.Moq;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Moq;

namespace Ctlg.UnitTests
{
    public static class MockHelper
    {
        public static void SetupOpenFileForRead(this Mock<IFilesystemService> mock, string path, byte[] content)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            mock.Setup(s => s.OpenFileForRead(path)).Returns(stream);
        }

        public static void SetupOpenFileForRead(this AutoMock mock, string path, string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            mock.Mock<IFilesystemService>().Setup(s => s.OpenFileForRead(path)).Returns(stream);
        }

        public static void SetupPath(this Mock<IFilesystemService> mock,
            string path1, string path2, string result)
        {
            mock.Setup(m => m.CombinePath(path1, path2)).Returns(result);
            mock.Setup(m => m.GetDirectoryName(result)).Returns(path1);
        }

        public static void SetupPath(this Mock<IFilesystemService> mock,
            string path1, string path2, string path3, string result)
        {
            mock.Setup(m => m.CombinePath(path1, path2, path3)).Returns(result);
        }
    }
}
