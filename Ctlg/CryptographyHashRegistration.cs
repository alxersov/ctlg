using Autofac;
using Ctlg.Core.Interfaces;
using Ctlg.Filesystem;

namespace Ctlg
{
    public static class CryptographyHashRegistration
    {
        public static void RegisterCryptographyHashFunction<THashAlgorithm>(
            this ContainerBuilder builder,
            string algorithmName) where THashAlgorithm : System.Security.Cryptography.HashAlgorithm
        {
            builder.RegisterType<CryptographyHashFunction<THashAlgorithm>>()
                   .Named<IHashFunction>(algorithmName);
            builder.RegisterType<THashAlgorithm>().AsSelf();
        }
    }
}
