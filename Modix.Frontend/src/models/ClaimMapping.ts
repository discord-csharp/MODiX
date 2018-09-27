export enum ClaimMappingType
{
    Granted = "Granted",
    Denied = "Denied"
}

export const MappingTypeFromBoolean = (input: boolean | null): ClaimMappingType | null =>
{
    switch (input)
    {
        case true:
            return ClaimMappingType.Granted;
        case false:
            return ClaimMappingType.Denied;
        default:
            return null;
    }
}

export default class ClaimMapping
{
    id:      number = 0;
    type:    ClaimMappingType = ClaimMappingType.Denied;
    guildId: string = "";
    roleId:  string | null = null;
    userId:  string | null = null;
    claim:   string = "";

    get isRoleMapping(): boolean
    {
        return this.roleId != null;
    }

    get isUserMapping(): boolean
    {
        return this.userId != null;
    }
}