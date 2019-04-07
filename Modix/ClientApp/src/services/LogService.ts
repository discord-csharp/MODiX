import _ from 'lodash';
import client from './ApiClient';
import RecordsPage from '@/models/RecordsPage';
import DeletedMessage from '@/models/logs/DeletedMessage'
import TableParameters from '@/models/TableParameters';
import DeletedMessageAbstraction from '@/models/logs/DeletedMessageAbstraction';
import Deserializer from '@/app/Deserializer';

export default class LogService
{
    static async getDeletedMessages(tableParams: TableParameters): Promise<RecordsPage<DeletedMessage>>
    {
        return (await client.put(`logs/deletedMessages`, tableParams)).data;
    }

    static async getDeletionContext(batchId: number): Promise<any>
    {
        let response = (await client.get(`logs/deletedMessages/context/${batchId}`)).data as DeletedMessageAbstraction[];

        for (let msg of response)
        {
            if (msg.sentTime)
            {
                msg.sentTime = new Date(msg.sentTime);
            }
        }

        return response;
    }
}