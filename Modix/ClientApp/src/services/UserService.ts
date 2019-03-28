import _ from 'lodash';
import client from './ApiClient';
import EphemeralUser from '@/models/EphemeralUser';

export default class UserService
{
    static async getUserInformation(userId: string): Promise<EphemeralUser | null>
    {
        return (await client.get(`userInformation/${userId}`)).data;
    }
}
