using MudBlazor;

namespace Modix.Web.Shared.Models.DeletedMessages;

public record DeletedMessagesQuery(TableFilter Filter, TableState TableState);
