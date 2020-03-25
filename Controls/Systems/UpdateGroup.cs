using Unity.Entities;

namespace UGUIDots.Controls {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class InputGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class MessagingUpdateGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MessagingUpdateGroup))]
    public class MessagingConsumptionGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MessagingConsumptionGroup))]
    public class MessagingProductionGroup : ComponentSystemGroup { }
}
