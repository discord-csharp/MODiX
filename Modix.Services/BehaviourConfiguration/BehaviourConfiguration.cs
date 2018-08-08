namespace Modix.Services.BehaviourConfiguration
{
    public interface IBehaviourConfiguration
    {
        InvitePurgeBehaviour InvitePurgeBehaviour { get; set; }
        MessageLogBehaviour MessageLogBehaviour { get; set; }
    }

    public class BehaviourConfiguration : IBehaviourConfiguration
    {
        public InvitePurgeBehaviour InvitePurgeBehaviour { get; set; }
        public MessageLogBehaviour MessageLogBehaviour { get; set; }
    }
}