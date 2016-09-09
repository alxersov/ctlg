using System;

namespace Ctlg
{
    public class ConsoleTextAttributesScope: IDisposable
    {
        public ConsoleTextAttributesScope()
        {
            _backgroundColor = Console.BackgroundColor;
            _foregroundColor = Console.ForegroundColor;
        }

        public void Dispose()
        {
            Console.BackgroundColor = _backgroundColor;
            Console.ForegroundColor = _foregroundColor;
        }

        private readonly ConsoleColor _backgroundColor;
        private readonly ConsoleColor _foregroundColor;
    }
}
