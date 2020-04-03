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
                        v-model="editedItem.name"
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
        <v-simple-table class="ma-2">
          <template v-slot:default>
            <thead>
              <tr v-if="showAlertNoProcessus === false">
                <th class="text-left">Nom du processus</th>
                <th class="text-left">Ordre d'éxecution</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="process in processusByWorkflow" :key="process.name">
                <td>{{ process.name }}</td>
                <td>{{ process.order }}</td>
              </tr>
            </tbody>
          </template>
        </v-simple-table>
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
import WorkflowData from '../data/Workflow.json'
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
    processusByWorkflow: [],
    headersWorkflow: [
      {
        text: 'Nom',
        align: 'start',
        sortable: false,
        value: 'name'
      },
      { text: 'Actions', value: 'actions', sortable: false },
      { text: 'Modifié le', value: 'updated_at', sortable: false }
    ],
    workflows: [],
    page: 1,
    itemsPerPage: 10,
    pageCount: 0,
    editedItem: {
      name: '',
      actions: 0,
      updated_at: 0
    },
    defaultItem: {
      name: '',
      actions: 0,
      updated_at: 0
    },
    workflowDate: ''
  }),

  computed: {
    formTitle() {
      return this.editedIndex === -1
        ? "Ajout d'un workflow"
        : "Edition d'un workflow"
    }
  },

  watch: {
    dialog(val) {
      val || this.close()
    }
  },

  created() {
    this.initialize()
  },

  methods: {
    async initialize() {
      try {
        const res = await axios.get('../data/Workflows.json')
        this.workflows = res
      } catch (e) {
        console.log(e)
      }
      this.workflows = WorkflowData
    },

    editItem(item) {
      this.editedIndex = this.workflows.indexOf(item)
      this.editedItem = Object.assign({}, item)
      this.dialogWorkflow = true
    },

    deleteItem(item) {
      const index = this.workflows.indexOf(item)
      confirm('Are you sure you want to delete this item?') &&
        this.workflows.splice(index, 1)
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
        Object.assign(this.workflows[this.editedIndex], this.editedItem)
      } else {
        this.workflows.push(this.editedItem)
      }
      this.close()
    },

    showDetail(item) {
      this.dialogDetailWorkflow = true
      this.workflowDate = item.name
      this.showAlertNoProcessus = false
      this.processusByWorkflow = []
      if (item.processus !== undefined) {
        if (item.processus.length > 0) {
          for (let i = 0; i < item.processus.length; i++) {
            const details = {
              name: item.processus[i].name,
              order: item.processus[i].order
            }
            this.processusByWorkflow.push(details)
          }
        }
      } else {
        this.showAlertNoProcessus = true
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
