<template>
  <nuxt-link
    :to="{
      name: 'client',
      params: {
        client: clientName,
        step: currentStep,
        workflowDate: workflowDate,
        textStatus: textStatus,
        workflowID: workflowID
      }
    }"
    append
    tag="span"
    class="route"
  >
    <v-hover v-slot="{ hover }">
      <v-card
        :elevation="hover ? 6 : 1"
        class="d-flex justify-space-between transition-swing cursor"
        transition="slide-y-transition"
        min-width="370"
        max-width="370"
      >
        <div>
          <v-card-title class="headline subtitle-2 text-uppercase">
            {{ clientName }}
          </v-card-title>
          <v-card-subtitle class="pb-0">ID {{ idorga }}</v-card-subtitle>
          <span class="ma-0 pl-4 subtitle-2 body-2"
            >Step {{ currentStep }} / {{ maxStep }}</span
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
          :value="percentCircular.toFixed(2)"
          :color="colorCircular"
          class="ma-5"
        >
          {{ percentCircular.toFixed(2) }}
        </v-progress-circular>
        <!-- <v-card-actions>
          <v-btn text>Button</v-btn>
          <v-btn text>Button</v-btn>
        </v-card-actions>-->
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
    textStatus: { type: String, default: '' },
    percentCircular: { type: Number, default: 0 },
    colorCircular: { type: String, default: 'primary' },
    workflowDate: { type: String, default: '' },
    idorga: { type: String, default: '' },
    workflowID: { type: String, default: '' }
  },
  mounted() {
    this.getStatusClient()
    this.setColorByTypologie()
  },
  methods: {
    getStatusClient() {
      switch (this.textStatus) {
        case 'CORRECT':
          this.colorIconStatus = 'success'
          this.iconStatus = 'mdi-check-circle'
          this.textStatus = 'Terminé'
          break
        case 'WARNING':
          this.colorIconStatus = 'warning'
          this.iconStatus = 'mdi-hand'
          this.textStatus = 'Manuel'
          break
        case 'IN PROGRESS':
          this.colorIconStatus = 'grey lighten-1'
          this.iconStatus = 'mdi-progress-clock'
          this.textStatus = 'En cours'
          break
        case 'ERROR':
          this.colorIconStatus = 'error'
          this.iconStatus = 'mdi-close-circle'
          this.textStatus = 'En erreur'
          break
        default:
          console.log('Sorry, we are out of ' + this.textStatus + '.')
      }
    },
    setColorByTypologie() {
      switch (this.clientTypolgie) {
        case 'SAAS DEDIE':
          this.colorCircular = 'teal lighten-2'
          this.clientTypolgie = 'SaaS Dédié'
          break
        case 'SAAS DESYNCHRONISE':
          this.colorCircular = 'lime lighten-2'
          this.clientTypolgie = 'SaaS Désynchronisé'
          break
        case 'SAAS MUTUALISE':
          this.colorCircular = 'red lighten-2'
          this.clientTypolgie = 'SaaS Mutualisé'
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
