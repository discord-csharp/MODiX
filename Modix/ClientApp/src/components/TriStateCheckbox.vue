<template>

    <div class="triStateCheckbox" :class="innerValue">

        <template v-if="loading">
            <div class="button is-loading">Changing...</div>
        </template>

        <template v-else>
            <div class="option false" title="False"
                @click="internalChange(false)" 
                :class="{'active': innerValue == false}">X</div>

            <div class="option null" 
                @click="internalChange(null)" title="Unset"
                :class="{'active': innerValue == null}">–</div>

            <div class="option true" 
                @click="internalChange(true)" title="True"
                :class="{'active': innerValue == true}">✓</div>
        </template>

    </div>

</template>

<style scoped lang="scss">

@import "../styles/variables";

$radius: 4px;
$trueColor: green;
$nullColor: grey;
$falseColor: red;

.button
{
    height: 22px;
    width: 96px;
}

.triStateCheckbox
{
    float: right;
    height: 24px;
    border: 1px solid black;

    overflow: hidden;

    border-radius: $radius;

    &.true
    {
        border-color: $trueColor;
    }

    &.null
    {
        border-color: $nullColor;
    }

    &.false
    {
        border-color: $falseColor;
    }
}

.option
{
    user-select: none;

    position: relative;
    top: -1px;

    height: 22px;
    width: 2em;

    display: inline-block;

    font-family: monospace;
    font-weight: bold;    
    text-align: center;
    color: gray;

    cursor: default;

    &:not(.active)
    {
        cursor: pointer;
    }

    &.active
    {
        color: white;

        &.true
        {
            background: $trueColor;
        }

        &.null
        {
            background: $nullColor;
        }

        &.false
        {
            background: $falseColor;
        }
    }
}

</style>


<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';

@Component
export default class TriStateCheckbox extends Vue
{
    @Prop({default: null}) private value!: boolean | null;
    @Prop({default: false}) private loading!: boolean;

    innerValue: boolean | null = null;

    @Watch('value')
    valueChange()
    {
        this.innerValue = this.value;
    }

    internalChange(value: boolean | null)
    {
        if (this.innerValue == value) { return; }

        this.innerValue = value;
        this.$emit("input", this.innerValue);
    }
}
</script>
