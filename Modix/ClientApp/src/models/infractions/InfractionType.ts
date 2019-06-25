export enum InfractionType
{
    Notice = "Notice",
    Warning = "Warning",
    Mute = "Mute",
    Ban = "Ban"
}

export function infractionTypeToEmoji(type: InfractionType)
{
    switch (type)
    {
        case "Notice":
            return "&#128221;";
        case "Warning":
            return "&#9888;";
        case "Mute":
            return "&#128263;";
        case "Ban":
            return "&#128296;";
        default:
            return type;
    }
}