import _ from 'lodash';
import GuildInfoResult from "@/models/GuildInfoResult";
import ModixState from "@/models/ModixState";
import { ModuleHelpData } from "@/models/ModuleHelpData";
import PromotionCampaign from "@/models/promotions/PromotionCampaign";
import RootState from "@/models/RootState";
import User from "@/models/User";
import GeneralService from "@/services/GeneralService";
import Vue from "vue";
import * as Vuex from "vuex";
import { BareActionContext, getStoreBuilder } from "vuex-typex";
import InfractionSummary from '@/models/infractions/InfractionSummary';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import ConfigurationService from '@/services/ConfigurationService';
import PromotionService from '@/services/PromotionService';
import ClaimMapping from '@/models/ClaimMapping';
import Role from '@/models/Role';
import Claim from '@/models/Claim';
import Guild from '@/models/Guild';
import DesignatedRoleMapping from '@/models/moderation/DesignatedRoleMapping';
import Channel from '@/models/Channel';

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
    campaigns: [],
    infractions: [],
    channelDesignations: [],
    claims: {},
    roles: [],
    channels: [],
    guilds: [],
    roleMappings: []
};

const storeBuilder = getStoreBuilder<RootState>();
const moduleBuilder = storeBuilder.module<ModixState>("modix", modixState);

namespace modix
{
    const setUser = (state: ModixState, user: User) => state.user = user;
    const setGuildInfo = (state: ModixState, guildInfo: Map<string, GuildInfoResult>) => state.guildInfo = guildInfo;
    const setCommands = (state: ModixState, commands: ModuleHelpData[]) => state.commands = commands;
    const setCampaigns = (state: ModixState, campaigns: PromotionCampaign[]) => state.campaigns = campaigns;
    const setInfractions = (state: ModixState, infractions: InfractionSummary[]) => state.infractions = infractions;
    const setRoles = (state: ModixState, roles: Role[]) => state.roles = roles;
    const setGuilds = (state: ModixState, guilds: Guild[]) => state.guilds = guilds;
    const setChannels = (state: ModixState, channels: Channel[]) => state.channels = channels;
    
    const setChannelDesignations = (state: ModixState, mappings: DesignatedChannelMapping[]) => state.channelDesignations = mappings;
    const setRoleDesignations = (state: ModixState, mappings: DesignatedRoleMapping[]) => state.roleMappings = mappings;
    const setClaims = (state: ModixState, claims: {[claim: string]: Claim[]}) => state.claims = claims;
    
    const getHasTriedAuth = (state: ModixState) => state.user != null;
    const getIsLoggedIn = (state: ModixState) => state.user && state.user.userId;
    const getCurrentClaims = (state: ModixState) => (state.user && state.user.claims) || [];
    const getCurrentGuild = (state: ModixState) => _.find(state.guilds, (guild: Guild) => guild.id == state.user!.selectedGuild);
 
    const pushError = (state: ModixState, error: string) => state.errors.push(error);
    const removeError = (state: ModixState, error: string) => state.errors.splice(state.errors.indexOf(error), 1);
    const clearErrors = (state: ModixState) => state.errors = [];
    const clearInfractions = (state: ModixState) => state.infractions = [];

    const updateUserInfo = async (context: ModixContext) => 
        mutatingServiceCall(GeneralService.getUser, setUser, err => setUser(modixState, new User()));
		
    const updateGuilds = async (context: ModixContext) => 
        mutatingServiceCall(GeneralService.getGuilds, setGuilds, err => setGuilds(modixState, []));

    const updateGuildInfo = async (context: ModixContext) => mutatingServiceCall(GeneralService.getGuildInfo, setGuildInfo);
    const updateCommands = async (context: ModixContext) => mutatingServiceCall(GeneralService.getCommands, setCommands);
    const updateCampaigns = async (context: ModixContext) => mutatingServiceCall(PromotionService.getCampaigns, setCampaigns);
    const updateInfractions = async (context: ModixContext) => mutatingServiceCall(GeneralService.getInfractions, setInfractions);
    const updateRoles = async (context: ModixContext) => mutatingServiceCall(GeneralService.getGuildRoles, setRoles);
    const updateChannels = async (context: ModixContext) => mutatingServiceCall(GeneralService.getChannels, setChannels);

    const updateChannelDesignations = async (context: ModixContext) => mutatingServiceCall(ConfigurationService.getChannelDesignations, setChannelDesignations);
    const updateRoleDesignations = async (context: ModixContext) => mutatingServiceCall(ConfigurationService.getRoleDesignations, setRoleDesignations);
    const updateClaims = async (context: ModixContext) => mutatingServiceCall(GeneralService.getClaims, setClaims);
    
    export const retrieveUserInfo = moduleBuilder.dispatch(updateUserInfo);
    export const retrieveGuildInfo = moduleBuilder.dispatch(updateGuildInfo);
    export const retrieveCommands = moduleBuilder.dispatch(updateCommands);
    export const retrieveCampaigns = moduleBuilder.dispatch(updateCampaigns);
    export const retrieveInfractions = moduleBuilder.dispatch(updateInfractions);
    export const retrieveChannelDesignations = moduleBuilder.dispatch(updateChannelDesignations);
    export const retrieveRoleDesignations = moduleBuilder.dispatch(updateRoleDesignations);
    export const retrieveClaims = moduleBuilder.dispatch(updateClaims);
    export const retrieveRoles = moduleBuilder.dispatch(updateRoles);
    export const retrieveChannels = moduleBuilder.dispatch(updateChannels);
    export const retrieveGuilds = moduleBuilder.dispatch(updateGuilds);

    export const pushErrorMessage = moduleBuilder.commit(pushError);
    export const removeErrorMessage = moduleBuilder.commit(removeError);
    export const clearErrorMessages = moduleBuilder.commit(clearErrors);
    export const clearInfractionData = moduleBuilder.commit(clearInfractions);

    export const hasTriedAuth = moduleBuilder.read(getHasTriedAuth);
    export const isLoggedIn = moduleBuilder.read(getIsLoggedIn);
    export const currentClaims = moduleBuilder.read(getCurrentClaims);
    export const currentGuild = moduleBuilder.read(getCurrentGuild);

    export const userHasClaims = (claims: string[]) =>
    {
        let diff = _.difference(claims, (modixState.user && modixState.user.claims) || []);

        return diff.length === 0;
    };
}

export default modix;

const mutatingServiceCall = async function<T>
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
            mutator(modixState, null);

            let message = `<strong>${err}</strong> while attempting service call. Call an admin!`;
            modixState.errors.push(message);
            console.trace(err);
        }
    }
}

export const vuexStore = storeBuilder.vuexStore();