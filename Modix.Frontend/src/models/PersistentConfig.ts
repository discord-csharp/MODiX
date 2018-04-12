import PersistentKeyValueService from "@/services/PersistentKeyValueService";

export interface PersistentConfig
{
    showInactiveCampaigns: boolean;
}

const defaultConfig: PersistentConfig =
{
    showInactiveCampaigns: false
};

export const config = (): PersistentConfig => PersistentKeyValueService.get("persistentConfig") || defaultConfig;
export const setConfig = (setter: (conf: PersistentConfig) => void) => 
{
    let instance = config();

    setter(instance);
    PersistentKeyValueService.set("persistentConfig", instance);
};