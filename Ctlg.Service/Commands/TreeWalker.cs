using System;
using Ctlg.Core;

namespace Ctlg.Service.Commands
{
    public class TreeWalker
    {
        public TreeWalker(File root)
        {
            _root = root;
        }

        public void Walk(Action<File> action)
        {
            ProcessTree(_root, action);
        }

        private void ProcessTree(File directory, Action<File> action)
        {
            foreach (var file in directory.Contents)
            {
                if (file.IsDirectory)
                {
                    ProcessTree(file, action);
                }
                else
                {
                    action(file);
                }
            }
        }

        private File _root;
    }
}
