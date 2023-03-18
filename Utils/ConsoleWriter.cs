using Ookii.CommandLine.Terminal;

namespace SteamAuth.Utils
{
    public class ConsoleWriter : IDisposable
    {
        private readonly VirtualTerminalSupport _virtualTerminal;
        private readonly bool _formatSupported = false;

        public ConsoleWriter()
        {
            _virtualTerminal = VirtualTerminal.EnableColor(StandardStream.Output);
            _formatSupported = _virtualTerminal.IsSupported;
        }

        public void Write(string text, params string[] formats)
        {
            if (_formatSupported)
            {
                Console.Write(formats);
            }

            Console.Write(text);
        }

        public void WriteLine(string text, params string[] formats)
        {
            if (_formatSupported)
            {
                Console.Write(string.Concat(formats));
            }

            Console.WriteLine(text);
        }

        public void Dispose()
        {
            _virtualTerminal.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}