import * as dateformat from "dateformat";
import * as _ from 'lodash';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import ModixState from '@/models/ModixState';

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

const { toHTML } = require('discord-markdown');

export const parseDiscordContent = (store: ModixState, content: string): string =>
{
    return toHTML(content, {discordCallback: {
        channel: (channel: any) =>
        {
            let foundChannel: any = store.channels[channel.id];

            if (foundChannel == undefined)
            {
                foundChannel = channel.id;
            }
            else
            {
                foundChannel = foundChannel.name;
            }

            return `<span class='channel'>#${_.escape(foundChannel)}</span>`;
        },
        user: (user: any) => `<span class='userMention'>@${user.id}</span>`
    }});
};