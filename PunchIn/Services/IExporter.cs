namespace PunchIn.Services
{
    public interface IExporter
    {
        void AddColumn(string value);
        void AddLineBreak();
        string Export(string exportPath);
    }
}
