import Axios, { AxiosResponse } from 'axios';
import User from '@/models/User';
import UserCodePaste from '@/models/UserCodePaste';
import { ModuleHelpData } from '@/models/ModuleHelpData';
import PromotionCreationData from '@/models/PromotionCreationData';
import PromotionCampaign from '@/models/PromotionCampaign';
import * as _ from 'lodash';
import PromotionCommentData from '@/models/PromotionCommentData';
import store from '@/app/Store';

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

    static async createCampaign(data: PromotionCreationData): Promise<any>
    {
        let response = (await client.put("campaigns", data)).data;
        return response;
    }

    static async commentOnCampaign(campaign: PromotionCampaign, data: PromotionCommentData): Promise<any>
    {
        let response = (await client.put(`campaigns/${campaign.id}/comments`, data)).data;
        return response;
    }

    static async approveCampaign(campaign: PromotionCampaign): Promise<any>
    {
        let response = (await client.post(`campaigns/${campaign.id}/approve`)).data;
        return response;
    }

    static async denyCampaign(campaign: PromotionCampaign): Promise<any>
    {
        let response = (await client.post(`campaigns/${campaign.id}/deny`)).data;
        return response;
    }

    static async activateCampaign(campaign: PromotionCampaign): Promise<any>
    {
        let response = (await client.post(`campaigns/${campaign.id}/activate`)).data;
        return response;
    }

    static async getAutocomplete(query: string): Promise<User[]>
    {
        let response = (await client.get(`autocomplete?query=${query}`)).data;
        return _.map(response, user => new User().deserializeFrom(user));
    }
}

//For debugging
(<any>window).service = GeneralService;