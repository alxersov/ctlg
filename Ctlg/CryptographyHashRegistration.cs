using System.Security.Cryptography;
using Autofac;
using Ctlg.Filesystem;

namespace Ctlg
{
    public static class CryptographyHashRegistration
    {
        public static void RegisterCryptographyHashFunction<THashAlgorithm>(this ContainerBuilder builder, string algorithmName) where THashAlgorithm : HashAlgorithm
        {
            builder.RegisterType<CryptographyHashFunction<THashAlgorithm>>().WithParameter("name", algorithmName).Named<IHashFunction>(algorithmName);
            builder.RegisterType<THashAlgorithm>().AsSelf();
        }
    }
}
