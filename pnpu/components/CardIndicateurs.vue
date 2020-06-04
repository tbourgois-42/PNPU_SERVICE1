<template>
  <v-layout>
    <v-card class="my-4" max-width="277">
      <v-card-title class="d-flex justify-space-between subtitle-1"
        >Mes indicateurs
        <v-btn icon @click.prevent="filterIndicators('NO FILTER')"
          ><v-icon>{{ iconFilter }}</v-icon></v-btn
        ></v-card-title
      >
      <v-divider class="mx-4"></v-divider>
      <v-chip
        class="ml-4 mt-4"
        color="grey"
        text-color="white"
        @click.prevent="filterIndicators('En cours')"
      >
        <v-avatar left class="grey darken-4">{{ countInProgress }}</v-avatar>
        En cours
      </v-chip>
      <v-chip
        class="ml-4 mt-4"
        color="error"
        text-color="white"
        @click.prevent="filterIndicators('En erreur')"
      >
        <v-avatar left class="red darken-4">{{ countInError }}</v-avatar> En
        erreur
      </v-chip>
      <v-chip
        class="ml-4 mb-4 mt-4"
        color="success"
        text-color="white"
        @click.prevent="filterIndicators('Terminé')"
      >
        <v-avatar left class="green darken-4">{{ countDone }}</v-avatar>
        Terminé
      </v-chip>
      <v-chip
        class="ml-5 mb-4 mt-4"
        color="warning"
        text-color="white"
        @click.prevent="filterIndicators('Manuel')"
      >
        <v-avatar left class="orange darken-4">{{ countManuel }}</v-avatar>
        Manuel
      </v-chip>
    </v-card>
  </v-layout>
</template>

<script>
export default {
  props: {
    clients: {
      type: Array,
      default: () => []
    }
  },

  data: () => ({
    maxStep: 8,
    localClients: [],
    filter: '',
    ClientsFiltered: [],
    iconFilter: 'mdi-filter'
  }),

  computed: {
    countDone() {
      return this.localClients.filter(
        (client) => client.ID_STATUT === 'Terminé'
      ).length
    },
    countInProgress() {
      return this.localClients.filter(
        (client) => client.ID_STATUT === 'En cours'
      ).length
    },
    countInError() {
      return this.localClients.filter(
        (client) => client.ID_STATUT === 'En erreur'
      ).length
    },
    countManuel() {
      return this.localClients.filter((client) => client.ID_STATUT === 'Manuel')
        .length
    }
  },

  watch: {
    /**
     * Alimente la liste des clients du composant via la propriété clients.
     */
    clients() {
      this.localClients = this.clients
    }
  },

  methods: {
    filterIndicators(filter) {
      this.iconFilter = 'mdi-filter-remove'
      if (filter === 'Manuel') {
        this.ClientsFiltered = this.localClients.filter(
          (items) => items.ID_STATUT === 'Manuel'
        )
      } else if (filter === 'Terminé') {
        this.ClientsFiltered = this.localClients.filter(
          (items) => items.ID_STATUT === 'Terminé'
        )
      } else if (filter === 'En erreur') {
        this.ClientsFiltered = this.localClients.filter(
          (items) => items.ID_STATUT === 'En erreur'
        )
      } else if (filter === 'En cours') {
        this.ClientsFiltered = this.localClients.filter(
          (items) => items.ID_STATUT === 'En cours'
        )
      } else if (filter === 'NO FILTER') {
        this.ClientsFiltered = this.localClients
        this.iconFilter = 'mdi-filter'
      }
      console.log('ClientsFiltered', this.ClientsFiltered)
      this.$emit('getIndicators', this.ClientsFiltered)
    }
  }
}
</script>
