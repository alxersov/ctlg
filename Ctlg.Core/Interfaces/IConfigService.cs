using System;
namespace Ctlg.Core.Interfaces
{
    public interface IConfigService
    {
        Config LoadConfig(string path = null);
    }
}
