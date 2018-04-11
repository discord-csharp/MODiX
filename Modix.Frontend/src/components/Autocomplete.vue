<template>

    <div>
        <slot></slot>
        
        <div class="autocomplete" v-show="entries.length > 0">
            <div class="entry" v-for="entry in entries" :key="entry.id" :class="{'hovered': hovered == entry}"
                @click="select(entry)" @mouseover="hovered = entry" @mouseout="mouseOut(entry)">
                <TinyUserView :user="entry" />
            </div>
        </div>
    </div>

</template>

<style lang="scss">

@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/base/_all";
@import '~bulma/sass/elements/form';
@import "~bulma/sass/elements/box";

.autocomplete
{
    position: absolute;
    z-index: 99;

    background: $white;
    width: 100%;
    
    padding: 0em 0em 0 0em;

    box-shadow: $box-shadow;
    border-radius: $box-radius;
    border-top-left-radius: 0;
    border-top-right-radius: 0;

    .entry
    {
        padding: 0.5em 0.5em 0 0.5em;

        &.hovered
        {
            background: $info;
            color: $white;

            cursor: pointer;
        }
    }
}

</style>


<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import TinyUserView from '@/components/TinyUserView.vue';
import User from '@/models/User';

@Component({
    components: {TinyUserView}
})
export default class Autocomplete extends Vue
{
    // @ts-ignore
    @Prop() private entries: any[];
    //@Prop() private displayProp: string | null = null;

    hovered: any = null;
    selectedIndex: number = -1;

    /*
    getDisplay(object: any): string
    {
        if (this.displayProp == null)
        {
            return object.toString();
        }

        return object[this.displayProp];
    }
    */

    @Watch('entries')
    entriesChanged()
    {
        console.log("Entries changed, resetting selection");
        this.selectedIndex = -1;
        this.hovered = null;
    }

    @Watch('selectedIndex')
    indexChanged()
    {
        console.log("Index changed, reselecting");
        this.hovered = this.entries[this.selectedIndex];
    }

    get inputBox(): HTMLInputElement
    {
        return <HTMLInputElement>this.$slots.default[0].elm;
    }

    mouseOut(entry: any)
    {
        if (this.hovered == entry)
        {
            this.hovered = null;
        }
    }

    select(entry: any)
    {
        this.$emit('select', entry);
        this.selectedIndex = -1;
    }

    mounted()
    {
        this.inputBox.addEventListener('keydown', this.keyDown);
        this.selectedIndex = -1;
    }

    keyDown(args: KeyboardEvent)
    {
        if (args.key == "ArrowUp")
        {
            console.log("Arrow Up!");

            if (this.selectedIndex <= 0)
            {
                this.selectedIndex = this.entries.length - 1;
            }
            else
            {
                this.selectedIndex--;
            }

            console.log("New Index: " + this.selectedIndex);
        }

        if (args.key == "ArrowDown")
        {
            console.log("Arrow Down!");
            
            if (this.selectedIndex >= this.entries.length - 1)
            {
                this.selectedIndex = 0;
            }
            else
            {
                this.selectedIndex++;
            }

            console.log("New Index: " + this.selectedIndex);
        }

        if (args.key == "Enter")
        {
            this.select(this.hovered);
        }
    }

    beforeDestroy()
    {
        this.inputBox.removeEventListener('keydown', this.keyDown);
    }
}
</script>
