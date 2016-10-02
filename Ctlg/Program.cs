using System;
using System.Linq;
using System.Security.Cryptography;
using Autofac;
using Ctlg.CommandLineOptions;
using Ctlg.Data;
using Ctlg.Db.Migrations;
using Ctlg.Filesystem.Service;
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
            var options = new Options();
            ICommand command = null;
            CommandLine.Parser.Default.ParseArguments(args, options,
                (verb, subOptions) =>
                {
                    command = CreateCommand(verb, subOptions);
                });
            
            if (command == null)
            {
                return 1;
            }

            var container = BuildIocContainer();
            using (var scope = container.BeginLifetimeScope())
            {
                DomainEvents.Container = scope;

                var svc = scope.Resolve<ICtlgService>();
                svc.ApplyDbMigrations();
                svc.Execute(command);
            }

            return 0;
        }

        private static ICommand CreateCommand(string commandName, object options)
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
                        command = new AddCommand
                        {
                            Path = add.Path.First(),
                            SearchPattern = add.SearchPattern,
                            HashFunctionName = add.HashFunctionName
                        };
                        break;
                    case "find":
                        var find = (Find) options;
                        command = new FindCommand {Hash = find.Hash};
                        break;
                    case "list":
                        command = new ListCommand();
                        break;
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Bad arguments supplied for {0} command. To get help on {0} comand please run ctlg {0} --help.", command);
            }

            return command;
        }

        private static IContainer BuildIocContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DataService>().As<IDataService>();
            builder.RegisterType<MigrationService>().As<IMigrationService>();
            builder.RegisterType<FilesystemServiceLongPath>().As<IFilesystemService>();

            builder.RegisterCryptographyHashFunction<MD5Cng>("MD5");
            builder.RegisterCryptographyHashFunction<SHA1Cng>("SHA-1");
            builder.RegisterCryptographyHashFunction<SHA256Cng>("SHA-256");
            builder.RegisterCryptographyHashFunction<SHA384Cng>("SHA-384");
            builder.RegisterCryptographyHashFunction<SHA512Cng>("SHA-512");
            builder.RegisterCryptographyHashFunction<Crc32Algorithm>("CRC32");

            builder.RegisterType<CtlgContext>().As<ICtlgContext>();
            builder.RegisterType<CtlgService>().As<ICtlgService>();

            var genericHandlerType = typeof(IHandle<>);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => TypeHelper.IsAssignableToGenericType(t, genericHandlerType))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}
