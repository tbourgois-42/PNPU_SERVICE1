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
            <CardPnpu
              :client-name="item.CLIENT_ID"
              :maxStep="maxStep"
              :clientTypolgie="item.TYPOLOGY"
              :textStatus="item.ID_STATUT"
              :currentStep="item.ORDER_ID"
              :percentCircular="item.PERCENTAGE_COMPLETUDE"
              :workflowDate="workflowDiplayed"
              :workflowID="workflowID"
              :idorga="item.ID_ORGANIZATION"
            />
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
    colorsnackbar: ''
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
    this.initialize()
  },

  methods: {
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
      } else {
        this.visibleItems = this.items.slice(
          (this.currentPage - 1) * this.pageSize,
          this.currentPage * 12
        )
      }
    },
    totalPages() {
      return Math.ceil(this.items.length / this.pageSize)
    },
    initialize() {
      const vm = this
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/Clients/Dashboard/` + this.workflowID
        )
        .then(function(response) {
          vm.items = response.data.GetInfoAllClientResult
          if (response.data.GetInfoAllClientResult.length > 0) {
            vm.workflowID = response.data.GetInfoAllClientResult[0].WORKFLOW_ID.toString()
            vm.getHistoricWorkflow()
          }
          vm.updateVisibleItems()
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} !`)
        })
    },
    getIndicators(value) {
      this.filteredIndicators = value
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
          vm.showSnackbar('error', `${error} !`)
        })
    },

    majDashboard(item) {
      const vm = this
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/Clients/Dashboard/` + item.WORKFLOW_ID
        )
        .then(function(response) {
          vm.items = response.data.GetInfoAllClientResult
          vm.workflowID = item.WORKFLOW_ID.toString()
          vm.workflows.forEach((element) => {
            if (element.WORKFLOW_ID.toString() === vm.workflowID) {
              vm.workflowDiplayed = element.WORKFLOW_LABEL
              vm.workflowStatut = element.STATUT_GLOBAL
            }
          })
          vm.updateVisibleItems()
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} !`)
        })
    },

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
