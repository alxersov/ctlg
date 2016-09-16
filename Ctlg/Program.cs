using System;
using System.Linq;
using System.Security.Cryptography;
using Autofac;
using Ctlg.CommandLineOptions;
using Ctlg.Data.Service;
using Ctlg.Db.Migrations;
using Ctlg.Filesystem.Service;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Utils;


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
            builder.RegisterType<FilesystemService>().As<IFilesystemService>();
            builder.RegisterType<Sha1HashFunction>().Named<IHashFunction>("SHA-1");
            builder.RegisterType<Sha256HashFunction>().Named<IHashFunction>("SHA-256");
            builder.RegisterType<CtlgContext>().As<ICtlgContext>();
            builder.RegisterType<CtlgService>().As<ICtlgService>();

            var genericHandlerType = typeof(IHandle<>);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => TypeHelper.IsAssignableToGenericType(t, genericHandlerType))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<SHA1Cng>().As<SHA1>();
            builder.RegisterType<SHA256Cng>().As<SHA256>();

            return builder.Build();
        }
    }
}
