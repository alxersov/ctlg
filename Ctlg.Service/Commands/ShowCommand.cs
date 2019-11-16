using System;
using System.Collections.Generic;
using Ctlg.Core.Interfaces;

namespace Ctlg.Service.Commands
{
    public class ShowCommand : ICommand
    {
        public ShowCommand(ICtlgService ctlgService)
        {
            CtlgService = ctlgService;
        }

        public void Execute()
        {
            foreach (var id in CatalogEntryIds)
            {
                CtlgService.Show(id);
            }
        }

        public IList<int> CatalogEntryIds { get; set; }
        private ICtlgService CtlgService { get; }
    }
}
