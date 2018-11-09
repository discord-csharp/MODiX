import { RoleDesignation } from '@/models/moderation/RoleDesignation';

export default interface DesignatedRoleMapping
{
    id: string;
    roleId: string;
    roleDesignation: RoleDesignation;
    name: string;
}