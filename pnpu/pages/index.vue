<template>
  <v-row>
    <v-col cols="12" class="d-flex justify-between py-0">
      <v-col cols="10">
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title class="title">
              {{ title }}
              <v-menu>
                <template v-slot:activator="{ on }">
                  <v-btn icon class="ml-8" v-on="on">
                    <v-icon>mdi-dots-vertical</v-icon>
                  </v-btn>
                </template>

                <v-list v-model="workflows.name">
                  <v-list-item
                    v-for="(workflow, id) in workflows"
                    :key="id"
                    @click="() => (workflowDate = workflow.name)"
                  >
                    <v-list-item-title>{{ workflow.name }}</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </v-list-item-title>
            <v-list-item-subtitle>
              Workflow {{ workflowDate }}
            </v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-col>

      <v-col cols="2">
        <v-text-field
          v-model="search"
          hide-details
          label="Chercher un client"
          append-icon="mdi-magnify"
          class="mt-4 pl-2"
          solo
        ></v-text-field>
      </v-col>
    </v-col>
    <v-col cols="12">
      <v-divider class="mx-4 pa-0"></v-divider>
    </v-col>

    <v-col cols="10" class="d-flex flex-wrap justify-start">
      <v-col v-for="(item, id) in visibleItems" :key="id" cols="3">
        <transition appear name="slide-in">
          <CardPnpu
            :client-name="item.client"
            :maxStep="maxStep"
            :clientTypolgie="item.typologie"
            :textStatus="item.textStatus"
            :currentStep="item.step"
            :percentCircular="item.percent"
            :workflowDate="workflowDate"
          />
        </transition>
      </v-col>
    </v-col>
    <v-col cols="2">
      <v-card class="mb-4">
        <v-card-title class="d-flex justify-space-between subtitle-1"
          >Lancement workflow<v-icon>mdi-cog-box</v-icon></v-card-title
        >
        <v-divider class="mx-4"></v-divider>
        <v-btn
          class="my-4 ml-6"
          color="primary"
          @click="dialog = !dialog"
          v-on="on"
        >
          <v-icon left>mdi-play</v-icon>Lancer
        </v-btn>
      </v-card>
      <!-- Lancer un process -->
      <v-dialog v-model="dialog" max-width="500px">
        <v-card>
          <v-card-title class="headline">Lancement du workflow</v-card-title>
          <v-card-subtitle class="mt-1">{{ workflowDate }}</v-card-subtitle>
          <v-divider></v-divider>
          <v-card-text>
            <v-container>
              <v-row>
                <v-col cols="12" sm="12" md="12">
                  <v-select
                    :items="typologie"
                    label="Typologie"
                    chips
                    multiple
                    solo
                  ></v-select>
                  <v-select
                    v-model="items.client"
                    :items="items.client"
                    label="Client"
                    chips
                    multiple
                    solo
                  ></v-select>
                </v-col>
                <v-col cols="12" sm="12" md="12">
                  <v-file-input
                    v-model="files"
                    color="primary"
                    counter
                    label=".mdb .zip"
                    multiple
                    placeholder="Selection des fichiers"
                    prepend-icon="mdi-paperclip"
                    outlined
                    :show-size="1000"
                  >
                    <template v-slot:selection="{ index, text }">
                      <v-chip v-if="index < 2" color="primary" dark label small>
                        {{ text }}
                      </v-chip>

                      <span
                        v-else-if="index === 2"
                        class="overline grey--text text--darken-3 mx-2"
                      >
                        +{{ files.length - 2 }} Fichier(s)
                      </span>
                    </template>
                  </v-file-input>
                </v-col>
              </v-row>
            </v-container>
          </v-card-text>

          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="error" @click="close"
              ><v-icon left>mdi-cancel</v-icon> Annuler</v-btn
            >
            <v-btn color="primary" @click="testpost"
              ><v-icon left>mdi-play</v-icon> Lancer</v-btn
            >
          </v-card-actions>
        </v-card>
      </v-dialog>
      <!-- Fin Lancer un process -->
      <v-card class="mx-auto" max-width="374">
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
      <v-card class="my-4">
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
      <v-snackbar v-model="snackbar" color="success" :timeout="6000" top>
        {{ snackbarMessage }}
        <v-btn dark text @click="snackbar = false">
          Close
        </v-btn>
      </v-snackbar>
    </v-col>
    <v-col cols="12">
      <v-pagination
        v-model="currentPage"
        :length="totalPages()"
        @input="updatePage(currentPage)"
        circle
        class="bottomPagination mb-6"
      ></v-pagination>
    </v-col>
  </v-row>
