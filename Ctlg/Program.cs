using System;
using Autofac;
using Ctlg.Data.Service;
using Ctlg.Db.Migrations;
using Ctlg.Filesystem.Service;
using Ctlg.Service;

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
            builder.RegisterType<CtlgContext>().As<ICtlgContext>();
            builder.RegisterType<CtlgService>().As<ICtlgService>();
            builder.RegisterType<ConsoleOutput>().As<IOutput>();

            return builder.Build();
        }

        private static void ShowUsageInfo()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tctlg.exe add <path to directory>");
            Console.WriteLine("\tctlg.exe list");
        }
    }
}
