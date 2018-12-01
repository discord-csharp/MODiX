import Role from '../Role';
import Rank from './Rank';

export default interface MentionData
{
    role: Role
    mentionability: string;
    minimumRank: Rank | null;
}
