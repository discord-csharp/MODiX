import { InfractionType } from '@/models/infractions/InfractionType';
import User from '../User';

export default class InfractionCreationData
{
    subject: User = new User();
    type: InfractionType = InfractionType.Notice;
    reason: string = "";
    duration: number = 0;
}
