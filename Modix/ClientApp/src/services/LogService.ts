import _ from 'lodash';
import client from './ApiClient';
import RecordsPage from '@/models/RecordsPage';

export default class LogService
{
    static async getDeletedMessages(pageSize: number, pageNumber: number): Promise<RecordsPage>
    {
        return (await client.get(`${pageSize}/${pageNumber}/deletedMessages`)).data;
    }
}

//For debugging
(<any>window).service = LogService;