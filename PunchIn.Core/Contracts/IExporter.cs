namespace PunchIn.Core.Contracts
{
    public interface IExporter
    {
        void AddColumn(string value);
        void AddLineBreak();
        string Export(string exportPath);
    }
}
