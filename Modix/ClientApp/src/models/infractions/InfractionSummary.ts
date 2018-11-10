import GuildUserIdentity from '@/models/core/GuildUserIdentity';
import { InfractionType } from '@/models/infractions/InfractionType';
import ModerationAction from '@/models/core/ModerationAction';

export default interface InfractionSummary
{
    id: number;
    guildId: string;
    type: InfractionType;
    reason: string;
    duration: string;
    subject: GuildUserIdentity;

    createAction: ModerationAction;
    rescindAction: ModerationAction;
    deleteAction: ModerationAction;
}