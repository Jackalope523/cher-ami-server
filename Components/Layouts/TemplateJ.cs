using System;
using System.Collections.Generic;

namespace CherAmiAPI.Components.Layouts
{
    public class TemplateJ : Template
    {
        public TemplateJ()
        {
            Slots =
            [
                new() { Width = 1088, Height = 756 },
                new() { Width = 2250, Height = 756 },
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

            int cost = 0;
            for (int i = posts.Count; i < Slots.Count; i++)
                cost += (i == 1) ? 2 : 1;
            return cost;
        }
    }
}
