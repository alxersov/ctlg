using System.Security.Cryptography;
using Autofac;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Filesystem;

namespace Ctlg
{
    public static class CryptographyHashRegistration
    {
        public static void RegisterCryptographyHashFunction<THashAlgorithm>(
            this ContainerBuilder builder,
            string algorithmName,
            HashAlgorithmId algorithmId) where THashAlgorithm : System.Security.Cryptography.HashAlgorithm
        {
            builder.RegisterType<CryptographyHashFunction<THashAlgorithm>>()
                   .WithParameter("name", algorithmName)
                   .WithParameter("algorithmId", (int)algorithmId)
                   .Named<IHashFunction>(algorithmName);
            builder.RegisterType<THashAlgorithm>().AsSelf();
        }
    }
}
