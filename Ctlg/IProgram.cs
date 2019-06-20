using System;
using Autofac;

namespace Ctlg
{
  public interface IProgram
    {
    int Run(ILifetimeScope scope, string[] args);
    }
}
