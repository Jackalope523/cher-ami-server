using System;
using System.Collections.Generic;

namespace CherAmiAPI.Components.Layouts
{
    public class TemplateA : Template
    {
        public TemplateA()
        {
            Slots =
            [
                new() { Width = 1088, Height = 756 },
                new() { Width = 1088, Height = 756 },
                new() { Width = 1088, Height = 756 },
                new() { Width = 1088, Height = 756 },
            ];
        }

        public override int Cost(List<PostComponentProps> posts)
        {
            if (posts.Count > Slots.Count)
                return InfiniteCost;

            for (int i = 0; i < posts.Count; i++)
            {
                if (posts[i].ImageHeight <= 0 || Slots[i].Height <= 0)
                    return InfiniteCost;

                double postRatio = (double)posts[i].ImageWidth / posts[i].ImageHeight;
                double slotRatio = (double)Slots[i].Width / Slots[i].Height;
                if (Math.Abs(postRatio - slotRatio) > 0.1)
                    return InfiniteCost;
            }

            return Slots.Count - posts.Count;
        }
    }
}
