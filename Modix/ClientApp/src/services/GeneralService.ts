import { ModuleHelpData } from '@/models/ModuleHelpData';
import User from '@/models/User';
import UserCodePaste from '@/models/UserCodePaste';
import _ from 'lodash';
import InfractionSummary from '@/models/infractions/InfractionSummary';
import GuildInfoResult from '@/models/GuildInfoResult';

import client from './ApiClient';
import Channel from '@/models/Channel';
import Role from '@/models/Role';
import Deserializer from '@/app/Deserializer';
import Claim from '@/models/Claim';
import Guild from '@/models/Guild';

export default class GeneralService
{
    static async getUser(): Promise<User>
    {
        let response = (await client.get("userInfo")).data;
        let user = Deserializer.getNew(User, response);
        
        return user;
    }

    static async getGuildInfo(): Promise<Map<string, GuildInfoResult>>
    {
        let response = (await client.get("guilds")).data;
        return (<Map<string, GuildInfoResult>>response);
    }

    static async getGuildRoles(): Promise<Role[]>
    {
        let response = (await client.get("roles")).data;
        return _.map(response, response => Deserializer.getNew(Role, response));
    }

    static async getClaims(): Promise<{[claim: string]: Claim[]}>
    {
        let response = (await client.get("claims")).data;
        return response;
    }

    static async getChannels(): Promise<Channel[]>
    {
        let response = (await client.get("channels")).data;
        return response;
    }

    static async getGuilds(): Promise<Guild[]>
    {
        let response = (await client.get("guildOptions")).data;
        return response;
    }

    static async switchGuild(guildId: string): Promise<void>
    {
        await client.post(`switchGuild/${guildId}`);
    }

    static async getCommands(): Promise<ModuleHelpData[]>
    {
        let response = (await client.get("commands")).data;
        return response;
    }

    static async getUserAutocomplete(query: string): Promise<User[]>
    {
        let response = (await client.get(`autocomplete/users?query=${encodeURIComponent(query)}`)).data;
        return _.map(response, user => Deserializer.getNew(User, user));
    }

    static async getChannelAutocomplete(query: string): Promise<Channel[]>
    {
        let response = (await client.get(`autocomplete/channels?query=${encodeURIComponent(query)}`)).data;
        return response;
    }

    static async getRankRolesAutocomplete(query: string): Promise<Role[]>
    {
        let response = (await client.get(`autocomplete/roles?query=${encodeURIComponent(query)}&rankOnly=true`)).data;
        return response;
    }

    static async getAllRolesAutocomplete(query: string): Promise<Role[]>
    {
        let response = (await client.get(`autocomplete/roles?query=${encodeURIComponent(query)}&rankOnly=false`)).data;
        return response;
    }
    
    static async getInfractions(): Promise<InfractionSummary[]>
    {
        let response = (await client.get("infractions")).data;
        return response;
    }

    static async getInfractionsForUser(id: number): Promise<InfractionSummary[]>
    {
        let response = (await client.get(`infractions/${id}`)).data;
        return response;
    }

    static async uploadRowboatJson(data: FormData) : Promise<number>
    {
        console.log(data);

        let response = await client.put("infractions/import", data, { timeout: 30000 });
        return response.data;
    }
}

//For debugging
(<any>window).service = GeneralService;