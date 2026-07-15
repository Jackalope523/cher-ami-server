using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace CherAmiAPI.Components.Pages
{
    public class PageB(List<PostComponentProps> postComponentProps) : Page(postComponentProps)
    {
        public override void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(0.5f, Unit.Inch);

                column.Item().Row(row =>
                {
                    row.Spacing(0.25f, Unit.Inch);

                    row.ConstantItem(3.625f, Unit.Inch)
                       .Component(new PostComponent(postComponentProps[0]));

                    if (postComponentProps.Count >= 2)
                    {
                        row.ConstantItem(3.625f, Unit.Inch)
                           .Component(new PostComponent(postComponentProps[1]));
                    }
                });
            });
        }
    }
}
