using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace CherAmiAPI.Components.Pages
{
    public class PageH(List<PostComponentProps> postComponentProps) : Page(postComponentProps)
    {
        public override void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(0.5f, Unit.Inch);


                column.Item()
                      .Component(new PostComponent(postComponentProps[0]));
            });
        }
    }
}
