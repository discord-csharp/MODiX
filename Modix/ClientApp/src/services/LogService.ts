import _ from 'lodash';
import client from './ApiClient';
import RecordsPage from '@/models/RecordsPage';
import DeletedMessage from '@/models/log/DeletedMessage'

export default class LogService
{
    static async getDeletedMessages(pageSize: number, pageNumber: number): Promise<RecordsPage<DeletedMessage>>
    {
        return (await client.get(`logs/${pageSize}/${pageNumber}/deletedMessages`)).data;
    }
}

//For debugging
(<any>window).service = LogService;
