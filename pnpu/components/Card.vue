<template>
  <nuxt-link
    :to="{
      name: 'client',
      params: {
        client: clientName,
        step: currentStep,
        workflowDate: workflowDate,
        textStatus: textStatus
      }
    }"
    append
    tag="span"
    class="route"
  >
    <v-hover v-slot="{ hover }">
      <v-card
        :elevation="hover ? 6 : 1"
        class="d-flex justify-lg-space-between transition-swing cursor"
        transition="slide-y-transition"
        max-height="174"
        min-width="300"
      >
        <div>
          <v-card-title class="headline subtitle-2 text-uppercase">
            {{ clientName }}
          </v-card-title>
          <v-card-subtitle
            >Step {{ currentStep }} / {{ maxStep }}</v-card-subtitle
          >
          <v-card-subtitle>{{ clientTypolgie }}</v-card-subtitle>
          <v-chip
            v-if="currentStep == 10"
            color="green lighten-1"
            class="ml-3 mb-3"
            text-color="white"
          >
            <v-icon left color="white">mdi-check-circle</v-icon>
            Terminé
          </v-chip>
          <v-chip
            v-else-if="currentStep < 10"
            :color="colorIconStatus"
            class="ml-3 mb-3"
            text-color="white"
          >
            <v-icon left color="white">{{ iconStatus }}</v-icon>
            {{ textStatus }}
          </v-chip>
        </div>
        <v-progress-circular
          :rotate="-90"
          :size="120"
          :width="10"
          :value="percentCircular"
          :color="colorCircular"
          class="ma-5"
        >
          {{ percentCircular }}
        </v-progress-circular>
      </v-card>
    </v-hover>
  </nuxt-link>
</template>

<script>
export default {
  name: 'CardPnpu',
  props: {
    clientName: { type: String, default: 'clientName' },
    currentStep: { type: Number, default: 0 },
    maxStep: { type: Number, default: 8 },
    clientTypolgie: { type: String, default: 'clientTypolgie' },
    colorIconStatus: { type: String, default: 'grey lighten-1' },
    iconStatus: { type: String, default: 'mdi-progress-clock' },
    textStatus: { type: String },
    percentCircular: { type: Number, default: 0 },
    colorCircular: { type: String, default: 'primary' },
    workflowDate: { type: String, default: '' }
  },
  beforeMount() {
    this.getStatusClient()
    this.setColorByTypologie()
  },
  methods: {
    getStatusClient() {
      switch (this.textStatus) {
        case 'Terminé':
          this.colorIconStatus = 'success'
          this.iconStatus = 'mdi-check-circle'
          this.percentCircular = 100
          break
        case 'Manuel':
          this.colorIconStatus = 'warning'
          this.iconStatus = 'mdi-hand'
          break
        case 'En cours':
          this.colorIconStatus = 'grey lighten-1'
          this.iconStatus = 'mdi-progress-clock'
          break
        case 'En erreur':
          this.colorIconStatus = 'error'
          this.iconStatus = 'mdi-close-circle'
          break
        default:
          console.log('Sorry, we are out of ' + this.textStatus + '.')
      }
    },
    setColorByTypologie() {
      switch (this.clientTypolgie) {
        case 'SaaS Dédié':
          this.colorCircular = 'teal lighten-2'
          break
        case 'SaaS Désynchronisé':
          this.colorCircular = 'lime lighten-2'
          break
        case 'Plateforme':
          this.colorCircular = 'red lighten-2'
          break
      }
    }
  }
}
</script>

<style scoped>
.cursor {
  cursor: pointer;
}
</style>
