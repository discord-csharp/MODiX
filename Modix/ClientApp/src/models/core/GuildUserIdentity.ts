export default interface GuildUserIdentity
{
    id: number;
    guildId: string;
    username: string;
    discriminator: string;
    nickname: string;
    displayName: string;
}