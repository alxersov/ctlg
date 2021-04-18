using System;
using System.Security.Cryptography;
using Autofac;
using AutoMapper;
using Ctlg.CommandLineOptions;
using Ctlg.Core.Interfaces;
using Ctlg.Filesystem;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Services;
using Ctlg.Service.Utils;

namespace Ctlg
{
    public static class ContainerConfiguration
    {
        public static void RegisterExternalDependencies(this ContainerBuilder builder, bool isRunningOnMono)
        {
            if (isRunningOnMono)
            {
                builder.RegisterType<FilesystemService>().As<IFilesystemService>().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<FilesystemServiceLongPath>().As<IFilesystemService>().InstancePerLifetimeScope();
            }
        }

        public static void RegisterCommonDependencies(this ContainerBuilder builder)
        {
            builder.RegisterType<SnapshotService>().As<ISnapshotService>().InstancePerLifetimeScope();
            builder.RegisterType<TextFileSnapshotFactory>().Named<ISnapshotFactory>("TXT").InstancePerLifetimeScope();
            builder.RegisterCryptographyHashFunction<MD5Cng>("MD5");
            builder.RegisterCryptographyHashFunction<SHA1Cng>("SHA-1");
            builder.RegisterCryptographyHashFunction<SHA256Cng>("SHA-256");
            builder.RegisterCryptographyHashFunction<SHA384Cng>("SHA-384");
            builder.RegisterCryptographyHashFunction<SHA512Cng>("SHA-512");

            builder.RegisterType<FileStorageService>().As<IFileStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<IndexFileService>().As<IFileStorageIndexService>().InstancePerLifetimeScope();
            builder.RegisterType<BackupService>().As<IBackupService>().InstancePerLifetimeScope();
            builder.RegisterType<HashingService>().As<IHashingService>().InstancePerLifetimeScope();
            builder.RegisterType<FileEnumerateStep>().As<ITreeProvider>().InstancePerLifetimeScope();
            builder.RegisterType<JsonConfigService>().As<IConfigService>().InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(BackupCommand).Assembly)
                .Where(t => t.IsAssignableTo<ICommand>())
                .AsSelf()
                .InstancePerDependency();
        }

        public static void RegisterDomainEventHandlers(this ContainerBuilder builder)
        {
            var genericHandlerType = typeof(IHandle<>);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => TypeHelper.IsAssignableToGenericType(t, genericHandlerType))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }

        public static void RegisterAutoMapperMappings(this ContainerBuilder builder)
        {
            builder.Register(context => CreateMappingConfiguration())
                .As<IConfigurationProvider>().InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var scope = context.Resolve<IComponentContext>();
                var configuration = context.Resolve<IConfigurationProvider>();

                return new Mapper(configuration, scope.Resolve);
            }).As<IMapper>().InstancePerLifetimeScope();
        }

        private static IConfigurationProvider CreateMappingConfiguration()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Backup, BackupCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<Restore, RestoreCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<RebuildIndex, RebuildIndexCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<BackupPull, BackupPullCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<Fsck, FsckCommand>().ConstructUsingServiceLocator();
            });
        }
    }
}
