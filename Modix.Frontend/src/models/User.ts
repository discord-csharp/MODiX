export default class User
{
    constructor(public name: string, public id: string, public avatarHash: string) { }

    get avatarUrl(): string
    {
        return `https://cdn.discordapp.com/avatars/${this.id}/${this.avatarHash}`;
    }
}