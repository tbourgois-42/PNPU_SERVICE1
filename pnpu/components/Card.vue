<template>
  <nuxt-link
    :to="{
      name: 'client',
      params: {
        client: clientName,
        step: currentStep,
        workflowDate: workflowDate,
        textStatus: localtextStatus,
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
          <v-card-subtitle class="pb-0">ID {{ localIDORGA }}</v-card-subtitle>
          <span class="ma-0 pl-4 subtitle-2 body-2"
            >Step {{ currentStep }} / {{ maxStep }}</span
          >
          <v-card-subtitle>{{ localclientTypolgie }}</v-card-subtitle>
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
            :color="localcolorIconStatus"
            class="ml-3 mb-3"
            text-color="white"
          >
            <v-icon left color="white">{{ localiconStatus }}</v-icon>
            {{ localtextStatus }}
          </v-chip>
        </div>
        <v-progress-circular
          :rotate="-90"
          :size="120"
          :width="10"
          :value="localpercentCircular"
          :color="localcolorCircular"
          class="ma-5"
        >
          {{ localpercentCircular }}
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
    textStatus: { type: String, default: null },
    percentCircular: { type: Number, default: 0 },
    colorCircular: { type: String, default: 'primary' },
    workflowDate: { type: String, default: '' },
    idorga: { type: String, default: '' },
    workflowID: { type: String, default: '' }
  },
  data() {
    return {
      localcolorCircular: this.colorCircular,
      localiconStatus: this.iconStatus,
      localcolorIconStatus: this.colorIconStatus,
      localpercentCircular: this.percentCircular,
      localtextStatus: this.textStatus,
      localclientTypolgie: this.clientTypolgie,
      localIDORGA: this.IDORGA
    }
  },
  beforeMount() {
    this.getStatusClient()
    this.setColorByTypologie()
  },
  methods: {
    getStatusClient() {
      switch (this.localtextStatus) {
        case 'COMPLETED':
          this.localcolorIconStatus = 'success'
          this.localiconStatus = 'mdi-check-circle'
          this.localpercentCircular = 100
          this.localtextStatus = 'Terminé'
          break
        case 'WARNING':
          this.localcolorIconStatus = 'warning'
          this.localiconStatus = 'mdi-hand'
          this.localtextStatus = 'Manuel'
          break
        case 'IN PROGRESS':
          this.localcolorIconStatus = 'grey lighten-1'
          this.localiconStatus = 'mdi-progress-clock'
          this.localtextStatus = 'En cours'
          break
        case 'ERROR':
          this.localcolorIconStatus = 'error'
          this.localiconStatus = 'mdi-close-circle'
          this.localtextStatus = 'En erreur'
          break
        default:
          console.log('Sorry, we are out of ' + this.localtextStatus + '.')
      }
    },
    setColorByTypologie() {
      switch (this.localclientTypolgie) {
        case 'SAAS DEDIE':
          this.localcolorCircular = 'teal lighten-2'
          this.localclientTypolgie = 'SaaS Dédié'
          break
        case 'SAAS DESYNCHRONISE':
          this.localcolorCircular = 'lime lighten-2'
          this.localclientTypolgie = 'SaaS Désynchronisé'
          break
        case 'SAAS MUTUALISE':
          this.localcolorCircular = 'red lighten-2'
          this.localclientTypolgie = 'SaaS Mutualisé'
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
