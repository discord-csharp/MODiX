<template>

    <div class="tinyUser">
        <figure class="image is-24x24">
            <img v-if="user.avatarHash" :src="user.avatarUrl" />
            <img v-else src="http://via.placeholder.com/32x32" />
        </figure>
        <span class="name">{{user.name}}</span>
    </div>

</template>

<style scoped lang="scss">


.image, .image img
{
    display: inline-block;
    margin-right: 0.5em;
}

.name
{
    line-height: 0;
    vertical-align: super;
}

.tinyUser
{
    display: inline-block;
}

</style>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

import User from '@/models/User';

@Component
export default class TinyUserView extends Vue
{
    @Prop() private user!: User;
}
</script>
