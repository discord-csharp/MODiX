import GuildUserIdentity from '../core/GuildUserIdentity';
import Role from '../Role';
import { GuildRoleBrief } from '../promotions/PromotionCampaign';

export default interface TagSummary
{
    content: string;
    created: Date;
    isOwnedByRole: boolean;
    name: string;
    ownerUser: GuildUserIdentity;
    ownerRole: GuildRoleBrief;
    uses: number;
    canMaintain: boolean;
}
