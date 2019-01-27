import TagAction from '@/models/Tags/TagAction';

export default interface TagSummary
{
    guildId: string;
    name: string;
    content: string;
    uses: number;

    createAction: TagAction;
    deleteAction: TagAction;
}
