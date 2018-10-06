<template>
    <div class="profile">
        <template v-if="user && user.userId">
            <img class="avatar-icon" :src="user.avatarUrl">
            <p class="title is-4">
                <span class="username">{{user.name}}</span>
                <!--<a href="/api/logout" title="Log Out">👋</a>-->
            </p>

            <v-popover>
                <div class="guildDropdown tooltip-target" v-if="currentGuild" v-tooltip="'Selected: ' + currentGuild.name">

                    <img class="dropdown-icon" :src="currentGuild.iconUrl">

                    <div class="expander">
                        <template v-if="expanded">▲</template>
                        <template v-else>▼</template>
                    </div>

                </div>

                <template slot="popover">
                    <div class="options">
                        <div class="option" v-for="guild in guilds" :key="guild.id" @click="selectGuild(guild)">
                            <img class="guildIcon" :src="guild.iconUrl">{{guild.name}}
                        </div>
                    </div>
                </template>
            </v-popover>
            
        </template>

        <template v-else>
            <p class="title is-4">
                <a href="/api/login">Log In 📥</a>
            </p>
        </template>
    </div>
</template>


<style scoped lang="scss">
@import "../styles/variables";
@import "~bulma/sass/base/_all";
@import "~bulma/sass/components/media";
@import "~bulma/sass/elements/box";

.title:not(:last-child)
{
    margin-bottom: 0;
}

.guildDropdown
{
    cursor: pointer;

    border-left: 1px solid transparentize($white, 0.75);

    margin: 2px 0.25em 0 0.33em;
    padding-left: 0.66em;

    display: inline-block;

    & > .avatar-icon
    {
        position: relative;
        top: 2px;
    }
}

.guildIcon
{
    vertical-align: middle;
    margin-right: 0.5em;

    max-height: 32px;
}

.option
{
    color: $black;
    padding: 0.5em 0.5em 0.5em 0.5em;

    &:hover
    {
        background: $info;
        color: $text;

        cursor: pointer;
    }
}

.expander
{
    display: inline-block;
    font-size: 0.8em;

    position: relative;
    top: -7px;
}

.profile
{
    justify-self: flex-end;
    color: white;

    padding: 8px 6px 8px 12px;
}

.title.is-4, .title.is-4 a
{
    word-break: normal;
    color: white;
}

.v-popover
{
    
}

.avatar-icon, .dropdown-icon
{
    margin-right: 0.5em;

    border-radius: 4px;
    box-shadow: -1px 1px 0px white;

    
}

.username
{
    margin-right: 0.2em;

    @include tiny()
    {
        display: none;
    }
}
</style>

<script lang="ts">
import Vue from 'vue';
import User from '@/models/User';
import modix from "@/app/Store";
import { Component } from 'vue-property-decorator';
import Guild from '@/models/Guild';
import store from '@/app/Store';
import GeneralService from '@/services/GeneralService';

@Component({})
export default class MiniProfile extends Vue
{
    expanded: boolean = false;

    get user(): User
    {
        return this.$store.state.modix.user;
    }

    get guilds(): Guild[]
    {
        return this.$store.state.modix.guilds as Guild[];
    }

    get currentGuild()
    {
        return store.currentGuild();
    }

    async created()
    {
        await store.retrieveGuilds();
    }

    async selectGuild(guild: Guild)
    {
        await GeneralService.switchGuild(guild.id);
        location.reload();
    }
}
</script>
