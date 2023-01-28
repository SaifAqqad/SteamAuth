using Ookii.CommandLine.Terminal;

namespace SteamAuth.Utils
{
    public class ConsoleWriter : IDisposable
    {
        private readonly VirtualTerminalSupport virtualTerminal;
        private readonly bool _formatSupported = false;

        public ConsoleWriter()
        {
            virtualTerminal = VirtualTerminal.EnableColor(StandardStream.Output);
            _formatSupported = virtualTerminal.IsSupported;
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
            virtualTerminal.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}