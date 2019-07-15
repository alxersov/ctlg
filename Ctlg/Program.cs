using System;
using System.Linq;
using System.Security.Cryptography;
using Autofac;
using CommandLine;
using Ctlg.CommandLineOptions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Data;
using Ctlg.Db.Migrations;
using Ctlg.Filesystem;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;
using Ctlg.Service.Utils;
using Force.Crc32;


namespace Ctlg
{
    class Program: IProgram, IHandle<ErrorEvent>
    {
        private ICtlgService CtlgService { get; set; }
        private ILifetimeScope Scope { get; set; }
        private int ExitCode { get; set; }

        static int Main(string[] args)
        {
            var container = BuildIocContainer();
            using (var scope = container.BeginLifetimeScope())
            {
                DomainEvents.Container = scope;

                var program = scope.Resolve<IProgram>();
                return program.Run(scope, args);
            }
        }

        public Program(ICtlgService ctlgService)
        {
            CtlgService = ctlgService;
        }

        public int Run(ILifetimeScope scope, string[] args)
        {
            ExitCode = 0;

            Scope = scope;

            CtlgService.ApplyDbMigrations();

            Parser.Default.ParseArguments<Add, Backup, Find, List, Restore, Show>(args)
                .WithParsed<Add>(opts => RunAdd(opts))
                .WithParsed<Backup>(opts => RunBackupCommand(opts))
                .WithParsed<Find>(opts => RunFindCommand(opts))
                .WithParsed<List>(opts => RunListCommand(opts))
                .WithParsed<Restore>(opts => RunRestoreCommand(opts))
                .WithParsed<Show>(opts => RunShowCommand(opts))
                .WithNotParsed(errors => { ExitCode = 1; });

            return ExitCode;
        }

        private void SetupAndExecute<T>(Action<T> setupActon) where T : ICommand
        {
            var command = Scope.Resolve<T>();

            setupActon(command);

            command.Execute();
        }

        private void RunAdd(Add options)
        {
            SetupAndExecute<AddCommand>(command => {
                command.Path = options.Path.First();
                command.SearchPattern = options.SearchPattern;
                command.HashFunctionName = options.HashFunctionName;
            });
        }

        private void RunBackupCommand(Backup options)
        {
            SetupAndExecute<BackupCommand>(command => {
                command.Path = options.Path;
                command.SearchPattern = options.SearchPattern;
                command.SnapshotName = options.Name;
                command.IsFastMode = options.Fast;
            });
        }

        private void RunFindCommand(Find options)
        {
            if (options.Checksum != null && options.HashFunctionName == null)
            {
                DomainEvents.Raise(new ErrorEvent("Checksum value parameter requires Hash function to be provided."));
                return;
            }

            if (options.Checksum == null &&
                options.Size == null &&
                options.NamePattern == null)
            {
                DomainEvents.Raise(new ErrorEvent("No parameters provided."));
                return;
            }

            SetupAndExecute<FindCommand>(command => {
                command.Hash = options.Checksum;
                command.HashFunctionName = options.HashFunctionName;
                command.NamePattern = options.NamePattern;
                command.Size = options.Size;
            });
        }

        private void RunListCommand(List options)
        {
            SetupAndExecute<ListCommand>(command => {});
        }

        private void RunRestoreCommand(Restore options)
        {
            SetupAndExecute<RestoreCommand>(command => {
                command.Path = options.Path;
                command.Name = options.Name;
                command.Date = options.Date;
            });
        }

        private void RunShowCommand(Show options)
        {
            var ids = options.CatalogEntryIds.Select(int.Parse).ToList();

            SetupAndExecute<ShowCommand>(command => {
                command.CatalogEntryIds = ids;
            });
        }

        private static IContainer BuildIocContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DataService>().As<IDataService>();
            builder.RegisterType<MigrationService>().As<IMigrationService>();

            if (IsMonoRuntime())
            {
                builder.RegisterType<FilesystemService>().As<IFilesystemService>();
            }
            else
            {
                builder.RegisterType<FilesystemServiceLongPath>().As<IFilesystemService>();
            }
            builder.RegisterType<ArchiveService>().As<IArchiveService>();
            builder.RegisterType<SnapshotService>().As<ISnapshotService>();
            builder.RegisterCryptographyHashFunction<MD5Cng>("MD5", HashAlgorithmId.MD5);
            builder.RegisterCryptographyHashFunction<SHA1Cng>("SHA-1", HashAlgorithmId.SHA1);
            builder.RegisterCryptographyHashFunction<SHA256Cng>("SHA-256", HashAlgorithmId.SHA256);
            builder.RegisterCryptographyHashFunction<SHA384Cng>("SHA-384", HashAlgorithmId.SHA384);
            builder.RegisterCryptographyHashFunction<SHA512Cng>("SHA-512", HashAlgorithmId.SHA512);
            builder.RegisterCryptographyHashFunction<Crc32Algorithm>("CRC32", HashAlgorithmId.CRC32);

            builder.RegisterType<CtlgContext>().As<ICtlgContext>();
            builder.RegisterType<CtlgService>().As<ICtlgService>();

            var genericHandlerType = typeof(IHandle<>);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => TypeHelper.IsAssignableToGenericType(t, genericHandlerType))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(AddCommand).Assembly)
                .Where(t => t.IsAssignableTo<ICommand>())
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterType<FileEnumerateStep>().As<ITreeProvider>();
            builder.RegisterType<SnapshotReader>().As<ISnapshotReader>();


            return builder.Build();
        }

        private static bool IsMonoRuntime()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public void Handle(ErrorEvent args)
        {
            ExitCode = 2;
        }
    }
}
