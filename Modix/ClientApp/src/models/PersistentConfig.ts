import PersistentKeyValueService from "@/services/PersistentKeyValueService";

export enum Theme
{
    Default = "Default",
    Spoopy = "Spoopy",
    Holiday = "Holiday"
}

export interface PersistentConfig
{
    showInactiveCampaigns: boolean;
    showInfractionState: boolean;
    showDeletedInfractions: boolean;
    theme: Theme;
}

const defaultConfig: PersistentConfig =
{
    showInactiveCampaigns: false,
    showInfractionState: false,
    showDeletedInfractions: false,
    theme: Theme.Default
};

export const config = (): PersistentConfig => PersistentKeyValueService.get("persistentConfig") || defaultConfig;
export const setConfig = (setter: (conf: PersistentConfig) => void) =>
{
    let instance = config();

    setter(instance);
    PersistentKeyValueService.set("persistentConfig", instance);
};
export const ensureConfig = () =>
{
    let config = PersistentKeyValueService.get<PersistentConfig>("persistentConfig");

    if (config == null)
    {
        PersistentKeyValueService.set("persistentConfig", defaultConfig);
        return;
    }

    for (let key of Object.getOwnPropertyNames(defaultConfig))
    {
        if (config.hasOwnProperty(key) == false)
        {
            PersistentKeyValueService.set("persistentConfig", defaultConfig);
            return;
        }
    }

}

export const themeContext = (<any>require).context('@/styles/themes/', true, /^.*\.scss$/);