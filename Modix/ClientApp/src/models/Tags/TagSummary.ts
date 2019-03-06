export default interface TagSummary
{
    content: string;
    created: Date;
    isOwnedByRole: boolean;
    name: string;
    ownerName: string;
    ownerColor: string;
    uses: number;
    canMaintain: boolean;
}
