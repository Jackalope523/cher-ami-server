using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace CherAmiAPI.Components
{
    public record PostComponentProps
    {
        public byte[] Image { get; init; }
        public int ImageWidth { get; init; }
        public int ImageHeight { get; init; }
        public byte[] AuthorAvatar { get; init; }
        public string AuthorName { get; init; }
        public string Text { get; init; }
    }

    public class PostComponent(PostComponentProps props) : IComponent
    {
        readonly string charcoal800 = "#242832";

        protected string GetInitials(string name)
        {
            string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return "";

            char first = char.ToUpper(parts[0][0]);

            if (parts.Length < 2)
                return $"{first}";

            char last = char.ToUpper(parts[^1][0]);

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
                           .FontFamily("Poppins", "Noto Emoji")
                           .FontSize(12)
                           .FontColor(charcoal800)
                           .SemiBold();
                      });

                column.Item()
                      .Text(props.Text)
                      .FontFamily("Poppins", "Noto Emoji")
                      .FontSize(12)
                      .FontColor(charcoal800);
            });
        }
    }
}
