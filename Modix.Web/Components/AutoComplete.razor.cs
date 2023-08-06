using Microsoft.AspNetCore.Components;
using Modix.Web.Models.Common;

namespace Modix.Web.Components
{
    public partial class AutoComplete<T> where T : IAutoCompleteItem
    {
        [Parameter]
        public RenderFragment<T> ItemTemplate { get; set; }

        [Parameter]
        public string? Placeholder { get; set; }

        [Parameter]
        public EventCallback<T> SelectedItemChanged { get; set; }

        [Parameter, EditorRequired]
        public string? Title { get; set; }

        [Parameter, EditorRequired]
        public Func<string, Task<IEnumerable<T>>> SearchFunc { get; set; } = null!;
    }
}
