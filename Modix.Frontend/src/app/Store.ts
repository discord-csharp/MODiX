import * as Vuex from "vuex";
import { getStoreAccessors } from "vuex-typescript";
import ModixState from "@/models/ModixState";
import RootState from "@/models/RootState";
import GeneralService from "@/services/GeneralService";
import User from "@/models/User";
import {ModuleHelpData} from "@/models/ModuleHelpData";
import GuildInfoResult from "@/models/GuildInfoResult";
import UserCodePaste from "@/models/UserCodePaste";

type ModixContext = Vuex.ActionContext<ModixState, RootState>;

export const modix =
{
    namespaced: true,

    state:
    {
        user: null,
        guildInfo: new Map<string, GuildInfoResult>(),
        errors: [],
        pastes: [],
        currentPaste: null,
        commands: []
    },

    getters:
    {
        getGuildInfo(state: ModixState)
        {
            return state.guildInfo;
        }
    },

    mutations:
    {
        setUser(state: ModixState, user: User)
        {
            state.user = user;
        },
        setGuildInfo(state: ModixState, guildInfo: Map<string, GuildInfoResult>)
        {
            state.guildInfo = guildInfo;
        },
        setPastes(state: ModixState, pastes: UserCodePaste[])
        {
            state.pastes = pastes;
        },
        setCurrentPaste(state: ModixState, paste: UserCodePaste | null)
        {
            state.currentPaste = paste;
        },
        setCommands(state: ModixState, commands: ModuleHelpData[])
        {
            state.commands = commands;
        },
        pushError(state: ModixState, error: string)
        {
            state.errors.push(error);
        },
        removeError(state: ModixState, error: string)
        {
            state.errors.splice(state.errors.indexOf(error), 1); 
        }
    },

    actions: 
    {
        async updateUserInfo(context: ModixContext): Promise<void>
        {
            try
            {
                context.commit('setUser', await GeneralService.getUser());
            }
            catch (err)
            {
                let message = "Couldn't retrieve user information: " + err;

                console.error(message);
                context.commit('setUser', null);

                //context.commit('pushError', message);
            }
        },

        async updateGuildInfo(context: ModixContext): Promise<void>
        {
            try
            {
                context.commit('setGuildInfo', await GeneralService.getGuildInfo());
            }
            catch (err)
            {
                let message = "Couldn't retrieve guild information: " + err;

                console.error(message);
                context.commit('setGuildInfo', new Map<string, GuildInfoResult>());

                //context.commit('pushError', message);
            }
        },

        async updatePastes(context: ModixContext): Promise<void>
        {
            try
            {
                context.commit('setPastes', await GeneralService.getPastes());
            }
            catch (err)
            {
                let message = "Couldn't retrieve paste information: " + err;

                console.error(message);
                context.commit('setPastes', new Array<UserCodePaste>());

                //context.commit('pushError', message);
            }
        },

        async getPaste(context: ModixContext, pasteId: number): Promise<void>
        {
            try
            {
                context.commit('setCurrentPaste', await GeneralService.getPaste(pasteId));
            }
            catch (err)
            {
                let message = "Couldn't retrieve paste information: " + err;

                console.error(message);
                context.commit('setCurrentPaste', null);

                //context.commit('pushError', message);
            }
        },

        async updateCommands(context: ModixContext): Promise<void>
        {
            try
            {
                context.commit('setCommands', await GeneralService.getCommands());
            }
            catch (err)
            {
                let message = "Couldn't retrieve command information: " + err;

                console.error(message);
                context.commit('setCommands', []);

                //context.commit('pushError', message);
            }
        }
    }
};

const { commit, read, dispatch } = getStoreAccessors<ModixState, RootState>("modix");

export const updateUserInfo = dispatch(modix.actions.updateUserInfo);
export const updateGuildInfo = dispatch(modix.actions.updateGuildInfo);
export const updatePastes = dispatch(modix.actions.updatePastes);
export const getPaste = dispatch(modix.actions.getPaste);

export const getGuildInfo = read(modix.getters.getGuildInfo);
export const removeError = commit(modix.mutations.removeError);
export const setCurrentPaste = commit(modix.mutations.setCurrentPaste);
export const updateCommands = dispatch(modix.actions.updateCommands);