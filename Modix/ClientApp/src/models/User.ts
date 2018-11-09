export default class User
{
    name: string = "";
    userId: string = "";
    avatarHash: string = "";
    claims: string[] = [];
    selectedGuild: string = "";

    get avatarUrl(): string
    {
        return `https://cdn.discordapp.com/avatars/${this.userId}/${this.avatarHash}`;
    }
}