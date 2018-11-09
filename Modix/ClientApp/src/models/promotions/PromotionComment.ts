import { PromotionSentiment } from "@/models/promotions/PromotionCampaign";
import { PromotionAction } from '@/models/promotions/PromotionAction';

export default interface PromotionComment
{
    id: number;
    sentiment: PromotionSentiment;
    content: string;
    createAction: PromotionAction;
}