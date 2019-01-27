import GuildUserIdentity from '@/models/core/GuildUserIdentity';

export default interface TagAction
{
    created: string;
    createdBy: GuildUserIdentity;
}
