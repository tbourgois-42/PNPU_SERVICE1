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
      @page-count="pageCount = $event"
      :loading="loadingData"
      loading-text="Chargement des workflows"
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
                      <v-text-field
                        v-model="editedItem.WORKFLOW_LABEL"
                        label="Nom du workflow"
                        required
                      ></v-text-field>
                    </v-col>
                  </v-row>
                </v-container>
              </v-card-text>

              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="blue darken-1" text @click="close">Cancel</v-btn>
                <v-btn color="blue darken-1" text @click="save()">Save</v-btn>
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
        <div v-if="showAlertNoProcessus" class="text-center">
          <v-alert type="info" class="ma-6">
            Aucun processus associé
          </v-alert>
        </div>
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
  props: ['selectedProcessus'],
  data: () => ({
    dialogWorkflow: false,
    dialogDetailWorkflow: false,
    editedIndex: -1,
    showAlertNoProcessus: false,
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
      { text: 'Actions', value: 'actions', sortable: false },
      { text: 'Modifié le', value: 'updated_at', sortable: false }
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
      actions: 0,
      updated_at: 0
    },
    defaultItem: {
      WORKFLOW_LABEL: '',
      actions: 0,
      updated_at: 0
    },
    workflowDate: '',
    loadingData: true,
    loadingWorkflowProcesses: true
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
    async initialize() {
      try {
        const res = await axios.get(
          'http://localhost:63267/Service1.svc/workflow'
        )
        this.workflows = res.data.GetAllWorkFLowResult
        this.loadingData = false
      } catch (e) {
        console.log(e)
      }
    },

    editItem(item) {
      this.editedIndex = this.workflows.indexOf(item)
      this.editedItem = Object.assign({}, item)
      this.dialogWorkflow = true
    },

    deleteItem(item) {
      const index = this.workflows.indexOf(item)
      console.log(item.WORKFLOW_ID)
      if (confirm('Etes-vous sûr de supprimer ce workflow ?') === true) {
        axios
          .delete('/workflow/' + item.WORKFLOW_ID)
          .then(function(response) {
            console.log(response)
            this.workflows.splice(index, 1)
          })
          .catch(function(error) {
            console.log(error)
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
        // Object.assign(this.workflows[this.editedIndex], this.editedItem)
        axios
          .put('/workflow/update', {
            WORKFLOW_LABEL: this.editedItem.WORKFLOW_LABEL
          })
          .then(function(response) {
            console.log(response)
            Object.assign(this.workflows[this.editedIndex], this.editedItem)
          })
          .catch(function(error) {
            console.log(error)
          })
      } else {
        console.log('ajout', this.editedItem.WORKFLOW_LABEL)
        if (this.editedItem.WORKFLOW_LABEL !== '') {
          axios
            .post('/workflow/insert', {
              WORKFLOW_LABEL: this.editedItem.WORKFLOW_LABEL
            })
            .then(function(response) {
              console.log(response)
              this.workflows.push(this.editedItem)
            })
            .catch(function(error) {
              console.log(error)
            })
        }
      }
      this.close()
    },

    async showDetail(item) {
      this.dialogDetailWorkflow = true
      this.workflowDate = item.WORKFLOW_LABEL
      this.showAlertNoProcessus = false
      this.WorkflowProcesses = []
      try {
        const res = await axios.get(
          'http://localhost:63267/Service1.svc/workflow/' +
            item.WORKFLOW_ID +
            '/processus'
        )
        this.loadingWorkflowProcesses = false
        this.WorkflowProcesses = res.data.GetWorkflowProcessesResult
        this.WorkflowProcesses.length === 0
          ? (this.showAlertNoProcessus = true)
          : (this.showAlertNoProcessus = false)
      } catch (e) {
        console.log(e)
      }
    },

    affectItem(item) {
      if (this.selectedProcessus !== undefined) {
        if (this.selectedProcessus.length !== 0) {
          item.processus = []
          this.selectedProcessus.forEach((element) => {
            item.processus.push(element)
          })
          this.snackbar = true
          this.colorsnackbar = 'success'
          this.snackbarMessage = 'Affectation effectué avec succès'
        } else {
          this.snackbar = true
          this.colorsnackbar = 'error'
          this.snackbarMessage =
            'Il est nécessaire de selectionner au minimum un processus pour pouvoir les affecter à un workflow'
        }
      }
    }
  }
}
</script>
