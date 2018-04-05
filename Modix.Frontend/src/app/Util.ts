import * as dateformat from "dateformat";

export const formatPasteDate = (date: Date): string =>
{
    return dateformat(date, "mm/dd/yy, h:MM:ss TT");
}

export const toTitleCase = (str: string): string =>
{
    return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
}