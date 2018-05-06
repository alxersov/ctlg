using System;
using System.Linq;
using System.Security.Cryptography;
using Autofac;
using Ctlg.CommandLineOptions;
using Ctlg.Core;
using Ctlg.Core.Interfaces;
using Ctlg.Data;
using Ctlg.Db.Migrations;
using Ctlg.Filesystem;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Utils;
using Force.Crc32;


namespace Ctlg
{
    class Program
    {
        static int Main(string[] args)
        {
            var container = BuildIocContainer();
            using (var scope = container.BeginLifetimeScope())
            {
                DomainEvents.Container = scope;

                var svc = scope.Resolve<ICtlgService>();
                svc.ApplyDbMigrations();

                ICommand command = CreateCommand(args, scope);

                if (command == null)
                {
                    return 1;
                }

                command.Execute(svc);
            }

            return 0;
        }

        private static ICommand CreateCommand(string[] args, ILifetimeScope scope)
        {
            ICommand command = null;
            var options = new Options();
            CommandLine.Parser.Default.ParseArguments(args, options,
                (verb, subOptions) =>
                {
                    command = CreateCommand(verb, subOptions, scope);
                });
            return command;
        }

        private static ICommand CreateCommand(string commandName, object options, ILifetimeScope scope)
        {
            if (commandName == null || options == null)
            {
                return null;
            }

            commandName = commandName.ToLowerInvariant();

            ICommand command = null;

            try
            {
                switch (commandName)
                {
                    case "add":
                        var add = (Add) options;
                        var addCommand = scope.Resolve<AddCommand>();

                        addCommand.Path = add.Path.First();
                        addCommand.SearchPattern = add.SearchPattern;
                        addCommand.HashFunctionName = add.HashFunctionName;

                        command = addCommand;
                        break;
                    case "find":
                        var find = (Find) options;

                        if (find.Checksum != null && find.HashFunctionName == null)
                        {
                            Console.Error.WriteLine("Checksum value parameter requires Hash function to be provided.");
                            throw new InvalidOperationException();
                        }

                        if (find.Checksum == null &&
                            find.Size == null &&
                            find.NamePattern == null)
                        {
                            Console.Error.WriteLine("No parameters provided.");
                            throw new InvalidOperationException();
                        }

                        command = new FindCommand
                        {
                            Hash = find.Checksum,
                            HashFunctionName = find.HashFunctionName,
                            NamePattern = find.NamePattern,
                            Size = find.Size
                        };
                        break;
                    case "list":
                        command = new ListCommand();
                        break;
                    case "show":
                        var show = (Show) options;
                        command = new ShowCommand(show.CatalogEntryIds.Select(int.Parse).ToList());
                        break;
                    case "backup":
                        var backup = (Backup)options;
                        var backupCommand = scope.Resolve<BackupCommand>();

                        backupCommand.Path = backup.Path;
                        backupCommand.Name = backup.Name;
                        backupCommand.IsFastMode = backup.Fast;
                        command = backupCommand;
                        break;
                    case "restore":
                        var restore = (Restore)options;
                        var restoreCommand = scope.Resolve<RestoreCommand>();

                        restoreCommand.Path = restore.Path;
                        restoreCommand.Name = restore.Name;
                        command = restoreCommand;
                        break;
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Bad arguments supplied for {0} command. To get help on {0} comand please run ctlg {0} --help.", commandName);
                throw;
            }

            return command;
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
    }
}
