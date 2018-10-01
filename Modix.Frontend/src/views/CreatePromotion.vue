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
                        <label class="label is-large">Then, the rank to be promoted to</label>
                        <div class="control">

                            <Autocomplete @select="selectedRole = $event" :minimumChars="-1"
                                          :serviceCall="roleServiceCall" placeholder="This one's fancy too">
                                <template slot-scope="{entry}">
                                    @{{entry.name}}
                                </template>
                            </Autocomplete>

                        </div>
                    </div>

                    <div class="field">
                        <label class="label is-large">Finally, say a few words on their behalf</label>
                        <div class="control">
                            <textarea class="textarea" v-model="creationData.comment" placeholder="They should be promoted because..."></textarea>
                        </div>
                    </div>

                    <div class="control">
                        <button class="button is-link" @click="createCampaign()">Submit</button>
                    </div>
                </div>

            </div>
        </section>
    </div>
</template>

<style lang="scss">

@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/base/_all";
@import '~bulma/sass/elements/form';
@import "~bulma/sass/elements/box";

.delete
{
    vertical-align: super;
    margin-left: 0.25em;
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
    creationData: PromotionCreationData = {userId: "", comment: "", roleId: ""};
    error: string | null = null;

    selectedUser: User = new User();
    selectedRole: Role | null = null;

    @Watch('selectedUser')
    userChanged()
    {
        this.creationData.userId = this.selectedUser.userId;
    }

    @Watch('selectedRole')
    roleChanged()
    {
        this.creationData.roleId = this.selectedRole!.id;
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
