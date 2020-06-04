<template>
  <v-col>
    <v-data-table
      :headers="headersWorkflow"
      :items="workflows"
      sort-by="calories"
      :page.sync="page"
      :items-per-page="itemsPerPage"
      hide-default-footer
      class="elevation-1"
      :loading="loadingData"
      loading-text="Chargement des workflows"
      @page-count="pageCount = $event"
    >
      <template v-slot:top>
        <v-toolbar flat color="white">
          <v-toolbar-title>Mes workflows</v-toolbar-title>
          <v-divider class="mx-4" inset vertical></v-divider>
          <v-spacer></v-spacer>
          <v-dialog v-model="dialogWorkflow" max-width="500px">
            <template v-slot:activator="{ on }">
              <v-btn color="primary" dark class="mb-2" v-on="on"
                >Créer un workflow</v-btn
              >
            </template>
            <v-card>
              <v-card-title>
                <span class="headline">{{ formTitle }}</span>
                <v-spacer></v-spacer>
                <v-icon>mdi-plus-circle</v-icon>
              </v-card-title>
              <v-divider></v-divider>
              <v-card-text>
                <v-container>
                  <v-row>
                    <v-col cols="12" sm="6" md="12">
                      <v-form
                        ref="form"
                        v-model="valid"
                        lazy-validation
                      >
                        <v-text-field
                          ref="autofocus"
                          v-model="editedItem.WORKFLOW_LABEL"
                          label="Nom du workflow"
                          :rules="[
                            (v) => !!v || 'Le nom du workflow est obligatoire'
                          ]"
                          required
                          @keypress="pressEnter($event)"
                        ></v-text-field>
                      </v-form>
                    </v-col>
                  </v-row>
                </v-container>
              </v-card-text>

              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="blue darken-1" text @click="close">Cancel</v-btn>
                <v-btn
                  :disabled="!valid"
                  color="blue darken-1"
                  text
                  @click="save()"
                  >Save</v-btn
                >
              </v-card-actions>
            </v-card>
          </v-dialog>
        </v-toolbar>
      </template>
      <template v-slot:item.actions="{ item }">
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-icon small class="mr-2" v-on="on" @click="editItem(item)">
              mdi-pencil
            </v-icon>
          </template>
          <span>Modifier</span>
        </v-tooltip>
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-icon small class="mr-2" v-on="on" @click="deleteItem(item)">
              mdi-delete
            </v-icon>
          </template>
          <span>Supprimer</span>
        </v-tooltip>
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-icon small class="mr-2" v-on="on" @click="affectItem(item)">
              mdi-arrow-down-bold
            </v-icon>
          </template>
          <span>Affecter les processus sélectionnés au workflow</span>
        </v-tooltip>
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-icon small v-on="on" @click="showDetail(item)">
              mdi-eye
            </v-icon>
          </template>
          <span>Voir le detail</span>
        </v-tooltip>
      </template>
      <template v-slot:no-data>
        <v-btn color="primary" @click="initialize">Reset</v-btn>
      </template>
    </v-data-table>
    <div class="text-center pt-2">
      <v-pagination v-model="page" :length="pageCount"></v-pagination>
    </div>
    <!-- detail workflow -->
    <v-dialog v-model="dialogDetailWorkflow" max-width="500px">
      <v-card class="pb-4">
        <v-card-title>
          Détails
          <v-spacer></v-spacer><v-icon>mdi-eye</v-icon>
        </v-card-title>
        <v-card-subtitle class="pt-2">
          {{ workflowDate }}
        </v-card-subtitle>
        <v-divider class="my-4"></v-divider>
        <v-data-table
          :headers="headersProcessus"
          :items="WorkflowProcesses"
          :loading="loadingWorkflowProcesses"
          loading-text="Chargement des processus associés..."
          hide-default-footer
          class="ma-2"
        >
        </v-data-table>
      </v-card>
    </v-dialog>
    <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
      {{ snackbarMessage }}
      <v-btn dark text @click="snackbar = false">
        Close
      </v-btn>
    </v-snackbar>
  </v-col>
</template>

