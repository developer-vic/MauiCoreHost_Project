namespace BiblCalCore
{
    /// <summary>
    /// Interface for output operations (replaces TPrint functionality)
    /// </summary>
    public interface IOutputWriter
    {
        void Write(string text);
        void WriteLine(string text);
        void Clear();
    }
}

