import { Dictionary } from "lodash";
import * as _ from "lodash";

import PromotionComment from "@/models/PromotionComment";
import PromotionUserInfo from '@/models/PromotionUserInfo';

export type PromotionSentiment = "For" | "Against" | "Neutral";  
export type CampaignStatus = "Active" | "Approved" | "Denied";

export const SentimentIcons = 
{
    "For": "👍",
    "Against": "👎",
    "Neutral": "😐"
}

export const StatusIcons = 
{
    "Active": "🗳️",
    "Approved": "✔️",
    "Denied": "❌"
}

export default class PromotionCampaign
{
    promotionCampaignId: number = 0;
    promotionFor: PromotionUserInfo = new PromotionUserInfo();

    startDate: Date = new Date(0);
    comments: PromotionComment[] = [];
    status: CampaignStatus = "Active";
    
    get sentimentRatio(): number
    {
        return this.votesFor / (this.votesFor + this.votesAgainst);
    }

    get votesFor(): number
    {
        return this.sentimentCounts['For'] || 0;
    }

    get votesAgainst(): number
    {
        return this.sentimentCounts['Against'] || 0;
    }

    get sentimentCounts(): Dictionary<number>
    {
        return _.countBy(this.comments, (comment: PromotionComment) => comment.sentiment);
    }

    deserializeFrom(input: any): PromotionCampaign
    {
        var self: any = this;
        
        for (let prop in input)
        {
            self[prop] = input[prop];
        }

        return this;
    }
}