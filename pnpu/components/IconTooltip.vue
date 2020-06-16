<template>
  <v-tooltip :top="direction" :color="color" :max-width="maxWidth">
    <template v-slot:activator="{ on, attrs }">
      <v-icon v-bind="attrs" :class="margingPadding" v-on="on">{{
        icon
      }}</v-icon>
    </template>
    <span>{{ text }}</span>
  </v-tooltip>
</template>

<script>
export default {
  props: {
    icon: {
      type: String,
      default: 'mdi-help-circle'
    },
    text: {
      type: String,
      default: 'tooltip'
    },
    direction: {
      type: String,
      default: 'top'
    },
    color: {
      type: String,
      default: ''
    },
    maxWidth: {
      type: Number,
      default: 400
    },
    margingPadding: {
      type: String,
      default: 'mb-6 ml-5'
    }
  }
}
</script>
