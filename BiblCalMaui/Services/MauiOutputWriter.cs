using BiblCalCore;
using System.Text;

namespace BiblCalMaui.Services
{
    /// <summary>
    /// MAUI implementation of IOutputWriter that stores output in memory
    /// </summary>
    public class MauiOutputWriter : IOutputWriter
    {
        private readonly StringBuilder _output = new StringBuilder();

        public void Write(string text)
        {
            _output.Append(text);
        }

        public void WriteLine(string text)
        {
            _output.AppendLine(text);
        }

        public void Clear()
        {
            _output.Clear();
        }

        public string GetOutput()
        {
            return _output.ToString();
        }
    }
}

