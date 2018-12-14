export default interface GuildStatApiData
{
    guildName: string;
    guildRoleCounts: GuildRoleCount[];
    topUserMessageCounts: {[displayName: string] : number};
}

export interface GuildRoleCount
{
    name: string;
    count: number;
    color: string;
}