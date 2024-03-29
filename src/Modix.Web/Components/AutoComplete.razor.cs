using Microsoft.AspNetCore.Components;
using Modix.Web.Models.Common;

namespace Modix.Web.Components;

public partial class AutoComplete<T> where T : IAutoCompleteItem
{
    [Parameter]
    // Nullability mismatch between MudBlazor, this value is checked for null in the MudBlazor component and changes behavior based on that
    // This is to get around the annoying warning when we assign a 'RenderFragment<T>?' to 'RenderFragment<T>'
    // In theory the nullable variant is "more" correct, but alas, here we are
    public RenderFragment<T> ItemTemplate { get; set; } = null!;

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public EventCallback<T> SelectedItemChanged { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter, EditorRequired]
    public Func<string, Task<IEnumerable<T>>> SearchFunc { get; set; } = null!;
}
