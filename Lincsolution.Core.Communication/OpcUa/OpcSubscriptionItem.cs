using Opc.Ua;

namespace Lincsolution.Core.Communication.OpcUa
{
    public class OpcSubscriptionItem
    {
        public string DisplayName { get; }
        public NodeId StartNodeId { get; }

        public OpcSubscriptionItem(string displayName, NodeId startNodeId)
        {
            DisplayName = displayName;
            StartNodeId = startNodeId;
        }
    }
}