<script>
import axios from 'axios'
export default {
  props: {
    selectedProcessus: {
      type: Array,
      default: () => []
    }
  },
  data: () => ({
    dialogWorkflow: false,
    dialogDetailWorkflow: false,
    editedIndex: -1,
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
    WorkflowProcesses: [],
    headersWorkflow: [
      {
        text: 'Nom',
        align: 'start',
        sortable: false,
        value: 'WORKFLOW_LABEL'
      },
      { text: 'Identifiant', value: 'WORKFLOW_ID', sortable: true },
      { text: 'Nombre de processus', value: 'NB_PROCESS', sortable: false },
      { text: 'Actions', value: 'actions', sortable: false }
    ],
    headersProcessus: [
      {
        text: 'Nom',
        align: 'start',
        sortable: false,
        value: 'PROCESS_LABEL'
      },
      { text: "Ordre d'éxecution", value: 'ORDER_ID' }
    ],
    workflows: [],
    page: 1,
    itemsPerPage: 10,
    pageCount: 0,
    editedItem: {
      WORKFLOW_LABEL: '',
      WORKFLOW_ID: 0,
      actions: 0,
      NB_PROCESS: 0
    },
    defaultItem: {
      WORKFLOW_LABEL: '',
      WORKFLOW_ID: 0,
      actions: 0,
      NB_PROCESS: 0
    },
    workflowDate: '',
    loadingData: true,
    loadingWorkflowProcesses: true,
    valid: true
  }),

  computed: {
    formTitle() {
      return this.editedIndex === -1
        ? "Ajout d'un workflow"
        : "Edition d'un workflow"
    }
  },

  watch: {
    dialogWorkflow(val) {
      val || this.close()
    }
  },

  created() {
    this.initialize()
  },

  methods: {
    pressEnter(e) {
      if (e.key === 'Enter') {
        this.save()
      }
    },
    /**
     * Charge les workflows depuis PNPU_WORKFLOW.
     */
    async initialize() {
      const vm = this
      try {
        const res = await axios.get(`${process.env.WEB_SERVICE_WCF}/workflow`)
        this.workflows = res.data.GetAllWorkFLowResult
        this.loadingData = false
      } catch (error) {
        vm.showSnackbar('error', `${error} !`)
      }
    },

    editItem(item) {
      this.editedIndex = this.workflows.indexOf(item)
      this.editedItem = Object.assign({}, item)
      this.dialogWorkflow = true
    },

    deleteItem(item) {
      const index = this.workflows.indexOf(item)
      if (confirm('Etes-vous sûr de supprimer ce workflow ?') === true) {
        const vm = this
        axios
          .delete(
            `${process.env.WEB_SERVICE_WCF}/workflow/` +
              item.WORKFLOW_ID +
              `/Delete`
          )
          .then(function(response) {
            if (response.status !== 200) {
              vm.showSnackbar(
                'error',
                `Modification impossible - HTTP error ${response.status} !`
              )
            } else {
              vm.showSnackbar(
                'success',
                'Suppression du workflow effectuée avec succès !'
              )
              vm.workflows.splice(index, 1)
            }
          })
          .catch(function(error) {
            vm.showSnackbar('error', `${error} !`)
          })
      }
    },

    close() {
      this.dialogWorkflow = false
      setTimeout(() => {
        this.editedItem = Object.assign({}, this.defaultItem)
        this.editedIndex = -1
      }, 300)
    },

    save() {
      if (this.editedIndex > -1) {
        const index = this.editedIndex
        const item = this.editedItem
        const vm = this
        axios
          .put(
            `${process.env.WEB_SERVICE_WCF}/workflow/` +
              this.editedItem.WORKFLOW_ID,
            {
              WORKFLOW_LABEL: this.editedItem.WORKFLOW_LABEL
            }
          )
          .then(function(response) {
            Object.assign(vm.workflows[index], item)
            vm.showSnackbar(
              'success',
              'Modification du workflow effectuée avec succès !'
            )
          })
          .catch(function(error) {
            vm.showSnackbar('error', `${error} !`)
          })
      } else if (this.editedItem.WORKFLOW_LABEL !== '') {
        const vm = this
        axios
          .post(`${process.env.WEB_SERVICE_WCF}/Workflow/CreateWorkflow/`, {
            WORKFLOW_LABEL: this.editedItem.WORKFLOW_LABEL
          })
          .then(function(response) {
            if (response.status !== 200) {
              vm.showSnackbar(
                'error',
                `Création impossible - HTTP error ${response.status} !`
              )
            } else {
              vm.workflows.push({
                WORKFLOW_LABEL: vm.editedItem.WORKFLOW_LABEL,
                WORKFLOW_ID: response.data,
                NB_PROCESS: 0
              })
              vm.showSnackbar(
                'success',
                'Création du workflow effectuée avec succès !'
              )
            }
          })
          .catch(function(error) {
            vm.showSnackbar('error', `${error} !`)
          })
      }
      this.close()
    },

    async showDetail(item) {
      const vm = this
      this.dialogDetailWorkflow = true
      this.workflowDate = item.WORKFLOW_LABEL
      this.WorkflowProcesses = []
      try {
        const res = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/workflow/` +
            item.WORKFLOW_ID +
            '/processus'
        )
        this.loadingWorkflowProcesses = false
        this.WorkflowProcesses = res.data.GetWorkflowProcessesResult
      } catch (error) {
        vm.showSnackbar('error', `${error} !`)
      }
    },

    affectItem(item) {
      if (this.selectedProcessus !== undefined) {
        if (this.selectedProcessus.length !== 0) {
          item.processus = []
          const vm = this
          this.selectedProcessus.forEach((element, index) => {
            const oneProcessus = {
              ID_WORKFLOW: item.WORKFLOW_ID,
              ID_PROCESS: element.ID_PROCESS,
              ID_ORDER: index,
              NB_PROCESS: vm.selectedProcessus.lenght
            }
            axios
              .post(
                `${process.env.WEB_SERVICE_WCF}/Workflow/` + item.WORKFLOW_ID,
                oneProcessus
              )
              .then(function(response) {
                item.NB_PROCESS = vm.selectedProcessus.length
              })
              .catch(function(error) {
                vm.showSnackbar('error', `${error} !`)
              })
            item.processus.push(element)
            this.showSnackbar('success', 'Affectation effectué avec succès')
          })
        } else {
          this.showSnackbar(
            'error',
            'Il est nécessaire de selectionner au minimum un processus pour pouvoir les affecter à un workflow'
          )
        }
      }
    },

    showSnackbar(color, message) {
      this.snackbar = true
      this.colorsnackbar = color
      this.snackbarMessage = message
    }
  }
}
</script>
