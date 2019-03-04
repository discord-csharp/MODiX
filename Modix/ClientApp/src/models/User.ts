export default class User
{
    name: string = "";
    userId: string = "";
    avatarHash: string = "";
    claims: string[] = [];
    selectedGuild: string = "";

    get avatarUrl(): string
    {
        return this.avatarHash;
    }
}