using System.Collections.Generic;
namespace CherAmiAPI.Components.Layouts
{
    public abstract class Template
    {
        protected const int InfiniteCost = 1000000;

        public List<Slot> Slots { get; set; }
        public int SlotNumber => Slots.Count;

        public abstract int Cost(List<PostComponentProps> posts);
    }
}
