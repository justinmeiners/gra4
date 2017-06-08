using System.Collections.Generic;

namespace GRA.Controllers
{
    public class DynamicAvatarDetails
    {

        // key is the layer id
        // string is the path to the avatar for this layer
        public Dictionary<int, string> Paths { get; set; }

        // list is sorted according to layer order, value is layer id
        public List<int> LayerOrders { get; set; }
        public string AvatarString { get; set; }
    }
}
