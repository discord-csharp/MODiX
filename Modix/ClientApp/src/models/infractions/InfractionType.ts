export enum InfractionType
{
    Notice = "Notice",
    Warning = "Warning",
    Mute = "Mute",
    Ban = "Ban"
}

export namespace InfractionType
{
    export function toEmoji(type: InfractionType)
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
}