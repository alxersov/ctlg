using System;
using System.Collections.Generic;

namespace Ctlg.Service.Commands
{
    public class ShowCommand : ICommand
    {
        public ShowCommand(IList<int> catalogEntryIds)
        {
            CatalogEntryIds = catalogEntryIds;
        }

        public void Execute(ICtlgService ctlgService)
        {
            foreach (var id in CatalogEntryIds)
            {
                ctlgService.Show(id);
            }
        }

        public IList<int> CatalogEntryIds { get; }
    }
}
