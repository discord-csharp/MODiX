export default interface DeletedMessageAbstraction
{
    messageId: number;
    username: string;
    sentTime: Date | null;
    content: string;
    url: string | null;
}