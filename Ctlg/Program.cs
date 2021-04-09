using System;
using Autofac;
using AutoMapper;
using CommandLine;
using Ctlg.CommandLineOptions;
using Ctlg.Core.Interfaces;
using Ctlg.Core.Utils;
using Ctlg.Service;
using Ctlg.Service.Commands;
using Ctlg.Service.Events;

namespace Ctlg
{
    class Program: IProgram, IHandle<ErrorEvent>
    {
        private ICtlgService CtlgService { get; set; }
        private IMapper Mapper { get; }
        private IConfigService ConfigService { get; }
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

        public Program(ICtlgService ctlgService, IMapper mapper, IConfigService configService)
        {
            CtlgService = ctlgService;
            Mapper = mapper;
            ConfigService = configService;
        }

        public int Run(string[] args)
        {
            ExitCode = 0;

            CtlgService.ApplyDbMigrations();

            Parser.Default.ParseArguments<Backup, Restore, RebuildIndex, BackupPull, Fsck>(args)
                .WithParsed<Backup>(opts => Run<BackupCommand>(opts))
                .WithParsed<Restore>(opts => Run<RestoreCommand>(opts))
                .WithParsed<RebuildIndex>(opts => Run<RebuildIndexCommand>(opts))
                .WithParsed<BackupPull>(opts => Run<BackupPullCommand>(opts))
                .WithParsed<Fsck>(opts => Run<FsckCommand>(opts))
                .WithNotParsed(errors => { ExitCode = 1; });

            NLog.LogManager.Shutdown();

            return ExitCode;
        }

        private void Run<T>(object options) where T : ICommand
        {
            var command = Mapper.Map<T>(options);
            var config = ConfigService.LoadConfig();

            try
            {
                command.Execute(config);
            }
            catch (Exception ex)
            {
                DomainEvents.Raise(new ErrorEvent(ex));
            }
        }

        private static IContainer BuildIocContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterExternalDependencies(IsMonoRuntime());
            builder.RegisterCommonDependencies();
            builder.RegisterDomainEventHandlers();
            builder.RegisterAutoMapperMappings();

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

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    }
}
