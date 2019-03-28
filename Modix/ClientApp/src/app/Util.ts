import * as dateformat from "dateformat";
import * as _ from 'lodash';
import ModixState from '@/models/ModixState';

export const formatDate = (date: Date): string =>
{
    return dateformat(date, "mm/dd/yy, h:MM:ss TT");
}

export const toTitleCase = (str: string): string =>
{
    return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
}

export const ordinalize = (value: number): string =>
{
    let rounded = Math.round(value);
    let asString = rounded.toString();

    if ((Math.floor(rounded / 10) % 10) == 1)
    {
        return asString + "th";
    }

    switch (rounded % 10)
    {
        case 1: return asString + "st";
        case 2: return asString + "nd";
        case 3: return asString + "rd";
        default: return asString + "th";
    }
}

export const toQuantity = (quantity: number, singular: string, plural: string): string =>
{
    let quantityString = quantity.toString();

    return quantity == 1
        ? quantityString + " " + singular
        : quantityString + " " + plural;
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
        user: (user: any) => `<span class='userMention'>@${user.id}</span>`,
        role: (role: any) =>
        {
            let foundRole = store.roles.find(r => r.id == role.id);

            if (foundRole)
            {
                return `<span class='role' style='color: ${foundRole.color};'>@${_.escape(foundRole.name)}</span>`;
            }
            else
            {
                return `<span class='role'>@${_.escape(role.id)}</span>`;
            }
        },
    }});
};