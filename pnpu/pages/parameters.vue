<template>
  <v-layout>
    <v-container row wrap>
      <v-flex md12>
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title class="title">
              {{ title }}
            </v-list-item-title>
            <v-list-item-subtitle>
              {{ subTitle }}
            </v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-flex>
      <v-flex md12>
        <v-divider class="my-2 mx-4" inset></v-divider>
      </v-flex>
    </v-container>
  </v-layout>
</template>

<script>
export default {
  components: {},
  data: () => ({
    title: 'Paramètres',
    subTitle: 'Subtitle'
  })
}
</script>
