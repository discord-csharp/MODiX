<template>
    <section class="section">
        <div class="message-narrow">
            <div class="field">
                <label class="label is-large">Tell us their username or ID</label>
                <div class="control">
                    <Autocomplete @select="selectedUser = $event"
                                  :serviceCall="userServiceCall" placeholder="Enter a username or an ID">
                        <template slot-scope="{entry}">
                            <TinyUserView :user="entry" />
                        </template>
                    </Autocomplete>
                </div>
                <p class="help is-danger"></p>
            </div>
        </div>
    </section>
</template>

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
