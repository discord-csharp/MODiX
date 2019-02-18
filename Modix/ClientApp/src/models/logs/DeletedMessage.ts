export default class DeletedMessage
{
    channel: string = "";
    author: string = "";
    created: Date = new Date();
    createdBy: string = "";
    content: string = "";
    reason: string = "";
    batchId: number | null = null;
}
