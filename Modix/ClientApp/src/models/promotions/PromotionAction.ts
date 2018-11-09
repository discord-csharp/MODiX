import GuildUserIdentity from '@/models/core/GuildUserIdentity';

export interface PromotionAction
{
    id:        number;
    created:   Date;
    createdBy: GuildUserIdentity;
}