using CherAmiAPI.Entities;
using Serilog;
using System.Collections.Generic;
using System.Linq;


namespace CherAmiAPI.Components.Layouts
{
    public class MagazinePlanner
    {
        public static List<(Template, List<PostComponentProps>)> Plan(List<PostComponentProps> posts)
        {
            List<Template> templates = [new TemplateA(), new TemplateB(), new TemplateC(), new TemplateD(), new TemplateE(), new TemplateF(), new TemplateG(), new TemplateH(), new TemplateI(), new TemplateJ()];

            List<(int cost, int templateIndex, int prevI)> best = [(0, -1, -1), .. Enumerable.Repeat((int.MaxValue, -1, -1), posts.Count)];

            for (int i = 1; i <= posts.Count; i++)
            {
                for (int j = 0; j < templates.Count; j++)
                {
                    for (int k = 1; k <= templates[j].Slots.Count && k <= i; k++)
                    {
                        int previousPostsCost = best[i - k].cost;
                        if (previousPostsCost == int.MaxValue) continue;

                        List<PostComponentProps> subset = posts.GetRange(i - k, k);
                        int stepCost = templates[j].Cost(subset);
                        if (stepCost == int.MaxValue) continue;
                        int newTotalCost = previousPostsCost + stepCost;

                        if (newTotalCost < best[i].cost)
                        {
                            best[i] = (newTotalCost, j, i - k);
                        }
                    }
                }
            }

            Log.Error("Backtracking plan. Total cost: {TotalCost} for {PostCount} posts.", best[posts.Count].cost, posts.Count);

            int index = posts.Count;
            List<(Template, List<PostComponentProps>)> result = [];
            while (index > 0)
            {
                int start = best[index].prevI;
                Template chosen = templates[best[index].templateIndex];
                List<PostComponentProps> chosenPosts = posts.GetRange(start, index - start);
                int stepCost = chosen.Cost(chosenPosts);

                Log.Error("Picked {Template} for posts [{Start}..{End}) (k={K}), step cost {StepCost}, cumulative {Cumulative}.",
                    chosen.GetType().Name, start, index, chosenPosts.Count, stepCost, best[index].cost);

                result.Insert(0, (chosen, chosenPosts));
                index = start;
            }

            return result;
        }
    }
}