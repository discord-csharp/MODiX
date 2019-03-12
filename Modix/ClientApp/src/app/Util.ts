import * as dateformat from "dateformat";
import * as _ from 'lodash';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';

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

const channelResolvingRegex = /<#(\d+)>/gm;
const urlResolvingRegex = /<(http.*)>/gm;
const mentionResolvingRegex = /<@(\d+)>/gm;

export const resolveMentions = (channelCache: { [key: string]: DesignatedChannelMapping; } | null, content: string) =>
{
    let replaced = content;

    if (channelCache)
    {
        replaced = content.replace(channelResolvingRegex, (sub, args: string) =>
        {
            if (!channelCache[args])
            {
                return args;
            }

            return `<span class='channel'>#${channelCache[args].name}</span>`;
        });
    }

    replaced = replaced.replace(urlResolvingRegex, (sub, args: string) =>
    {
        return `<a target="_blank" href="${_.escape(args)}">${_.escape(args)}</a>`;
    });

    replaced = replaced.replace(mentionResolvingRegex, (sub, args: string) =>
    {
        return `<span class='userMention'>@${_.escape(args)}</span>`;
    });

    return replaced;
}