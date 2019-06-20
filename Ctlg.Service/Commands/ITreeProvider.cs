using System;
using Ctlg.Core;

namespace Ctlg.Service.Commands
{
    public interface ITreeProvider
    {
        File ReadTree(string path);
    }
}
