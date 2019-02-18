import _ from 'lodash';
import client from './ApiClient';
import RecordsPage from '@/models/RecordsPage';
import DeletedMessage from '@/models/logs/DeletedMessage'
import TableParameters from '@/models/TableParameters';

export default class LogService
{
    static async getDeletedMessages(tableParams: TableParameters): Promise<RecordsPage<DeletedMessage>>
    {
        return (await client.put(`logs/deletedMessages`, tableParams)).data;
    }
}

//For debugging
(<any>window).service = LogService;
