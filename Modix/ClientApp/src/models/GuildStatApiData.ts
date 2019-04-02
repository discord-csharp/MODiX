export default interface GuildStatApiData
{
    guildName: string;
    guildRoleCounts: GuildRoleCount[];
    topUserMessageCounts: PerUserMessageCount[];
}

export interface GuildRoleCount
{
    name: string;
    count: number;
    color: string;
}

export interface PerUserMessageCount
{
    username: string;
    discriminator: string;
    rank: number;
    messageCount: number;
    isCurrentUser: boolean;
}