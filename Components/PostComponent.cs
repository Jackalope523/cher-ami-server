using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace CherAmiAPI.Components
{
    public class PostComponentProps
    {
        public MemoryStream Image { get; set; }
        public MemoryStream AuthorAvatar { get; set; }
        public string AuthorName { get; set; }
        public string Text { get; set; }
    }

    public class PostComponent(PostComponentProps props) : IComponent
    {
        readonly string charcoal800 = "#242832";

        private static string GetInitials(string name)
        {
            string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            char first = char.ToUpper(parts[0][0]);
            char last = char.ToUpper(parts[1][0]);

            return $"{first}{last}";
        }

        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(0.25f, Unit.Inch);

                column.Item()
                      .Image(props.Image);

                column.Item()
                      .Row(row =>
                      {
                          row.Spacing(0.25f, Unit.Inch);

                          if (props.AuthorAvatar == null)
                          {
                              row.ConstantItem(0.5f, Unit.Inch)
                                 .Height(0.5f, Unit.Inch)
                                 .Width(0.5f, Unit.Inch)
                                 .Background("#F4F1EA")
                                 .CornerRadius(0.25f, Unit.Inch)
                                 .AlignCenter()
                                 .AlignMiddle()
                                 .Text(GetInitials(props.AuthorName))
                                 .FontColor("#868581")
                                 .FontFamily("Poppins")
                                 .FontSize(12);
                          }
                          else
                          {
                              row.ConstantItem(0.5f, Unit.Inch)
                                 .CornerRadius(0.25f, Unit.Inch)
                                 .Image(props.AuthorAvatar);
                          }

                          row.RelativeItem()
                           .AlignMiddle()
                           .Text(props.AuthorName)
                           .FontFamily("Poppins")
                           .FontSize(12)
                           .FontColor(charcoal800)
                           .SemiBold();
                      });

                column.Item()
                      .Text(props.Text)
                      .FontFamily("Poppins")
                      .FontSize(12)
                      .FontColor(charcoal800);
            });
        }

    }
}
