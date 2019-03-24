import { InfractionType } from './InfractionType';

export default class InfractionCreationData
{
    type: InfractionType = InfractionType.Notice;
    reason: string = "";
    durationMonths: number | null = null;
    durationDays: number | null = null;
    durationHours: number | null = null;
    durationMinutes: number | null = null;
    durationSeconds: number | null = null;
}
