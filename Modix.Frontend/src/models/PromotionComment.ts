import { PromotionSentiment } from "@/models/PromotionCampaign";

export default interface PromotionComment
{
    id: number;
    postedDate: Date;
    sentiment: PromotionSentiment;
    body: string;
}