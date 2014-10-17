
namespace PunchIn.Models
{
    public enum States
    {
        Analysis,
        InProgress,
        Review,
        Testing,
        Done
    }

    public enum WorkTypes
    {
        BacklogItem,
        Bug,
        Change,
        Datafix,
        ServiceCall,
        Support,
        Task
    }
}
