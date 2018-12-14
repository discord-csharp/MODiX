import {config} from '@/models/PersistentConfig'

const themeFiles = (<any>require).context('@/assets/', true, /^.*\.png$/);

export default (filename: string): string =>
{
    return themeFiles('./' + config().theme + '/' + filename);
}