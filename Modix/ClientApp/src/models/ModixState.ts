import { ModuleHelpData } from "@/models/ModuleHelpData";
import PromotionCampaign from "@/models/promotions/PromotionCampaign";
import User from "@/models/User";
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import Role from '@/models/Role';
import Claim from '@/models/Claim';
import Guild from '@/models/Guild';
import DesignatedRoleMapping from '@/models/moderation/DesignatedRoleMapping';
import Channel from '@/models/Channel';

export default interface ModixState
{
    user: User | null;
    errors: string[];
    commands: ModuleHelpData[];
    campaigns: PromotionCampaign[];

    channelDesignations: DesignatedChannelMapping[];
    channelDesignationTypes: string[];

    roleMappings: DesignatedRoleMapping[];
    roleDesignationTypes: string[];

    claims: {[claim: string]: Claim[]};

    roles: Role[];
    guilds: Guild[];
    channels: { [key: string]: Channel; };
}