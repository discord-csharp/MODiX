import Role from './Role';

export default class EphemeralUser
{
    id: number = 0;

    username: string = "";

    nickname: string = "";

    discriminator: string = "";

    avatarUrl: string = "";

    status: string = "";

    createdAt: Date = new Date();

    joinedAt: Date = new Date();

    firstSeen: Date = new Date();

    lastSeen: Date = new Date();

    rank: number = 0;

    last7DaysMessages: number = 0;

    last30DaysMessages: number = 0;

    averageMessagesPerDay: number = 0;

    percentile: number = 0;

    roles: Role[] = [];

    isBanned: boolean = false;

    banReason: string = "";
}
