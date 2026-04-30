using CherAmiAPI.Entities;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace CherAmiAPI.Components.Pages
{
    public abstract class Page(List<PostComponentProps> postComponentProps) : IComponent
    {
        protected readonly List<PostComponentProps> postComponentProps = postComponentProps;

        public abstract void Compose(IContainer container);
    }
}
