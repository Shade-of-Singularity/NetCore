using System.CodeDom.Compiler;

namespace NetCore.InternalAutoGen
{
    public static class NetworkMethodHelpers
    {
        /// <summary>
        /// Where all network method interfaces are located.
        /// </summary>
        public const string Namespace = nameof(NetCore);
        public static void SplitAndWriteLine(this IndentedTextWriter writer, in string code)
        {
            char[] buffer = new char[1024];
            int last = 0;
            int length;
            while (true)
            {
                const string NewLine = "\r\n";
                int index = code.IndexOf(NewLine, last);
                if (index == -1) break;

                length = index - last;
                if (length > buffer.Length)
                {
                    buffer = new char[length]; // Implementing proper NextPoT would have been better.
                }

                code.CopyTo(last, buffer, 0, length);
                writer.WriteLine(buffer, 0, length); // We split lines for writer for handling indentation.
                last = index + NewLine.Length;
            }

            length = code.Length - last;
            code.CopyTo(last, buffer, 0, length);
            writer.WriteLine(buffer, 0, length);
        }
    }
}
