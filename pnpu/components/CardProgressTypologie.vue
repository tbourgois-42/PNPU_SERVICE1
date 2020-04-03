<template>
  <v-layout>
    <v-card>
      <v-card-title class="d-flex justify-space-between subtitle-1">
        Avancement par typologie<v-icon>mdi-progress-clock</v-icon>
      </v-card-title>
      <v-divider class="mx-4"></v-divider>
      <v-card-text>
        <div class="mb-4 subtitle">
          Saas Dédié<v-icon
            v-if="progressSaaSDedie === 100"
            class="mx-4"
            color="success"
            small
            >mdi-check-decagram</v-icon
          >
        </div>
        <div>
          <v-tooltip top>
            <template v-slot:activator="{ on }">
              <v-progress-linear
                height="10"
                :value="progressSaaSDedie"
                striped
                class="my-4"
                :color="colorCircularSaaSDedie"
                v-on="on"
              ></v-progress-linear>
            </template>
            <span>{{ progressSaaSDedie }}</span>
          </v-tooltip>
        </div>
        <div class="mb-4 subtitle">
          Saas Désynchronisé<v-icon
            v-if="progressSaaSDesynchronise === 100"
            class="mx-4"
            color="success"
            small
            >mdi-check-decagram</v-icon
          >
        </div>
        <div>
          <v-tooltip top>
            <template v-slot:activator="{ on }">
              <v-progress-linear
                height="10"
                :value="progressSaaSDesynchronise"
                striped
                class="my-4"
                :color="colorCircularSaaSDesync"
                v-on="on"
              ></v-progress-linear>
            </template>
            <span>{{ progressSaaSDesynchronise }}</span>
          </v-tooltip>
        </div>
        <div class="mb-4 subtitle">
          Plateforme<v-icon
            v-if="progressPlateforme === 100"
            class="mx-4"
            color="success"
            small
            >mdi-check-decagram</v-icon
          >
        </div>

        <div>
          <v-tooltip top>
            <template v-slot:activator="{ on }">
              <v-progress-linear
                height="10"
                :value="progressPlateforme"
                striped
                class="mt-4"
                :color="colorCircularSaaSPlat"
                v-on="on"
              ></v-progress-linear>
            </template>
            <span>{{ progressPlateforme }}</span>
          </v-tooltip>
        </div>
      </v-card-text>
    </v-card>
  </v-layout>
</template>

<script>
export default {
  props: ['clients'],
  data: () => ({
    progressSaaSDedie: '',
    progressSaaSDesynchronise: '',
    progressPlateforme: '',
    colorCircularSaaSPlat: 'red lighten-2',
    colorCircularSaaSDedie: 'teal lighten-2',
    colorCircularSaaSDesync: 'lime lighten-2'
  }),

  mounted() {
    this.calcProgessByTypologie()
  },

  methods: {
    calcProgessByTypologie() {
      let progressSDedie = 0
      let progressSDesync = 0
      let progressPlat = 0
      let nbClientDesync = 0
      let nbClientPlat = 0
      let nbClientDedie = 0
      this.clients.forEach((element) => {
        if (element.typologie === 'SaaS Désynchronisé') {
          progressSDesync = progressSDesync + element.percent
          nbClientDesync = nbClientDesync + 1
        }
        if (element.typologie === 'SaaS Dédié') {
          progressSDedie = progressSDedie + element.percent
          nbClientDedie = nbClientDedie + 1
        }
        if (element.typologie === 'Plateforme') {
          progressPlat = progressPlat + element.percent
          nbClientPlat = nbClientPlat + 1
        }
      })
      this.progressPlateforme = Math.round(
        (progressPlat / (nbClientPlat * 100)) * 100
      )
      this.progressSaaSDedie = Math.round(
        (progressSDedie / (nbClientDedie * 100)) * 100
      )
      this.progressSaaSDesynchronise = Math.round(
        (progressSDesync / (nbClientDesync * 100)) * 100
      )
    }
  }
}
</script>
