using System.CodeDom.Compiler;

namespace NetCore.InternalAutoGen
{
    public static class NetworkMethodHelpers
    {
        static char[] Buffer = new char[1024];
        static readonly object _lock = new();
        /// <summary>
        /// Where all network method interfaces are located.
        /// </summary>
        public const string Namespace = nameof(NetCore);
        public static void SplitAndWriteLine(this IndentedTextWriter writer, in string code)
        {
            lock (_lock)
            {
                int last = 0;
                int length;
                while (true)
                {
                    const string NewLine = "\r\n";
                    int index = code.IndexOf(NewLine, last);
                    if (index == -1) break;

                    length = index - last;
                    if (length > Buffer.Length)
                    {
                        Buffer = new char[length]; // Implementing proper NextPoT would have been better.
                    }

                    code.CopyTo(last, Buffer, 0, length);
                    writer.WriteLine(Buffer, 0, length); // We split lines for writer for handling indentation.
                    last = index + NewLine.Length;
                }

                length = code.Length - last;
                code.CopyTo(last, Buffer, 0, length);
                writer.WriteLine(Buffer, 0, length);
            }    
        }
    }
}
