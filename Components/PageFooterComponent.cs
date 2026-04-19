using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CherAmiAPI.Components
{
    public class PageFooterComponentProps
    {
        public string Date { get; set; }
    }

    public class PageFooterComponent(PageFooterComponentProps props) : IDynamicComponent
    {
        readonly string charcoal800 = "#242832";

        public DynamicComponentComposeResult Compose(DynamicContext context)
        {
            var content = context.CreateElement(element =>
            {
                element
                    .Element(x => context.PageNumber % 2 != 0 ? x.AlignRight() : x.AlignLeft())
                    .Text(text =>
                    {
                        if (context.PageNumber % 2 != 0)
                        {
                            text.Span(props.Date)
                                .FontSize(12)
                                .FontFamily("Poppins")
                                .FontColor(charcoal800)
                                .Medium();

                            text.Span("  ");

                            text.CurrentPageNumber()
                                .FontSize(12)
                                .FontFamily("Poppins")
                                .FontColor(charcoal800)
                                .SemiBold();
                        }
                        else
                        {
                            text.CurrentPageNumber()
                                .FontSize(12)
                                .FontFamily("Poppins")
                                .FontColor(charcoal800)
                                .SemiBold();

                            text.Span("  ");

                            text.Span("Cher Ami")
                                .FontSize(14)
                                .FontFamily("Damion")
                                .FontColor(charcoal800)
                                .Medium();
                        }
                    });
            });

            return new DynamicComponentComposeResult
            {
                Content = content,
                HasMoreContent = false
            };
        }
    }
}
