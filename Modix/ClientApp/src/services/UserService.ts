import _ from 'lodash';
import client from './ApiClient';
import EphemeralUser from '@/models/EphemeralUser';
import UserMessagePerChannelCount from '@/models/UserMessagePerChannelCount';

export default class UserService
{
    static async getUserInformation(userId: string): Promise<EphemeralUser | null>
    {
        return (await client.get(`userInformation/${userId}`)).data;
    }
    
    static async getMessageCountPerChannel(userId: string, after: Date | null = null): Promise<UserMessagePerChannelCount[]>
    {
        const afterQuery = after ? after.toISOString() : "";
        let response = (await client.get(`userInformation/${userId}/messages?after=${afterQuery}`)).data;
        return response;
    }
}
