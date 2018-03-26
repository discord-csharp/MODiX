import Axios, { AxiosResponse } from 'axios';
import User from '@/models/User';
import UserCodePaste from '@/models/UserCodePaste';

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
        let user = new User(response["name"], response["userId"], response["avatarHash"]);
        
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

    static async getCommands(): Promise<any>
    {
        let response = (await client.get("commands")).data;
        return response;
    }
}

(<any>window).service = GeneralService;