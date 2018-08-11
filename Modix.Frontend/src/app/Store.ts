import GuildInfoResult from "@/models/GuildInfoResult";
import ModixState from "@/models/ModixState";
import { ModuleHelpData } from "@/models/ModuleHelpData";
import PromotionCampaign from "@/models/PromotionCampaign";
import RootState from "@/models/RootState";
import User from "@/models/User";
import UserCodePaste from "@/models/UserCodePaste";
import GeneralService from "@/services/GeneralService";
import Vue from "vue";
import * as Vuex from "vuex";
import { BareActionContext, getStoreBuilder } from "vuex-typex";

Vue.use(Vuex);

type ModixContext = BareActionContext<ModixState, RootState>;

const modixState: ModixState =
{
    user: null,
    guildInfo: new Map<string, GuildInfoResult>(),
    errors: [],
    pastes: [],
    currentPaste: null,
    commands: [],
    campaigns: []
};

const storeBuilder = getStoreBuilder<RootState>();
const moduleBuilder = storeBuilder.module<ModixState>("modix", modixState);

namespace modix
{
    const setUser = (state: ModixState, user: User) => state.user = user;
    const setGuildInfo = (state: ModixState, guildInfo: Map<string, GuildInfoResult>) => state.guildInfo = guildInfo;
    const setPastes = (state: ModixState, pastes: UserCodePaste[]) => state.pastes = pastes;
    const setCurrentPaste = (state: ModixState, paste: UserCodePaste | null) => state.currentPaste = paste;
    const setCommands = (state: ModixState, commands: ModuleHelpData[]) => state.commands = commands;
    const setCampaigns = (state: ModixState, campaigns: PromotionCampaign[]) => state.campaigns = campaigns;

    const getHasTriedAuth = (state: ModixState) => state.user != null;
    const getIsLoggedIn = (state: ModixState) => state.user && state.user.userId;

    const pushError = (state: ModixState, error: string) => state.errors.push(error);
    const removeError = (state: ModixState, error: string) => state.errors.splice(state.errors.indexOf(error), 1);
    const clearErrors = (state: ModixState) => state.errors = [];

    const updateUserInfo = async (context: ModixContext) => 
        tryServiceCall(GeneralService.getUser, setUser, err => setUser(modixState, new User()));

    const updateGuildInfo = async (context: ModixContext) => tryServiceCall(GeneralService.getGuildInfo, setGuildInfo);
    const updatePastes = async (context: ModixContext) => tryServiceCall(GeneralService.getPastes, setPastes);
    const updateCommands = async (context: ModixContext) => tryServiceCall(GeneralService.getCommands, setCommands);
    const updateCampaigns = async (context: ModixContext) => tryServiceCall(GeneralService.getCampaigns, setCampaigns);

    export const retrieveUserInfo = moduleBuilder.dispatch(updateUserInfo);
    export const retrieveGuildInfo = moduleBuilder.dispatch(updateGuildInfo);
    export const retrievePastes = moduleBuilder.dispatch(updatePastes);
    export const retrieveCommands = moduleBuilder.dispatch(updateCommands);
    export const retrieveCampaigns = moduleBuilder.dispatch(updateCampaigns);

    export const pushErrorMessage = moduleBuilder.commit(pushError);
    export const removeErrorMessage = moduleBuilder.commit(removeError);
    export const clearErrorMessages = moduleBuilder.commit(clearErrors);

    export const hasTriedAuth = moduleBuilder.read(getHasTriedAuth);
    export const isLoggedIn = moduleBuilder.read(getIsLoggedIn);
}

export default modix;

const tryServiceCall = async function<T>
(
    serviceAction: () => Promise<T>, 
    mutator: (state: ModixState, param: any) => void, 
    actionError: ((err: Error) => void) | null = null
)
{
    try
    {
        let result = await serviceAction();
        mutator(modixState, result);
    }
    catch (err)
    {
        if (actionError != null)
        {
            actionError(err);
        }
        else
        {
            let message = "Couldn't retrieve: " + err;
            mutator(modixState, null);

            //console.error(message);
        }
    }
}

export const vuexStore = storeBuilder.vuexStore();