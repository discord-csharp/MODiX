import { Dictionary } from "lodash";
import _ from "lodash";

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
    id: number = 0;
    userId: number = 0;
    username: string = "Unknown";
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
        return _.countBy(this.comments, comment => comment.sentiment);
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

export interface PromotionComment
{
    id: number;
    postedDate: Date;
    sentiment: PromotionSentiment;
    body: string;
}