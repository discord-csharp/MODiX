import * as dateformat from "dateformat";

export const formatDate = (date: Date): string =>
{
    return dateformat(date, "mm/dd/yy, h:MM:ss TT");
}

export const toTitleCase = (str: string): string =>
{
    return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
}

export const getCookie = (name: string) =>
{
    var re = new RegExp(name + "=([^;]+)");
    var value = re.exec(document.cookie);
    return (value != null) ? unescape(value[1]) : null;
}

export const toBool = (input: any): boolean =>
{
    if (!input) { return false; }
    return input === true || input === "true" || input === "TRUE";
}