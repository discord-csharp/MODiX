import { PromotionSentiment } from "@/models/promotions/PromotionCampaign";

export default interface PromotionCommentData
{
    body: string;
    sentiment: PromotionSentiment;
}