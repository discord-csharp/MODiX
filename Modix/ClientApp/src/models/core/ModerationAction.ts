import GuildUserIdentity from '@/models/core/GuildUserIdentity';

export default interface ModerationAction
{
    id: number;
    created: string;
    createdBy: GuildUserIdentity;
}