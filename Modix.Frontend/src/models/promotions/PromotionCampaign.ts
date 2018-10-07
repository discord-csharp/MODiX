import _ from "lodash";

import PromotionComment from "@/models/promotions/PromotionComment";
import PromotionService from '@/services/PromotionService';
import GuildUserIdentity from '@/models/core/GuildUserIdentity';
import { PromotionAction } from '@/models/promotions/PromotionAction';

export enum PromotionSentiment
{
    Approve = "Approve",
    Oppose = "Oppose",
    Abstain = "Abstain"
}

export enum CampaignOutcome
{
    Accepted = "Accepted",
    Rejected = "Rejected",
    Failed = "Failed"
}

export const SentimentIcons: {[sentiment in PromotionSentiment]: string} = 
{
    "Approve": "&#128077;",
    "Oppose": "&#128078;",
    "Abstain": "&#128528;"
}

export const StatusIcons: {[sentiment in CampaignOutcome]: string} = 
{
    "Accepted": "&#10004;",
    "Rejected": "&#10060;",
    "Failed": "&#10067;"
}

export default class PromotionCampaign
{
    id:              number = 0;
    guildId:         string = '';
    subject?:        GuildUserIdentity;
    targetRole?:     TargetRole;
    createAction?:   PromotionAction;
    outcome:         CampaignOutcome = CampaignOutcome.Failed;
    closeAction?:    PromotionAction;
    
    commentCounts: {[sentiment in PromotionSentiment]: number} = 
    {
        "Abstain": 0,
        "Approve": 0,
        "Oppose": 0
    };

    get isActive(): boolean
    {
        return !this.outcome;
    }

    get sentimentRatio(): number
    {
        return this.votesFor / (this.votesFor + this.votesAgainst);
    }

    get votesFor(): number
    {
        return this.commentCounts[PromotionSentiment.Approve] || 0;
    }

    get votesAgainst(): number
    {
        return this.commentCounts[PromotionSentiment.Oppose] || 0;
    }

    get startDate(): Date
    {
        return (this.createAction == null ? new Date(0) : this.createAction.created);
    }
}

export interface PromotionSubject
{
    id:            string;
    username:      string;
    discriminator: string;
    nickname:      null;
    displayName:   string;
}

export interface TargetRole
{
    id:       string;
    name:     string;
    position: number;
}
