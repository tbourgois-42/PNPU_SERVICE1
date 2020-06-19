<template>
  <v-layout>
    <v-row>
      <v-col cols="12" class="d-flex space-between align-start py-0">
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

                  <v-list v-model="workflows.INSTANCE_NAME">
                    <v-list-item
                      v-for="(workflow, id) in workflows"
                      :key="id"
                      @click="majDashboard(workflow)"
                    >
                      <v-list-item-title>{{
                        workflow.INSTANCE_NAME
                      }}</v-list-item-title>
                    </v-list-item>
                  </v-list>
                </v-menu>
              </v-list-item-title>
              <v-list-item-subtitle>
                {{ workflowDiplayed }} | {{ workflowStatut }}
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
            <nuxt-link
              :to="{
                name: 'client-id',
                params: {
                  id: item.CLIENT_ID,
                  clientName: item.CLIENT_NAME,
                  clientId: item.CLIENT_ID,
                  step: item.ORDER_ID,
                  workflowDate: workflowDiplayed,
                  textStatus: item.ID_STATUT,
                  workflowID: workflowID,
                  idInstanceWF: idInstanceWF
                }
              }"
              append
              tag="span"
              class="route"
            >
              <v-hover v-slot="{ hover }">
                <v-card
                  :elevation="hover ? 6 : 1"
                  class="mx-auto transition-swing cursor"
                  outlined
                >
                  <v-col class="d-flex">
                    <v-list-item>
                      <v-list-item-content>
                        <v-tooltip top>
                          <template v-slot:activator="{ on }">
                            <v-list-item-title
                              v-on="on"
                              class="title overflow_ellipsis"
                              >{{ item.CLIENT_NAME }}</v-list-item-title
                            >
                          </template>
                          <span>{{ item.CLIENT_NAME }}</span>
                        </v-tooltip>
                        <v-list-item-subtitle class="pb-1"
                          >ID {{ item.ID_ORGANIZATION }}</v-list-item-subtitle
                        >
                        <v-list-item-subtitle class="pb-2"
                          >Step {{ item.ORDER_ID }}/
                          {{ maxStep }}</v-list-item-subtitle
                        >
                        <v-list-item-subtitle>
                          <v-chip
                            :color="item.colorCircular"
                            text-color="white"
                            class="pl-2"
                            label
                          >
                            <v-icon left>mdi-label</v-icon>
                            {{ item.TYPOLOGY }}
                          </v-chip>
                        </v-list-item-subtitle>
                      </v-list-item-content>
                    </v-list-item>
                    <v-progress-circular
                      :size="75"
                      :width="5"
                      :value="item.PERCENTAGE_COMPLETUDE"
                      :color="item.colorCircular"
                      class="mt-2 mr-2"
                    >
                      {{ item.PERCENTAGE_COMPLETUDE.toFixed(0) }}
                    </v-progress-circular>
                  </v-col>
                  <v-divider class="mx-4"></v-divider>
                  <v-card-actions class="ma-1 mx-4 d-flex justify-start">
                    <v-chip
                      :color="item.colorIconStatus"
                      class="ml-1 mr-2"
                      text-color="white"
                      label
                    >
                      <v-icon left color="white">{{ item.iconStatus }}</v-icon>
                      {{ item.ID_STATUT }}
                    </v-chip>
                    <v-chip class="pl-2" label>
                      <v-avatar left color="grey ligthen-1">
                        <!--{{ nbLocalisation }}-->
                        {{ item.NB_LOCALISATION }}
                      </v-avatar>
                      Localisation
                    </v-chip>
                    <v-icon color="error">mdi-exclamation-thick</v-icon>
                  </v-card-actions>
                </v-card>
              </v-hover>
            </nuxt-link>
          </transition>
        </v-col>
      </v-col>
      <v-col cols="2">
        <CardLaunchWorkflow
          :clients="items"
          :typologie="typologie"
          :workflowID="workflowID"
        />
        <CardProgressTypologie :clients="items" />
        <CardIndicateurs :clients="items" @getIndicators="getIndicators" />
      </v-col>
      <v-col cols="12" class="justify-center">
        <v-pagination
          v-model="currentPage"
          :length="totalPages()"
          circle
          class="bottomPagination mb-6"
          @input="updatePage(currentPage)"
        ></v-pagination>
      </v-col>
      <v-overlay :value="loadingData">
        <v-progress-circular indeterminate size="64"></v-progress-circular>
      </v-overlay>
    </v-row>
    <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
      {{ snackbarMessage }}
      <v-btn dark text @click="snackbar = false">
        Close
      </v-btn>
    </v-snackbar>
  </v-layout>
</template>

