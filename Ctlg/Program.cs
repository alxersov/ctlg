using System;
using System.Linq;
using System.Security.Cryptography;
using Autofac;
using AutoMapper;
using CommandLine;
using Ctlg.CommandLineOptions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Core.Utils;
using Ctlg.Data;
using Ctlg.Db.Migrations;
using Ctlg.Filesystem;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Ctlg.Service.Services;
using Ctlg.Service.Utils;
using Force.Crc32;

namespace Ctlg
{
    class Program: IProgram, IHandle<ErrorEvent>
    {
        private ICtlgService CtlgService { get; set; }
        private IMapper Mapper { get; }
        private int ExitCode { get; set; }

        static int Main(string[] args)
        {
            Logger.Info("ctlg {version}", AppVersion.Version);

            var container = BuildIocContainer();
            using (var scope = container.BeginLifetimeScope())
            {
                DomainEvents.Context = scope.Resolve<IComponentContext>();

                var program = scope.Resolve<IProgram>();
                return program.Run(args);
            }
        }

        public Program(ICtlgService ctlgService, IMapper mapper)
        {
            CtlgService = ctlgService;
            Mapper = mapper;
        }

        public int Run(string[] args)
        {
            ExitCode = 0;

            CtlgService.ApplyDbMigrations();

            Parser.Default.ParseArguments<Add, Backup, Find, List, Restore, Show, RebuildIndex, BackupPull>(args)
                .WithParsed<Add>(opts => Run<AddCommand>(opts))
                .WithParsed<Backup>(opts => Run<BackupCommand>(opts))
                .WithParsed<Find>(opts => Run<FindCommand>(opts))
                .WithParsed<List>(opts => Run<ListCommand>(opts))
                .WithParsed<Restore>(opts => Run<RestoreCommand>(opts))
                .WithParsed<Show>(opts => Run<ShowCommand>(opts))
                .WithParsed<RebuildIndex>(opts => Run<RebuildIndexCommand>(opts))
                .WithParsed<BackupPull>(opts => Run<BackupPullCommand>(opts))
                .WithNotParsed(errors => { ExitCode = 1; });

            NLog.LogManager.Shutdown();

            return ExitCode;
        }

        private void Run<T>(object options) where T : ICommand
        {
            var command = Mapper.Map<T>(options);

            try
            {
                command.Execute();
            }
            catch (Exception ex)
            {
                DomainEvents.Raise(new ErrorEvent(ex));
            }
        }

        private static IContainer BuildIocContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DataService>().As<IDataService>().InstancePerLifetimeScope();
            builder.RegisterType<MigrationService>().As<IMigrationService>().InstancePerLifetimeScope();

            if (IsMonoRuntime())
            {
                builder.RegisterType<FilesystemService>().As<IFilesystemService>().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<FilesystemServiceLongPath>().As<IFilesystemService>().InstancePerLifetimeScope();
            }
            builder.RegisterType<ArchiveService>().As<IArchiveService>().InstancePerLifetimeScope();
            builder.RegisterType<SnapshotService>().As<ISnapshotService>().InstancePerLifetimeScope();
            builder.RegisterCryptographyHashFunction<MD5Cng>("MD5");
            builder.RegisterCryptographyHashFunction<SHA1Cng>("SHA-1");
            builder.RegisterCryptographyHashFunction<SHA256Cng>("SHA-256");
            builder.RegisterCryptographyHashFunction<SHA384Cng>("SHA-384");
            builder.RegisterCryptographyHashFunction<SHA512Cng>("SHA-512");
            builder.RegisterCryptographyHashFunction<Crc32Algorithm>("CRC32");

            builder.RegisterType<CtlgContext>().As<ICtlgContext>().InstancePerLifetimeScope();
            builder.RegisterType<CtlgService>().As<ICtlgService>().InstancePerLifetimeScope();
            builder.RegisterType<FileStorageService>().As<IFileStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<IndexFileService>().As<IFileStorageIndexService>().InstancePerLifetimeScope();
            builder.RegisterType<BackupService>().As<IBackupService>().InstancePerLifetimeScope();
            builder.RegisterType<HashingService>().As<IHashingService>().InstancePerLifetimeScope();

            var genericHandlerType = typeof(IHandle<>);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => TypeHelper.IsAssignableToGenericType(t, genericHandlerType))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(AddCommand).Assembly)
                .Where(t => t.IsAssignableTo<ICommand>())
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<FileEnumerateStep>().As<ITreeProvider>().InstancePerLifetimeScope();
            builder.RegisterType<BackupWriter>().AsSelf().InstancePerDependency();
            builder.Register(context => CreateMappingConfiguration())
                .As<IConfigurationProvider>().InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var scope = context.Resolve<IComponentContext>();
                var configuration = context.Resolve<IConfigurationProvider>();

                return new Mapper(configuration, scope.Resolve);
            }).As<IMapper>().InstancePerLifetimeScope();

            return builder.Build();
        }

        private static IConfigurationProvider CreateMappingConfiguration()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Add, AddCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<Backup, BackupCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<Find, FindCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<List, ListCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<Restore, RestoreCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<Show, ShowCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<RebuildIndex, RebuildIndexCommand>().ConstructUsingServiceLocator();
                cfg.CreateMap<BackupPull, BackupPullCommand>().ConstructUsingServiceLocator();
            });
        }

        private static bool IsMonoRuntime()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public void Handle(ErrorEvent args)
        {
            ExitCode = 2;
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    }
}