</template>

<script>
import axios from 'axios'
import CardPnpu from '../components/Card.vue'
import ClientData from '../data/Clients.json'
import Workflow from '../data/Workflow.json'
export default {
  components: { CardPnpu },
  data: () => ({
    items: ClientData,
    workflows: Workflow,
    workflowDate: '03/2020',
    title: 'Dashboard',
    pageSize: 12,
    currentPage: 1,
    visibleItems: [],
    maxStep: 8,
    search: '',
    filter: '',
    colorCircularSaaSPlat: 'red lighten-2',
    colorCircularSaaSDedie: 'teal lighten-2',
    colorCircularSaaSDesync: 'lime lighten-2',
    progressSaaSDedie: '',
    progressSaaSDesynchronise: '',
    progressPlateforme: '',
    dialog: false,
    typologie: ['SaaS Dédié', 'Plateforme', 'SaaS Désynchronisé'],
    snackbarMessage: '',
    snackbar: false
  }),
  beforeMount() {
    this.updateVisibleItems()
    this.totalPages()
  },
  mounted() {
    this.calcProgessByTypologie()
  },
  methods: {
    updatePage(pageNumber) {
      this.currentPage = pageNumber
      this.updateVisibleItems()
    },
    updateVisibleItems() {
      this.visibleItems = this.items.slice(
        (this.currentPage - 1) * this.pageSize,
        this.currentPage * 12
      )
    },
    totalPages() {
      return Math.ceil(this.items.length / this.pageSize)
    },
    calcProgessByTypologie() {
      let progressSDedie = 0
      let progressSDesync = 0
      let progressPlat = 0
      let nbClientDesync = 0
      let nbClientPlat = 0
      let nbClientDedie = 0
      this.items.forEach((element) => {
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
    },
    testpost() {
      axios
        .post('https://jsonplaceholder.typicode.com/posts', {
          title: 'foo',
          body: 'bar',
          userId: 1
        })
        .then((response) => {
          console.log(response.status)
          if (response.status === 201) {
            this.snackbar = true
            this.snackbarMessage = 'Lancement effectué avec succès'
          }
        })
        .catch(function(error) {
          console.log(error)
        })
    },
    close() {
      this.dialog = false
    }
  },
  computed: {
    countDone() {
      return this.items.filter((items) => items.step === this.maxStep).length
    },
    countInProgress() {
      return this.items.filter((items) => items.textStatus === 'En cours')
        .length
    },
    countInError() {
      return this.items.filter((items) => items.textStatus === 'En erreur')
        .length
    },
    countManuel() {
      return this.items.filter((items) => items.textStatus === 'Manuel').length
    },
    filteredItems() {
      if (this.filter === 'InProgress') {
        return this.items.filter((items) => items.textStatus === 'En cours')
      } else if (this.filter === 'InError') {
        return this.items.filter((items) => items.textStatus === 'En erreur')
      } else if (this.filter === 'Manuel') {
        return this.items.filter((items) => items.textStatus === 'Manuel')
      } else if (this.filter === 'Done') {
        return this.items.filter((items) => items.step === this.maxStep)
      } else if (this.filter === 'Plateforme') {
        return this.items.filter((items) => items.typologie === 'Plateforme')
      } else if (this.filter === 'SaaS Dédié') {
        return this.items.filter((items) => items.typologie === 'SaaS Dédié')
      } else if (this.filter === 'SaaS Désynchronisé') {
        return this.items.filter(
          (items) => items.typologie === 'SaaS Désynchronisé'
        )
      } else if (this.search) {
        return this.items.filter((item) => {
          return item.client.toUpperCase().match(this.search.toUpperCase())
        })
      } else {
        this.updateVisibleItems()
      }
      return this.items
    }
  }
}
</script>

<style lang="css" scoped>
.container {
  max-width: max-content;
}

.bottomPagination {
  bottom: 0;
  position: absolute;
}

.slide-in-enter {
  opacity: 0;
  transform: scale(0.5);
}

.slide-in-enter-active {
  transition: all 0.4s ease;
}
</style>
