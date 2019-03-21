import Role from './Role';

export default class EphemeralUser
{
    id!: string;

    username!: string;

    nickname!: string;

    discriminator!: string;

    avatarUrl!: string;

    status!: string;

    createdAt!: Date;

    joinedAt!: Date;

    firstSeen!: Date;

    lastSeen!: Date;

    rank!: number;

    last7DaysMessages!: number;

    last30DaysMessages!: number;

    averageMessagesPerDay!: number;

    percentile!: number;

    roles!: Role[];

    isBanned!: boolean;

    banReason!: string;
}
