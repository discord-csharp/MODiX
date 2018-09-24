<template>

    <div class="tags has-addons">
        <span class="tag is-info">#{{designation.name}}</span>

        <template v-if="canDelete && showConfirm">
            <span class="tag is-dark">Remove Designation?</span>
            <a class="tag is-danger button" :class="{'is-loading': loading}" @click="confirm()">Yes</a>
            <a class="tag" @click="showConfirm = false">Nvm</a>
        </template>
        <a class="tag is-danger" v-else @click="showConfirm = true">X</a>
    </div>

</template>

<style lang="scss">
@import "../../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/tag";
@import "~bulma/sass/elements/form";

.pointer
{
    cursor: pointer;
}
</style>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import ConfigurationService from '@/services/ConfigurationService';
import DesignatedRoleMapping from '@/models/moderation/DesignatedRoleMapping';

@Component
export default class IndividualDesignation extends Vue
{
    @Prop() private designation!: DesignatedChannelMapping | DesignatedRoleMapping;
    @Prop() private canDelete!: boolean;

    showConfirm: boolean = false;
    loading: boolean = false;

    async confirm()
    {
        this.loading = true;

        this.$emit("confirm", this.designation);
        
        this.loading = false;
    }
}
</script>
