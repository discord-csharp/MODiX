<template>
    <div>
        <HeroHeader text="Promotions" /> 
    
        <section class="section">
            <div class="container">

                <div class="level">

                        <div class="level-left">

                        <div class="level-item">
                            <h1 class="title">
                                Campaigns
                            </h1>
                        </div>

                        </div>

                        <div class="level-right columns is-mobile">

                            <div class="column">
                                <label class="checkbox">
                                    <input type="checkbox" v-model="showInactive">
                                    Show Inactive
                                </label>
                            </div>

                            <div class="column is-narrow">
                                <router-link class="button is-pulled-right" to="/promotions/create">Start One</router-link>
                            </div>
                        
                        </div>
                    
                </div>
                
                <p v-if="campaigns.length == 0">
                    There's no active campaigns at the moment. You could start one, though!
                </p>
                <div v-else>
                    <PromotionListItem v-for="campaign in campaigns" :campaign="campaign" :key="campaign.id" 
                        @commentSubmitted="refresh()" @showPanel="showPanel(campaign)" />
                </div>
            </div>
        </section>

        <div class="modal" :class="{'is-active': showModal}">
            <div class="modal-background" @click="toggleModal()"></div>
            <div class="modal-card">
                <template v-if="modalCampaign">
                    <header class="modal-card-head">
                        <p class="modal-card-title"><strong>{{modalCampaign.username}}</strong>'s Campaign</p>
                        <button class="delete" aria-label="close" @click="toggleModal()"></button>
                    </header>
                    <section class="modal-card-body">
                        <PromotionCommentView class="expanded" v-for="comment in modalCampaign.comments" :key="comment.id" :comment="comment" />
                    </section>
                    <footer class="modal-card-foot level">
                        <div class="level-left">
                            <button v-if="modalCampaign.status == 'Denied'" class="button is-success" @click="activate()">Re-Activate</button>
                            <button v-else class="button is-success" @click="promote()">Promote</button>
                        </div>
                        <div class="level-right">
                            <button class="button is-danger" v-if="modalCampaign.status != 'Denied'" @click="deny()">Deny</button>
                        </div>
                    </footer>
                </template>
            </div>
        </div>


    </div>
</template>

<style lang="scss">

@import "../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/box";
@import "~bulma/sass/elements/tag";
@import "~bulma/sass/elements/form";
@import "~bulma/sass/components/menu";
@import "~bulma/sass/components/panel";
@import "~bulma/sass/components/level";
@import "~bulma/sass/elements/progress";
@import "~bulma/sass/components/modal";

.modal
{
    z-index: -999;

    &.is-active
    {
        z-index: 999;
    }
}

.modal-card
{
    top: 100%;

    transition: top 0.66s cubic-bezier(0.23, 1, 0.32, 1);
    transition-delay: 200ms;
}

.is-active .modal-card
{
    top: 0;
}

.modal-card
{
    @include tablet()
    {
        width: 95%;
    }
}

</style>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import PromotionListItem from '@/components/Promotions/PromotionListItem.vue';
import PromotionCommentView from '@/components/Promotions/PromotionCommentView.vue';

import store from "../app/Store";
import * as _ from 'lodash';
import PromotionCampaign from '@/models/PromotionCampaign';
import GeneralService from '@/services/GeneralService';
import {config, setConfig} from '@/models/PersistentConfig';
import PersistentKeyValueService from '@/services/PersistentKeyValueService';

@Component({
    components:
    {
        HeroHeader,
        PromotionListItem,
        PromotionCommentView
    },
})
export default class Promotions extends Vue
{
    showInactive: boolean = false;
    showModal: boolean = false;
    modalCampaign: PromotionCampaign | null = null;

    get campaigns(): PromotionCampaign[]
    {
        let campaigns = this.$store.state.modix.campaigns as PromotionCampaign[];
        let ordered = _.orderBy(campaigns, campaign =>  [campaign.status == 'Active', campaign.startDate.getTime(), campaign.comments.length], ['desc', 'desc', 'desc']);
        return _.filter(ordered, campaign => (this.showInactive ? true : campaign.status == "Active"));
    }

    @Watch('showInactive')
    inactiveChanged()
    {
        setConfig(conf => conf.showInactiveCampaigns = this.showInactive);
    }

    async created()
    {
        this.showInactive = config().showInactiveCampaigns;
        await this.refresh();
    }

    async refresh()
    {
        await store.retrieveCampaigns();
    }

    showPanel(campaign: PromotionCampaign)
    {
        this.modalCampaign = campaign;
        this.toggleModal();
    }

    toggleModal()
    {
        this.showModal = !this.showModal;
    }

    async promote()
    {
        if (this.modalCampaign == null) { return; }

        try
        {
            await GeneralService.approveCampaign(this.modalCampaign);
        }
        catch (err)
        {
            store.pushErrorMessage(err.response.data);
        }

        this.toggleModal();
        await this.refresh();
    }

    async deny()
    {
        if (this.modalCampaign == null) { return; }

        await GeneralService.denyCampaign(this.modalCampaign);
        this.toggleModal();
        await this.refresh();
    }

    async activate()
    {
        if (this.modalCampaign == null) { return; }

        await GeneralService.activateCampaign(this.modalCampaign);
        this.toggleModal();
        await this.refresh();
    }

    updated()
    {
        
    }
}
</script>
