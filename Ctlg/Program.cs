using System;
using System.Security.Cryptography;
using Autofac;
using Ctlg.Data.Service;
using Ctlg.Db.Migrations;
using Ctlg.Filesystem.Service;
using Ctlg.Service;
using Ctlg.Service.Utils;

namespace Ctlg
{
    class Program
    {
        static int Main(string[] args)
        {
            var parser = new ArgsParser();
            var command = parser.Parse(args);
            if (command != null)
            {
                var container = BuildIocContainer();

                using (var scope = container.BeginLifetimeScope())
                {
                    DomainEvents.Container = scope;

                    var svc = scope.Resolve<ICtlgService>();
                    svc.ApplyDbMigrations();
                    svc.Execute(command);
                }
            }
            else
            {
                ShowUsageInfo();
                return 1;
            }

            return 0;
        }

        private static IContainer BuildIocContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DataService>().As<IDataService>();
            builder.RegisterType<MigrationService>().As<IMigrationService>();
            builder.RegisterType<FilesystemService>().As<IFilesystemService>();
            builder.RegisterType<HashService>().As<IHashService>();
            builder.RegisterType<CtlgContext>().As<ICtlgContext>();
            builder.RegisterType<CtlgService>().As<ICtlgService>();

            var genericHandlerType = typeof(IHandle<>);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => TypeHelper.IsAssignableToGenericType(t, genericHandlerType))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<SHA1Cng>().As<SHA1>();

            return builder.Build();
        }

        private static void ShowUsageInfo()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tctlg.exe add <path to directory>");
            Console.WriteLine("\tctlg.exe find <hash>");
            Console.WriteLine("\tctlg.exe list");
        }
    }
}
