import { toBool } from '@/app/Util';

export default class ApplicationConfiguration
{
    static get isSpoopy(): boolean
    {
        return toBool(process.env.VUE_APP_SPOOPY);
    }
}