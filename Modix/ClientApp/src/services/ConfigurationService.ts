import _ from 'lodash';
import client from '@/services/ApiClient';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import DesignatedChannelCreationData from '@/models/moderation/DesignatedChannelCreationData';
import ClaimMapping from '@/models/ClaimMapping';
import Deserializer from '@/app/Deserializer';
import RoleClaimModifyData from '@/models/configuration/RoleClaimModifyData';
import DesignatedRoleMapping from '@/models/moderation/DesignatedRoleMapping';
import DesignatedRoleCreationData from '@/models/moderation/DesignatedRoleCreationData';
import MentionabilityType from '@/models/configuration/MentionabilityType';

export default class ConfigurationService
{
    static async getChannelDesignations(): Promise<DesignatedChannelMapping[]>
    {
        let response = (await client.get("config/channels")).data;
        return response;
    }

    static async assignChannel(data: DesignatedChannelCreationData): Promise<void>
    {
        await client.put("config/channels", data);
    }

    static async unassignChannel(id: string): Promise<void>
    {
        await client.delete(`config/channels/${id}`);
    }

    static async getRoleDesignations(): Promise<DesignatedRoleMapping[]>
    {
        let response = (await client.get("config/roles")).data;
        return response;
    }

    static async assignRole(data: DesignatedRoleCreationData): Promise<void>
    {
        await client.put("config/roles", data);
    }

    static async unassignRole(id: string): Promise<void>
    {
        await client.delete(`config/roles/${id}`);
    }

    static async getMappedClaims(): Promise<ClaimMapping[]>
    {
        let response = (await client.get("config/claims")).data;
        return _.map(response, claim => Deserializer.getNew(ClaimMapping, claim));
    }

    static async modifyRoleClaim(data: RoleClaimModifyData)
    {
        await client.patch("config/claims", data);
    }

    static async getMentionabilityTypes(): Promise<MentionabilityType[]>
    {
        return (await client.get("config/mentions")).data;
    }
}