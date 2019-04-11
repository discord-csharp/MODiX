export default interface GuildUserIdentity
{
    id: number;
    guildId: string;
    username: string;
    discriminator: string;
    nickname: string;
}

export const getFullUsername = (user: GuildUserIdentity) =>
{
    return `${user.username}#${user.discriminator}`;
}