using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace CherAmiAPI.Components.Pages
{
    public class PageD(List<PostComponentProps> postComponentProps) : Page(postComponentProps)
    {
        public override void Compose(IContainer container)
        {
            container.Row(row =>
            {
                row.Spacing(0.25f, Unit.Inch);

                row.RelativeItem().Column(column =>
                {
                    column.Spacing(0.5f, Unit.Inch);

                    column.Item().Component(new PostComponent(postComponentProps[0]));

                    if (postComponentProps.Count >= 2)
                        column.Item().Component(new PostComponent(postComponentProps[1]));
                });

                if (postComponentProps.Count >= 3)
                    row.ConstantItem(3.625f, Unit.Inch)
                       .Component(new PostComponent(postComponentProps[2]));
            });
        }
    }
}
