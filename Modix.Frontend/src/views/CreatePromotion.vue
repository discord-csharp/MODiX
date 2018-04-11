<template>
    <div>
        <HeroHeader text="Start a Campaign" />
    
        <section class="section">
            <div class="container columns">

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
                        <div class="control" :class="{'is-loading': loading}" v-if="!selectedAutocomplete.userId">

                            <Autocomplete :entries="autocompletes" @select="selectedAutocomplete = $event">
                                <input class="input" type="text" :class="{'is-danger': error}" placeholder="We have a fancy autocomplete!"
                                    v-model="searchQuery" @input="debouncedAutocomplete()">
                            </Autocomplete>

                        </div>
                        <div class="control" v-else>
                            <TinyUserView :user="selectedAutocomplete" /><button class="delete" aria-label="delete" @click="resetAutocomplete()"></button>
                        </div>
                        <p class="help is-danger">{{error}}</p>
                    </div>

                    

                    <div class="field">
                        <label class="label is-large">Then say a few words on their behalf</label>
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
import store from "../app/Store";
import * as _ from 'lodash';
import PromotionCreationData from '@/models/PromotionCreationData';
import User from '@/models/User';
import GeneralService from '@/services/GeneralService';

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
    searchQuery: string = "";
    creationData: PromotionCreationData = {userId: "", comment: ""};
    error: string | null = null;

    loading: boolean = false;
    autocompletes: User[] = [];
    selectedAutocomplete: User = new User();

    debouncedAutocomplete: Function = () => null;

    @Watch('selectedAutocomplete')
    selectedChanged()
    {
        this.searchQuery = this.selectedAutocomplete.name;
        this.creationData.userId = this.selectedAutocomplete.userId;
        this.autocompletes = [];
    }

    resetAutocomplete()
    {
        this.selectedAutocomplete = new User();
    }

    async createCampaign()
    {
        this.error = null;

        if (!this.creationData.userId)
        {
            this.creationData.userId = this.searchQuery;
        }

        try
        {
            await GeneralService.createCampaign(this.creationData);
            this.$router.push("/promotions");
        }
        catch (err)
        {
            this.error = err.response.data;
        }
    }

    created()
    {
        let self = this;

        this.debouncedAutocomplete = 
            _.debounce(async () => 
            {
                if (!self.searchQuery)
                {
                    self.autocompletes = [];
                    return;
                }

                self.loading = true;
                self.autocompletes = await GeneralService.getAutocomplete(self.searchQuery);
                self.loading = false;
            }, 500);
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
