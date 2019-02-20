import PromotionCampaign from '@/models/promotions/PromotionCampaign';
import PromotionCommentData from '@/models/promotions/PromotionCommentData';
import PromotionCreationData from '@/models/promotions/PromotionCreationData';
import client from './ApiClient';
import _ from 'lodash';
import PromotionComment from '@/models/promotions/PromotionComment';
import Deserializer from '@/app/Deserializer';
import Role from '@/models/Role';

export default class PromotionService
{
    static async getCampaigns(): Promise<PromotionCampaign[]>
    {
        let response = (await client.get("campaigns")).data as PromotionCampaign[];

        _.forEach(response, campaign =>
        {
            if (campaign.createAction)
            {
                campaign.createAction.created = new Date(campaign.createAction.created);
            }
        });

        return _.map(response, campaign => Deserializer.getNew(PromotionCampaign, campaign));
    }

    static async getComments(campaignId: number): Promise<PromotionComment[]>
    {
        let response = (await client.get(`campaigns/${campaignId}`)).data as PromotionComment[];

        _.forEach(response, comment =>
        {
            if (comment.createAction)
            {
                comment.createAction.created = new Date(comment.createAction.created);
            }
        });

        return response.filter(comment => !comment.isModified);
    }

    static async getNextRankRoleForUser(subjectId: string): Promise<Role>
    {
        return (await client.get(`campaigns/${subjectId}/nextRank`)).data as Role;
    }

    static async createCampaign(data: PromotionCreationData): Promise<void>
    {
        await client.put("campaigns", data);
    }

    static async commentOnCampaign(campaign: PromotionCampaign, data: PromotionCommentData): Promise<void>
    {
        await client.put(`campaigns/${campaign.id}/comments`, data);
    }

    static async updateComment(comment: PromotionComment, data: PromotionCommentData): Promise<void>
    {
        await client.put(`campaigns/${comment.id}/updateComment`, data);
    }

    static async approveCampaign(campaign: PromotionCampaign): Promise<void>
    {
        await client.post(`campaigns/${campaign.id}/accept`);
    }

    static async forceApproveCampaign(campaign: PromotionCampaign): Promise<void>
    {
        await client.post(`campaigns/${campaign.id}/forceAccept`);
    }

    static async denyCampaign(campaign: PromotionCampaign): Promise<void>
    {
        await client.post(`campaigns/${campaign.id}/reject`);
    }
}