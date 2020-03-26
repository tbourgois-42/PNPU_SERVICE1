<template>
  <v-hover v-slot="{ hover }">
    <v-card
      :elevation="hover ? 6 : 1"
      class="d-flex justify-lg-space-between transition-swing"
      transition="slide-y-transition"
      @click.prevent="filter = avancement.title"
    >
      <div>
        <v-card-title class="headline subtitle-2 text-uppercase">{{
          clientName
        }}</v-card-title>
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
</template>

<script>
export default {
  name: 'CardPnpu',
  props: {
    clientName: { type: String, default: 'clientName' },
    currentStep: { type: Number, default: 0 },
    maxStep: { type: Number, default: 10 },
    clientTypolgie: { type: String, default: 'clientTypolgie' },
    colorIconStatus: { type: String, default: 'grey lighten-1' },
    iconStatus: { type: String, default: 'mdi-progress-clock' },
    textStatus: { type: String, default: 'En cours' },
    percentCircular: { type: Number, default: 0 },
    colorCircular: { type: String, default: 'primary' }
  }
}
</script>

<style scoped>
.v-hover {
  cursor: pointer;
}
</style>
