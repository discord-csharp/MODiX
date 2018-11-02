import { ClaimMappingType } from '@/models/ClaimMapping';

export default interface RoleClaimModifyData
{
    claim: string;
    mappingType: ClaimMappingType | null;
    roleId: string;
}