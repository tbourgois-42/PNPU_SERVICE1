<template>
  <v-layout>
    <v-card class="my-4" min-width="300px">
      <v-card-title class="d-flex justify-space-between subtitle-1"
        >Mes indicateurs<v-icon>mdi-filter</v-icon></v-card-title
      >
      <v-divider class="mx-4"></v-divider>
      <v-chip class="ml-4 mt-4" color="grey" text-color="white">
        <v-avatar left class="grey darken-4">{{ countInProgress }}</v-avatar>
        En cours
      </v-chip>
      <v-chip class="ml-4 mt-4" color="error" text-color="white">
        <v-avatar left class="red darken-4">{{ countInError }}</v-avatar> En
        erreur
      </v-chip>
      <v-chip class="ml-4 mb-4 mt-4" color="success" text-color="white">
        <v-avatar left class="green darken-4">{{ countDone }}</v-avatar>
        Terminé
      </v-chip>
      <v-chip class="ml-5 mb-4 mt-4" color="warning" text-color="white">
        <v-avatar left class="orange darken-4">{{ countManuel }}</v-avatar>
        Manuel
      </v-chip>
    </v-card>
  </v-layout>
</template>

<script>
export default {
  props: ['Clients'],
  data: () => ({
    maxStep: 8
  }),

  computed: {
    countDone() {
      return this.Clients.filter((Clients) => Clients.step === this.maxStep)
        .length
    },
    countInProgress() {
      return this.Clients.filter((Clients) => Clients.textStatus === 'En cours')
        .length
    },
    countInError() {
      return this.Clients.filter(
        (Clients) => Clients.textStatus === 'En erreur'
      ).length
    },
    countManuel() {
      return this.Clients.filter((Clients) => Clients.textStatus === 'Manuel')
        .length
    }
  }
}
</script>