<script>
import axios from 'axios'
import CardLaunchWorkflow from '../components/CardLaunchWorkflow'
import CardIndicateurs from '../components/CardIndicateurs'
import CardProgressTypologie from '../components/CardProgressTypologie'
export default {
  components: {
    CardLaunchWorkflow,
    CardProgressTypologie,
    CardIndicateurs
  },
  data: () => ({
    items: [],
    workflows: [],
    workflowDiplayed: '',
    workflowStatut: '',
    workflowNameInstance: '',
    workflowID: '0',
    title: 'Dashboard',
    pageSize: 12,
    currentPage: 1,
    visibleItems: [],
    maxStep: 7,
    search: '',
    filter: '',
    typologie: [
      { value: 256, text: 'SaaS Dédié' },
      { value: 257, text: 'SaaS Mutualisé' },
      { value: 258, text: 'SaaS Désynchronisé' }
    ],
    filteredIndicators: [],
    colorIconStatus: '',
    iconStatus: '',
    percentCircular: '',
    textStatus: '',
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
    loadingData: false,
    eventEmitted: false,
    nbLocalisation: 0,
    idInstanceWF: '',
    filterSearch: false
  }),

  computed: {
    colorLocalisation() {
      if (this.nbLocalisation >= 0 && this.nbLocalisation <= 50) return 'indigo'
      if (this.nbLocalisation >= 51 && this.nbLocalisation <= 60) return 'teal'
      if (this.nbLocalisation >= 61 && this.nbLocalisation <= 80) return 'green'
      return 'red'
    }
  },

  watch: {
    /**
     * Met à jour l'affichage si un filtre est sélectionné.
     */
    filteredIndicators() {
      this.updateVisibleItems()
    },

    /**
     * Recherche d'un client.
     */
    search() {
      this.visibleItems = []
      this.items.forEach((element) => {
        if (
          element.CLIENT_NAME.toUpperCase().match(this.search.toUpperCase()) !==
          null
        ) {
          if (this.visibleItems.length < this.pageSize) {
            this.visibleItems.push(element)
            this.filterSearch = true
            this.eventEmitted = false
            this.totalPages()
          } else {
            this.filterSearch = false
          }
        }
      })
    }
  },

  created() {
    this.updateVisibleItems()
    this.totalPages()
    this.getHistoricWorkflow()
    // this.initialize()
    // this.getMaxStep()
  },

  methods: {
    /**
     * Met à jour les informations des cartes.
     */
    setCardInfos() {
      this.items.forEach((element) => {
        switch (element.TYPOLOGY) {
          case 'SAAS DEDIE':
            element.colorCircular = 'teal lighten-2'
            break
          case 'SAAS DESYNCHRONISE':
            element.colorCircular = 'lime lighten-2'
            break
          case 'SAAS MUTUALISE':
            element.colorCircular = 'red lighten-2'
            break
        }
        switch (element.ID_STATUT) {
          case 'CORRECT':
            element.colorIconStatus = 'success'
            element.iconStatus = 'mdi-check-circle'
            element.ID_STATUT = 'Terminé'
            break
          case 'WARNING':
            element.colorIconStatus = 'warning'
            element.iconStatus = 'mdi-hand'
            element.ID_STATUT = 'Manuel'
            break
          case 'ERROR':
            element.colorIconStatus = 'error'
            element.iconStatus = 'mdi-alert-circle'
            element.ID_STATUT = 'En erreur'
            break
          case 'IN PROGRESS':
            element.colorIconStatus = 'grey lighten-1'
            element.iconStatus = 'mdi-progress-clock'
            element.ID_STATUT = 'En cours'
            break
        }
      })
    },

    /**
     * Met à jour la page courante.
     */
    updatePage(pageNumber) {
      this.currentPage = pageNumber
      this.updateVisibleItems()
    },

    /**
     * Gère l'affichage des cartes en fonction du nombre de page 12 / Page.
     */
    updateVisibleItems() {
      this.visibleItems = []
      if (this.filteredIndicators.length > 0) {
        this.visibleItems = this.filteredIndicators.slice(
          (this.currentPage - 1) * this.pageSize,
          this.currentPage * 12
        )
      } else if (this.eventEmitted === false) {
        this.visibleItems = this.items.slice(
          (this.currentPage - 1) * this.pageSize,
          this.currentPage * 12
        )
      }
    },

    /**
     * Calcul le nombre total de pages.
     */
    totalPages() {
      if (this.eventEmitted === true) {
        this.eventEmitted = false
        return Math.ceil(this.filteredIndicators.length / this.pageSize)
      } else {
        return Math.ceil(this.items.length / this.pageSize)
      }
    },

    /**
     * Chargement des informations pour les cartes.
     */
    initialize() {
      const vm = this
      vm.loadingData = true
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/Clients/Dashboard/` +
            this.workflowID +
            '/' +
            this.idInstanceWF
        )
        .then(function(response) {
          vm.items = response.data.GetInfoAllClientResult
          if (response.data.GetInfoAllClientResult.length > 0) {
            vm.workflowID = response.data.GetInfoAllClientResult[0].WORKFLOW_ID.toString()
            vm.getWorkflowName(vm.workflowID)
            vm.getMaxStep()
          }
          vm.setCardInfos()
          vm.items.forEach((client) => {
            axios
              .get(
                `${process.env.WEB_SERVICE_WCF}/localisation/workflow/` +
                  client.WORKFLOW_ID +
                  '/' +
                  client.ID_H_WORKFLOW +
                  '/' +
                  client.CLIENT_ID
              )
              .then(function(response) {
                client.NB_LOCALISATION = response.data.GetNbLocalisationResult
              })
              .catch(function(error) {
                vm.showSnackbar('error', `${error}`)
              })
          })
          vm.updateVisibleItems()
          vm.loadingData = false
        })
        .catch(function(error) {
          vm.showSnackbar(
            'error',
            `${error} ! Impossible de récupérer l'historique des steps`
          )
          vm.loadingData = false
        })
    },

    /**
     * Indicateur sélectionné pour filtrer l'affiche des cartes.
     */
    getIndicators(value) {
      this.filteredIndicators = value
      this.eventEmitted = true
    },

    /**
     * Récupère l'historique des workflows order by LAUNCHING_DATE.
     */
    async getHistoricWorkflow() {
      try {
        const response = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/workflow/historic`
        )
        if (response.status === 200) {
          this.workflows = response.data.GetHWorkflowResult
          // On récupère le dernier workflow lancé
          this.workflowID = response.data.GetHWorkflowResult[
            this.workflows.length - 1
          ].WORKFLOW_ID.toString()
          this.workflowDisplayed =
            response.data.GetHWorkflowResult[
              this.workflows.length - 1
            ].WORKFLOW_LABEL
          this.workflowStatut =
            response.data.GetHWorkflowResult[
              this.workflows.length - 1
            ].STATUT_GLOBAL
          this.workflowNameInstance =
            response.data.GetHWorkflowResult[
              this.workflows.length - 1
            ].INSTANCE_NAME
          this.idInstanceWF =
            response.data.GetHWorkflowResult[
              this.workflows.length - 1
            ].ID_H_WORKFLOW
          this.initialize()
          this.getMaxStep()
        }
      } catch (error) {
        this.showSnackbar(
          'error',
          `${error} ! Une erreur s'est produite lors de la récupération de l'historique des workflows`
        )
      }
    },

    /**
     * Récupère les informations du workflow affiché à l'écran.
     * @param {object} workflowID - Workflow sélectionné dans l'entête
     */
    getWorkflowName(workflowID) {
      for (let index = 0; index < this.workflows.length; index++) {
        const element = this.workflows[index]
        if (element.ID_H_WORKFLOW === this.idInstanceWF) {
          this.workflowDiplayed = element.INSTANCE_NAME
          this.workflowStatut = element.STATUT_GLOBAL
        }
      }
    },

    /**
     * Met à jour l'affichage des tuiles du Dashboard.
     * @param {object} workflow - Workflow sélectionné dans l'entête
     */
    majDashboard(item) {
      const vm = this
      vm.items = []
      vm.visibleItems = []
      vm.filteredIndicators = []
      vm.eventEmitted = false
      vm.loadingData = true
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/Clients/Dashboard/` +
            item.WORKFLOW_ID +
            '/' +
            item.ID_H_WORKFLOW
        )
        .then(function(response) {
          vm.items = response.data.GetInfoAllClientResult
          vm.workflowID = item.WORKFLOW_ID.toString()
          vm.getWorkflowName(vm.workflowID)
          vm.getMaxStep()
          vm.setCardInfos()
          vm.items.forEach((client) => {
            axios
              .get(
                `${process.env.WEB_SERVICE_WCF}/localisation/workflow/` +
                  client.WORKFLOW_ID +
                  '/' +
                  client.ID_H_WORKFLOW +
                  '/' +
                  client.CLIENT_ID
              )
              .then(function(response) {
                client.NB_LOCALISATION = response.data.GetNbLocalisationResult
              })
              .catch(function(error) {
                vm.showSnackbar('error', `${error}`)
              })
          })
          vm.updateVisibleItems()
          vm.loadingData = false
        })
        .catch(function(error) {
          vm.showSnackbar(
            'error',
            `${error} ! Impossible de récupérer l'historique des workflows`
          )
          vm.loadingData = false
        })
    },

    /**
     * Récupère le nombre maximal de step pour un workflow donné.
     */
    async getMaxStep() {
      try {
        const response = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/workflow/` +
            this.workflowID +
            `/maxstep`
        )
        if (response.status === 200) {
          this.maxStep = response.data
        }
      } catch (error) {
        this.showSnackbar(
          'error',
          `${error} ! Impossible de récupérer le nombre max de step, la valeur 7 par defaut est appliquée dans l'affichage de la carte`
        )
      }
    },

    /**
     * Gére l'affichage du snackbar.
     * @param {string} color - Couleur de la snackbar.
     * @param {string} message - Message affiché dans la snackbar.
     */
    showSnackbar(color, message) {
      this.snackbar = true
      this.colorsnackbar = color
      this.snackbarMessage = message
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
.cursor {
  cursor: pointer;
}
.overflow_ellipsis {
  width: 0.75em;
  text-overflow: ellipsis;
}
</style>
