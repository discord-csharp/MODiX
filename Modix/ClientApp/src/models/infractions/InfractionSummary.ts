import GuildUserIdentity from '@/models/core/GuildUserIdentity';
import ModerationAction from '@/models/core/ModerationAction';
import { InfractionType } from './InfractionType';

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

    canRescind: boolean;
    canDelete: boolean;
}
