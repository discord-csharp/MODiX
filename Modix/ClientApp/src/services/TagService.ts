import _ from 'lodash';
import client from './ApiClient';
import TagSummary from '@/models/Tags/TagSummary';
import TagCreationData from '@/models/Tags/TagCreationData';
import TagMutationData from '@/models/Tags/TagMutationData';

export default class TagService
{
    static async getTags(): Promise<TagSummary[]>
    {
        return (await client.get("tags")).data;
    }

    static async createTag(name: string, data: TagCreationData): Promise<void>
    {
        await client.put(`tags/${name}`, data);
    }

    static async updateTag(name: string, data: TagMutationData): Promise<void>
    {
        await client.patch(`tags/${name}`, data);
    }

    static async deleteTag(name: string): Promise<void>
    {
        await client.delete(`tags/${name}`);
    }
}

//For debugging
(<any>window).service = TagService;
