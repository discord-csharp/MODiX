<template>
    <section class="section">
        <div class="message-narrow">
            <div class="field">
                <label class="label">User ID</label>
                <div class="control">
                    <input :class="inputClass" type="text" placeholder="Enter a User ID..." v-model="userId" />
                </div>
            </div>
        </div>
    </section>
</template>

<script lang="ts">
import { Component, Watch } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';
import UserService from '@/services/UserService';

@Component({})
export default class UserSearch extends ModixComponent
{
    userId: string | null = null;

    inputClass: any = 'input';

    @Watch('userId')
    async computeInputClass(): Promise<void>
    {
        if (this.userId == null || this.userId.length == 0)
        {
            this.inputClass = 'input';
            return;
        }

        let user = await UserService.getUserInformation(this.userId);

        if (!user || user.id <= 0)
        {
            this.inputClass = 'input is-danger';
        }
        else
        {
            this.inputClass = 'input is-success';
        }


        this.$emit('userSelected', user);
    }
}
</script>
