export default class PersistentKeyValueService
{
    private static _cache: Map<string, any> = new Map<string, any>();

    static get<T>(key: string): T | null
    {
        if (!this._cache.get(key))
        {
            let found = localStorage.getItem(key);

            if (found)
            {
                this._cache.set(key, JSON.parse(found));
            }
            else
            {
                return null;
            }
        }

        return this._cache.get(key);
    }

    static set(key: string, data: any)
    {
        let json = JSON.stringify(data);

        localStorage.setItem(key, json);
        this._cache.set(key, data);
    }
}