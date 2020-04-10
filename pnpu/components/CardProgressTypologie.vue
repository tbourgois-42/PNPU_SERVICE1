<template>
  <v-layout>
    <v-card min-width="277">
      <v-card-title class="d-flex justify-space-between subtitle-1">
        Avancement par typologie<v-btn
          icon
          @click.prevent="filterIndicators('NO FILTER')"
          ><v-icon>{{ iconFilter }}</v-icon></v-btn
        >
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
          SaaS Mutualisé<v-icon
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
  props: {
    clients: {
      type: Array,
      default: () => []
    }
  },

  data() {
    return {
      progressSaaSDedie: '',
      progressSaaSDesynchronise: '',
      progressPlateforme: '',
      colorCircularSaaSPlat: 'red lighten-2',
      colorCircularSaaSDedie: 'teal lighten-2',
      colorCircularSaaSDesync: 'lime lighten-2',
      localClients: '',
      iconFilter: 'mdi-filter'
    }
  },

  watch: {
    clients() {
      this.localClients = this.clients
      this.calcProgessByTypologie()
    }
  },

  methods: {
    calcProgessByTypologie() {
      let progressSDedie = 0
      let progressSDesync = 0
      let progressPlat = 0
      let nbClientDesync = 0
      let nbClientPlat = 0
      let nbClientDedie = 0
      this.localClients.forEach((element) => {
        if (element.TYPOLOGY === 'SAAS DESYNCHRONISE') {
          progressSDesync = progressSDesync + element.PERCENTAGE_COMPLETUDE
          nbClientDesync = nbClientDesync + 1
        } else if (element.TYPOLOGY === 'SAAS DEDIE') {
          progressSDedie = progressSDedie + element.PERCENTAGE_COMPLETUDE
          nbClientDedie = nbClientDedie + 1
        } else if (element.TYPOLOGY === 'SAAS MUTUALISE') {
          progressPlat = progressPlat + element.PERCENTAGE_COMPLETUDE
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
