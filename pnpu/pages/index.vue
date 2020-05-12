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

                  <v-list v-model="workflows.WORKFLOW_LABEL">
                    <v-list-item
                      v-for="(workflow, id) in workflows"
                      :key="id"
                      @click="majDashboard(workflow)"
                    >
                      <v-list-item-title>{{
                        workflow.WORKFLOW_LABEL
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
            <!--<CardPnpu
              :client-name="item.CLIENT_ID"
              :maxStep="maxStep"
              :clientTypolgie="item.TYPOLOGY"
              :textStatus="item.ID_STATUT"
              :currentStep="item.ORDER_ID"
              :percentCircular="item.PERCENTAGE_COMPLETUDE"
              :workflowDate="workflowDiplayed"
              :workflowID="workflowID"
              :idorga="item.ID_ORGANIZATION"
            />-->
            <v-hover v-slot="{ hover }">
              <v-card
                :elevation="hover ? 6 : 1"
                class="mx-auto transition-swing cursor"
                outlined
              >
                <v-col class="d-flex">
                  <v-list-item>
                    <v-list-item-content>
                      <v-list-item-title class="title">{{ item.CLIENT_ID }}</v-list-item-title>
                      <v-list-item-subtitle class="pb-1">ID {{ item.ID_ORGANIZATION }}</v-list-item-subtitle>
                      <v-list-item-subtitle class="pb-2">Step {{ item.ORDER_ID }}/ {{ maxStep }}</v-list-item-subtitle>
                      <v-list-item-subtitle>
                        <!--<v-chip color="teal lighten-2" text-color="white" class="pl-2" label>-->
                        <v-chip :color="item.colorCircular" text-color="white" class="pl-2" label>
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
                    {{ item.PERCENTAGE_COMPLETUDE }}
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
                    <v-icon left color="white">mdi-check-circle</v-icon>
                    {{ item.ID_STATUT }}
                  </v-chip>
                  <v-chip class="pl-2" label>
                    <v-avatar
                      left
                      color="grey lighten-1"
                    >
                      15
                    </v-avatar>
                    Localisation
                  </v-chip>
                </v-card-actions>
              </v-card>
            </v-hover>
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
import CardPnpu from '../components/Card.vue'
import CardLaunchWorkflow from '../components/CardLaunchWorkflow'
import CardIndicateurs from '../components/CardIndicateurs'
import CardProgressTypologie from '../components/CardProgressTypologie'
export default {
  components: {
    CardPnpu,
    CardLaunchWorkflow,
    CardProgressTypologie,
    CardIndicateurs
  },
  data: () => ({
    items: [],
    workflows: '',
    workflowDiplayed: '',
    workflowStatut: '',
    workflowID: '0',
    title: 'Dashboard',
    pageSize: 12,
    currentPage: 1,
    visibleItems: [],
    maxStep: 7,
    search: '',
    filter: '',
    typologie: ['SaaS Dédié', 'SaaS Mutualisé', 'SaaS Désynchronisé'],
    filteredIndicators: [],
    colorIconStatus: '',
    iconStatus: '',
    percentCircular: '',
    textStatus: '',
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
    loadingData: false,
    eventEmitted: false
  }),

  computed: {},

  watch: {
    filteredIndicators() {
      this.updateVisibleItems()
    }
  },

  created() {},

  beforeMount() {
    this.updateVisibleItems()
    this.totalPages()
  },

  mounted() {
    this.getHistoricWorkflow()
    this.initialize()
    this.getMaxStep()
  },

  methods: {
    setCardInfos(){
      this.items.forEach(element => {
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
            break
          case 'WARNING':
            element.colorIconStatus = 'warning'
            break
          case 'ERROR':
            element.colorIconStatus = 'error'
            break
          case 'IN PROGRESS':
            element.colorIconStatus = 'grey lighten-1'
            break
        }
      })
    },
    updatePage(pageNumber) {
      this.currentPage = pageNumber
      this.updateVisibleItems()
    },
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
    totalPages() {
      if (this.eventEmitted === true) {
        return Math.ceil(this.filteredIndicators.length / this.pageSize)
        this.eventEmitted = false
      } else {
        return Math.ceil(this.items.length / this.pageSize)
      }
    },
    initialize() {
      const vm = this
      vm.loadingData = true
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/Clients/Dashboard/` + this.workflowID
        )
        .then(function(response) {
          vm.items = response.data.GetInfoAllClientResult
          if (response.data.GetInfoAllClientResult.length > 0) {
            vm.workflowID = response.data.GetInfoAllClientResult[0].WORKFLOW_ID.toString()
            vm.getWorkflowName(vm.workflowID)
          }  
          vm.setCardInfos()
          vm.updateVisibleItems()
          vm.loadingData = false
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} ! Impossible de récupérer l'historique des steps`)
          vm.loadingData = false
        })
    },
    getIndicators(value) {
      this.filteredIndicators = value
      this.eventEmitted = true
    },
    getHistoricWorkflow() {
      const vm = this
      axios
        .get(`${process.env.WEB_SERVICE_WCF}/workflow/historic`)
        .then(function(response) {
          vm.workflows = response.data.GetHWorkflowResult
          vm.workflowID = response.data.GetHWorkflowResult[0].WORKFLOW_ID.toString()
          vm.workflows.forEach((element) => {
            if (element.WORKFLOW_ID.toString() === vm.workflowID) {
              vm.workflowDiplayed = element.WORKFLOW_LABEL
              vm.workflowStatut = element.STATUT_GLOBAL
            }
          })
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} ! `)
        })
    },

    /**
     * Récupère les informations du workflow affiché à l'écran.
     * @param {object} workflowID - Workflow sélectionné dans l'entête
     */
    getWorkflowName(workflowID) {
      this.workflows.forEach(element => {
        if (element.WORKFLOW_ID.toString() === workflowID) {
          this.workflowDiplayed = element.WORKFLOW_LABEL
          this.workflowStatut = element.STATUT_GLOBAL
        }
      })
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
          `${process.env.WEB_SERVICE_WCF}/Clients/Dashboard/` + item.WORKFLOW_ID
        )
        .then(function(response) {
          vm.items = response.data.GetInfoAllClientResult
          vm.workflowID = item.WORKFLOW_ID.toString()
          vm.getWorkflowName(vm.workflowID)
          vm.getMaxStep()  
          vm.setCardInfos()
          vm.updateVisibleItems()
          vm.loadingData = false
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} ! Impossible de récupérer l'historique des workflows`)
          vm.loadingData = false
        })
    },
    
    /**
     * Récupère le nombre maximal de step pour un workflow donné.
     */
    getMaxStep() {
      const vm = this
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/workflow/` + vm.workflowID + `/maxstep`
        )
        .then(function(response) {
          vm.maxStep = response.data.GetMaxStepWorkflowResult
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} ! Impossible de récupérer le nombre de step du workflow ${vm.workflowDiplayed}`)
        })
    },

    /**
     * Gére l'affichage du snackbar.
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
</style>
