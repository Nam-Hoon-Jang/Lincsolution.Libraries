using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;

namespace Lincsolution.Core.Communication.OpcUa
{
    public interface IOpcClient
    {
        bool Connect(string url, string name, string certPath,
            object targetInstance, IEnumerable<OpcSubscriptionItem> subscriptionItemList,
            uint sessionTimeout, int subscriptionInterval, uint subscriptionMaxNotificationPerPublish,
            bool useIdentity, ushort keySize);
        void DisConnect();
        void CreateSubscription();
        bool Write(NodeId id, object data);
        NodeId GetNodeId(string name);
        OpcClientException LastException { get; }
        string UserName { get; set; }
        string Password { get; set; }
    }
}
