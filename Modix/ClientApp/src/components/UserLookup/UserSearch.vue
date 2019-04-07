<template>
    <div class="message-narrow">
        <div class="control">
            <Autocomplete @select="selectedUser = $event" :serviceCall="userServiceCall"
                          placeholder="Username or ID">
                <template slot-scope="{entry}">
                    <TinyUserView :user="entry" />
                </template>
            </Autocomplete>
        </div>
    </div>
</template>

<style lang="scss">
.message-narrow
{
    margin-bottom: 1em;

    .control input
    {
        font-size: 1.5em;
    }
}
</style>

<script lang="ts">
import { Component, Watch } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';
import UserService from '@/services/UserService';
import GeneralService from '@/services/GeneralService';
import TinyUserView from '@/components/TinyUserView.vue';
import Autocomplete from '@/components/Autocomplete.vue';
import User from '@/models/User';

@Component(
{
    components:
    {
        TinyUserView,
        Autocomplete
    }
})
export default class UserSearch extends ModixComponent
{
    selectedUser: User = new User();

    @Watch('selectedUser')
    async userChanged()
    {
        if (this.selectedUser && this.selectedUser.userId)
        {
            let ephemeralUser = await UserService.getUserInformation(this.selectedUser.userId);
            this.$emit('userSelected', ephemeralUser);
        }
    }

    get userServiceCall()
    {
        return GeneralService.getUserAutocomplete;
    }
}
</script>
