import { PieChartItem } from '@/models/PieChart'

export default interface GuildStatApiData
{
    guildName: string;
    guildRoleCounts: GuildRoleCount[];
    topUserMessageCounts: PerUserMessageCount[];
}

export interface GuildRoleCount extends PieChartItem
{
}

export interface PerUserMessageCount
{
    username: string;
    discriminator: string;
    rank: number;
    messageCount: number;
    isCurrentUser: boolean;
}