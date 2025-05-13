namespace SPP.Domain.Interfaces
{
    public interface IWorkerCommands
    {
        void startProcessing(string filepath);
        void sendStatistics();
        void sendInfo();
    }
}