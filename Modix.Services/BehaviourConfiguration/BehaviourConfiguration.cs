namespace Modix.Services.BehaviourConfiguration
{
    public interface IBehaviourConfiguration
    {
        InvitePurgeBehaviour InvitePurgeBehaviour { get; set; }
    }

    public class BehaviourConfiguration : IBehaviourConfiguration
    {
        public InvitePurgeBehaviour InvitePurgeBehaviour { get; set; }
    }
}