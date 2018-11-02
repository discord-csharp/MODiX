import { RoleDesignation } from '@/models/moderation/RoleDesignation';

export default interface DesignatedRoleCreationData
{
    roleId: string;
    roleDesignations: RoleDesignation[];
}