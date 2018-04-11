import { PromotionSentiment } from "@/models/PromotionCampaign";

export default interface PromotionCommentData
{
    body: string;
    sentiment: PromotionSentiment;
}