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

        private void RunAdd(Add options)
        {
            var command = Scope.Resolve<AddCommand>();

            command.Path = options.Path.First();
            command.SearchPattern = options.SearchPattern;
            command.HashFunctionName = options.HashFunctionName;

            command.Execute(CtlgService);
        }

        private void RunBackupCommand(Backup options)
        {
            var command = Scope.Resolve<BackupCommand>();
            
            command.Path = options.Path;
            command.Name = options.Name;
            command.IsFastMode = options.Fast;

            command.Execute(CtlgService);
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

            var command = Scope.Resolve<FindCommand>();

            command.Hash = options.Checksum;
            command.HashFunctionName = options.HashFunctionName;
            command.NamePattern = options.NamePattern;
            command.Size = options.Size;

            command.Execute(CtlgService);
        }

        private void RunListCommand(List options)
        {
            var command = Scope.Resolve<ListCommand>();

            command.Execute(CtlgService);
        }

        private void RunRestoreCommand(Restore options)
        {
            var command = Scope.Resolve<RestoreCommand>();

            command.Path = options.Path;
            command.Name = options.Name;
            
            command.Execute(CtlgService);		
        }

        private void RunShowCommand(Show options)
        {
            var ids = options.CatalogEntryIds.Select(int.Parse).ToList();

            var command = Scope.Resolve<ShowCommand>(new NamedParameter("catalogEntryIds", ids));

            command.Execute(CtlgService);
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
