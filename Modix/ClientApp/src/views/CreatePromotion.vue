<template>
    <div>
        <section class="section container">

            <h1 class="title">
                Start a Campaign
            </h1>

            <div class="columns">

                <div class="column is-one-third">
                    <div class="">
                        <p class="box">Feel like someone deserves recognition? <strong>Start a promotion campaign for them</strong> - even if that person is yourself!</p>
                        <p class="box">Once a campaign is started, users can <strong>anonymously comment</strong>, voicing their opinions for or against the individual up for promotion</p>
                        <p class="box">
                            Staff will periodically review campaigns. If approved, the user will be <strong>immediately promoted!</strong> If not, they may be permanently denied,
                            or further looked into as the campaign runs its course.
                        </p>
                    </div>
                </div>

                <div class="column">
                    <div class="field">
                        <label class="label is-large">Tell us their username</label>
                        <div class="control">

                            <Autocomplete @select="selectedUser = $event"
                                          :serviceCall="userServiceCall" placeholder="We have a fancy autocomplete!">
                                <template slot-scope="{entry}">
                                    <TinyUserView :user="entry" />
                                </template>
                            </Autocomplete>

                        </div>
                        <p class="help is-danger">{{error}}</p>
                    </div>

                    <div class="field">
                        <label class="label is-large">They can be promoted to this rank</label>
                        <span class="role" :style="roleStyle()">{{nextRank}}</span>
                    </div>

                    <div class="field">
                        <label class="label is-large">Finally, say a few words on their behalf</label>
                        <div class="control">
                            <textarea class="textarea" v-model="creationData.comment" placeholder="They should be promoted because..." :disabled="isNone"></textarea>
                        </div>
                    </div>

                    <div class="control">
                        <button class="button is-link" @click="createCampaign()" :disabled="isNone">Submit</button>
                    </div>
                </div>

            </div>
        </section>
    </div>
</template>

<style scoped lang="scss">

@import "~bulma/sass/utilities/_all";

.delete
{
    vertical-align: super;
    margin-left: 0.25em;
}

.role
{
    font-size: 14px;
    font-weight: 400 !important;
    color: #607d8b;
    padding: 4px 8px;
    border: 2px solid #607d8b;
    border-radius: 3px;
    position: relative;
    top: -2px;

    @include mobile() {
        display: table;
        margin-top: -0.6em;
    }
}

</style>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import TinyUserView from '@/components/TinyUserView.vue';
import Autocomplete from '@/components/Autocomplete.vue';
import store from "@/app/Store";
import * as _ from 'lodash';
import PromotionCreationData from '@/models/promotions/PromotionCreationData';
import User from '@/models/User';
import Role from '@/models/Role';
import GeneralService from '@/services/GeneralService';
import PromotionService from '@/services/PromotionService';
import { isNull } from 'util';

@Component({
    components:
    {
        HeroHeader,
        TinyUserView,
        Autocomplete
    },
})
export default class CreatePromotion extends Vue
{
    creationData: PromotionCreationData = {userId: "", comment: ""};
    error: string | null = null;

    selectedUser: User = new User();
    selectedRole: Role | null = null;

    nextRank: string = "None";
    isNone: boolean = true;

    @Watch('selectedUser')
    async userChanged()
    {
        this.creationData.userId = this.selectedUser.userId;

        if (this.selectedUser && this.selectedUser.userId)
        {
            this.selectedRole = await PromotionService.getNextRankRoleForUser(this.selectedUser.userId);
        }
    }

    @Watch('selectedRole')
    roleChanged()
    {
        if (this.selectedRole!.name == null)
        {
            this.isNone = true;
            this.nextRank = "None";
        }
        else
        {
            this.isNone = false;
            this.nextRank = "\u27A5 " + this.selectedRole!.name;
        }
    }

    roleStyle()
    {
        if (!this.selectedRole)
        {
            return { color: "#607d8b", borderColor: "#607d8b" };
        }

        return { color: this.selectedRole!.fgColor, borderColor: this.selectedRole!.fgColor };
    }

    resetAutocomplete()
    {
        this.selectedUser = new User();
        this.selectedRole = null;
    }

    async createCampaign()
    {
        this.error = null;

        try
        {
            await PromotionService.createCampaign(this.creationData);
            this.$router.push("/promotions");
        }
        catch (err)
        {
            this.error = err.response.data;
        }
    }

    get userServiceCall()
    {
        return GeneralService.getUserAutocomplete;
    }

    get roleServiceCall()
    {
        return GeneralService.getRankRolesAutocomplete;
    }

    created()
    {
        let self = this;
    }

    mounted()
    {
        this.resetAutocomplete();
    }

    updated()
    {

    }
}
</script>
