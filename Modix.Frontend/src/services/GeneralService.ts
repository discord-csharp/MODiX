import { ModuleHelpData } from '@/models/ModuleHelpData';
import PromotionCampaign from '@/models/PromotionCampaign';
import PromotionCommentData from '@/models/PromotionCommentData';
import PromotionCreationData from '@/models/PromotionCreationData';
import User from '@/models/User';
import UserCodePaste from '@/models/UserCodePaste';
import Axios from 'axios';
import * as _ from 'lodash';

const client = Axios.create
({
    baseURL: '/api/',
    timeout: 1000,
    withCredentials: true
});

export default class GeneralService
{
    static async getUser(): Promise<User>
    {
        let response = (await client.get("userInfo")).data;
        let user = new User().deserializeFrom(response);
        
        return user;
    }

    static async getGuildInfo(): Promise<Map<string, Map<string, number>>>
    {
        let response = (await client.get("guilds")).data;
        return (<Map<string, Map<string, number>>>response);
    }

    static async getPastes(): Promise<UserCodePaste[]>
    {
        let response = <UserCodePaste[]>(await client.get("pastes")).data;
        response.forEach(paste => paste.created = new Date(paste.created));
        
        return response;
    }

    static async getPaste(pasteId: number): Promise<UserCodePaste>
    {
        let response = <UserCodePaste>(await client.get("pastes/" + pasteId)).data;
        response.created = new Date(response.created);
        
        return response;
    }

    static async getCommands(): Promise<ModuleHelpData[]>
    {
        let response = (await client.get("commands")).data;
        return response;
    }

    static async getCampaigns(): Promise<PromotionCampaign[]>
    {
        let response = (await client.get("campaigns")).data as PromotionCampaign[];
        
        _.forEach(response, campaign => 
        {
            campaign.startDate = new Date(campaign.startDate);
            _.forEach(campaign.comments, comment => comment.postedDate = new Date(comment.postedDate));
        });

        return _.map(response, campaign => new PromotionCampaign().deserializeFrom(campaign));;
    }

    static async createCampaign(data: PromotionCreationData): Promise<void>
    {
        await client.put("campaigns", data);
    }

    static async commentOnCampaign(campaign: PromotionCampaign, data: PromotionCommentData): Promise<void>
    {
        await client.put(`campaigns/${campaign.id}/comments`, data);
    }

    static async approveCampaign(campaign: PromotionCampaign): Promise<void>
    {
        await client.post(`campaigns/${campaign.id}/approve`);
    }

    static async denyCampaign(campaign: PromotionCampaign): Promise<void>
    {
        await client.post(`campaigns/${campaign.id}/deny`);
    }

    static async activateCampaign(campaign: PromotionCampaign): Promise<void>
    {
        await client.post(`campaigns/${campaign.id}/activate`);
    }

    static async getAutocomplete(query: string): Promise<User[]>
    {
        let response = (await client.get(`autocomplete?query=${query}`)).data;
        return _.map(response, user => new User().deserializeFrom(user));
    }
}

//For debugging
(<any>window).service = GeneralService;