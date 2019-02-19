<template>
    <div class="profile">
        <template v-if="user && user.userId">
            <img class="avatar-icon" v-if="user.avatarHash" :src="user.avatarUrl">

            <v-popover>
                <p class="title is-4">
                    <span class="username">
                        {{user.name}}
                        <div class="expander">
                            &#9660;
                        </div>
                    </span>
                </p>

                <template slot="popover">
                    <div class="options">
                        <v-popover trigger="hover click" placement="right" :delay="{hide: 300}">
                            <a class="option">
                                Theme &#9654;
                            </a>
                            <template slot="popover">
                                <div class="options">
                                    <div class="option theme" v-for="theme in themes" v-bind:key="theme.value" v-on:click="switchTheme(theme)">
                                        {{theme.name}}
                                    </div>
                                </div>
                            </template>
                        </v-popover>

                        <a class="option divider"></a>

                        <a class="option" href="/api/logout">Log Out</a>
                    </div>
                </template>
            </v-popover>

            <v-popover>
                <div class="guildDropdown tooltip-target" v-if="currentGuild" v-tooltip="'Selected: ' + currentGuild.name">

                    <img class="dropdown-icon" :src="currentGuild.iconUrl">

                    <div class="expander">
                        &#9660;
                    </div>

                </div>

                <template slot="popover">
                    <div class="options">
                        <div class="option" v-for="guild in guilds" :key="guild.id" @click="selectGuild(guild)">
                            <img class="icon" :src="guild.iconUrl">{{guild.name}}
                        </div>
                    </div>
                </template>
            </v-popover>

        </template>

        <template v-else>
            <p class="title is-4">
                <a href="/api/login">Log In &#128229;</a>
            </p>
        </template>
    </div>
</template>

<script lang="ts">
import Vue from 'vue';
import User from '@/models/User';
import modix from "@/app/Store";
import { Component } from 'vue-property-decorator';
import Guild from '@/models/Guild';
import store from '@/app/Store';
import GeneralService from '@/services/GeneralService';
import { Theme, themeContext, config, setConfig } from '@/models/PersistentConfig';

@Component({})
export default class MiniProfile extends Vue
{
    get user(): User
    {
        return this.$store.state.modix.user;
    }

    get guilds(): Guild[]
    {
        return this.$store.state.modix.guilds as Guild[];
    }

    get themes(): Theme[]
    {
        return themeContext.keys().map((theme: Theme) =>
        {
            return {
                name: this.toCaps(theme.slice(2, theme.length - 5)),
                value: theme
            }
        });
    }

    get currentGuild(): Guild
    {
        return store.currentGuild() ||
        {
            id: "0",
            name: "Unknown",
            iconUrl: ""
        };
    }

    toCaps(input: string): string
    {
        return input.charAt(0).toUpperCase() + input.substr(1);
    }

    switchTheme(theme: any)
    {
        setConfig(conf => conf.theme = theme.name);
        location.reload();
    }

    async selectGuild(guild: Guild)
    {
        await GeneralService.switchGuild(guild.id);
        location.reload();
    }
}
</script>
