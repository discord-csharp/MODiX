<template>

    <div class="tags has-addons">
        <span class="tag is-link">
            <template v-if="isChannel">#</template><template v-else>@</template>{{designation.name}}
        </span>

        <template v-if="showConfirm">
            <span class="tag is-dark">Remove Designation?</span>
            <a class="tag is-danger button" :class="{'is-loading': loading}" @click="confirm()">Yes</a>
            <a class="tag" @click="showConfirm = false">Nvm</a>
        </template>
        <a class="tag is-danger" v-else-if="canDelete" @click="showConfirm = true">X</a>
    </div>

</template>

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

    get isChannel(): boolean
    {
        return (this.designation as any).channelId != undefined;
    }
}
</script>
