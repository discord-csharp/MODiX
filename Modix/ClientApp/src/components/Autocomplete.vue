<template>

    <div class="autocomplete-container" :class="{'committed': committed}">

        <div v-if="committed">
            <slot v-if="committed" v-bind:entry="committed" />
            <button class="delete" aria-label="delete" @click="committed = null"></button>
        </div>

        <template v-else>
            <input class="input" type="text" :class="{'is-danger': error}" :placeholder="placeholder"
                v-model="searchQuery" @input="debouncedAutocomplete()" @blur="blur()" @keydown="keyDown($event)">

            <div class="autocomplete" v-show="entries.length > 0">

                <div class="entry" v-for="(entry, index) in entries" :key="entry.id" :class="{'hovered': hovered == entry}"
                    @click="select(entry)" @mouseover.stop="hoveredChanged(index)">

                    <slot v-bind:entry="entry" />
                </div>

            </div>
        </template>
    </div>

</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import TinyUserView from '@/components/TinyUserView.vue';
import * as _ from 'lodash';

@Component({
    components: {TinyUserView}
})
export default class Autocomplete extends Vue
{
    @Prop({default: (query: string) => Promise})
    private serviceCall!: Function;

    @Prop({default: "We have a fancy autocomplete!"})
    private placeholder!: string;

    @Prop({default: false})
    private error!: boolean;

    @Prop({default: 2})
    private minimumChars!: number;

    debouncedAutocomplete: Function = () => null;

    hovered: any = null;
    selectedIndex: number = -1;
    loading: boolean = false;
    searchQuery: string = "";
    entries: any[] = [];

    committed: any = null;

    @Watch('committed')
    committedChanged()
    {
        this.$emit('select', this.committed);
    }

    @Watch('entries', { immediate: true })
    entriesChanged(entries: any[], oldEntries: any[])
    {
        if (entries.length > 0)
        {
            this.selectedIndex = 0;
            this.indexChanged();
        }
        else
        {
            this.selectedIndex = -1;
        }
    }

    @Watch('selectedIndex')
    indexChanged()
    {
        if (this.selectedIndex < 0)
        {
            this.hovered = null;
        }
        else
        {
            this.hovered = this.entries[this.selectedIndex];
        }
    }

    hoveredChanged(index: number)
    {
        this.selectedIndex = index;
    }

    select(entry: any)
    {
        this.committed = entry;
        this.searchQuery = "";
        this.entries = [];
        this.selectedIndex = -1;
    }

    mounted()
    {
        this.selectedIndex = -1;
    }

    async keyDown(args: KeyboardEvent)
    {
        if (args.key == "ArrowUp")
        {
            args.preventDefault();

            if (this.selectedIndex <= 0)
            {
                this.selectedIndex = this.entries.length - 1;
            }
            else
            {
                this.selectedIndex--;
            }
        }

        if (args.key == "ArrowDown")
        {
            args.preventDefault();

            if (this.entries.length == 0)
            {
                await this.makeServiceCall();
            }

            if (this.selectedIndex >= this.entries.length - 1)
            {
                this.selectedIndex = 0;
            }
            else
            {
                this.selectedIndex++;
            }
        }

        if (args.key == "Enter")
        {
            args.preventDefault();

            this.select(this.hovered);
        }
    }

    created()
    {
        this.debouncedAutocomplete = _.debounce(this.makeServiceCall, 350);
    }

    blur()
    {
        this.entries = [];
        this.selectedIndex = -1;
    }

    async makeServiceCall()
    {
        if (this.searchQuery.length <= this.minimumChars)
        {
            return;
        }

        this.loading = true;
        this.entries = await this.serviceCall(this.searchQuery);
        this.$emit('entries', this.entries);
        this.loading = false;
    }
}
</script>
