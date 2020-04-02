<template>
  <v-layout>
    <v-container class="grey lighten-5">
      <v-list-item>
        <v-list-item-content>
          <v-list-item-title class="title">
            Paramétrage des workflows
          </v-list-item-title>
          <v-list-item-subtitle>
            subtext
          </v-list-item-subtitle>
        </v-list-item-content>
      </v-list-item>
      <v-divider class="my-2 mx-4" inset></v-divider>
      <v-row>
        <v-col cols="12" sm="6">
          <v-data-table
            :headers="headersWorkflow"
            :items="workflows"
            sort-by="calories"
            :page.sync="page"
            :items-per-page="itemsPerPage"
            hide-default-footer
            class="elevation-1"
            show-select
            single-select
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
                              :rules="nameRules"
                              label="Nom du workflow"
                              required
                            ></v-text-field>
                          </v-col>
                        </v-row>
                      </v-container>
                    </v-card-text>

                    <v-card-actions>
                      <v-spacer></v-spacer>
                      <v-btn color="blue darken-1" text @click="close"
                        >Cancel</v-btn
                      >
                      <v-btn color="blue darken-1" text @click="save(workflow)"
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
                  <v-icon
                    small
                    class="mr-2"
                    v-on="on"
                    @click="editItem(item, 'Workflow')"
                  >
                    mdi-pencil
                  </v-icon>
                </template>
                <span>Modifier</span>
              </v-tooltip>
              <v-tooltip top>
                <template v-slot:activator="{ on }">
                  <v-icon
                    small
                    class="mr-2"
                    v-on="on"
                    @click="deleteItem(item)"
                  >
                    mdi-delete
                  </v-icon>
                </template>
                <span>Supprimer</span>
              </v-tooltip>
              <v-tooltip top>
                <template v-slot:activator="{ on }">
                  <v-icon
                    small
                    class="mr-2"
                    v-on="on"
                    @click="affectItem(item)"
                  >
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
        </v-col>
        <v-col cols="12" sm="6">
          <v-data-table
            v-model="selectedProcessus"
            :headers="headersProcessus"
            :items="processus"
            :page.sync="pageProcessus"
            :items-per-page="itemsPerPageProcessus"
            hide-default-footer
            class="elevation-1"
            show-select
            @page-count="pageCountProcessus = $event"
            @item-selected="test($event)"
          >
            <template v-slot:top>
              <v-toolbar flat color="white">
                <v-toolbar-title>Mes processus</v-toolbar-title>
                <v-divider class="mx-4" inset vertical></v-divider>
                <v-spacer></v-spacer>
                <v-dialog v-model="dialogProcessus" max-width="500px">
                  <template v-slot:activator="{ on }">
                    <v-btn color="primary" dark class="mb-2" v-on="on"
                      >Créer un processus</v-btn
                    >
                  </template>
                  <v-card>
                    <v-card-title>
                      <span class="headline">{{ formTitle }}</span>
                      <v-spacer></v-spacer>
                      <v-icon>mdi-pencil</v-icon>
                    </v-card-title>
                    <v-divider></v-divider>
                    <v-card-text>
                      <v-container>
                        <v-row>
                          <v-col cols="12" sm="6" md="12">
                            <v-text-field
                              v-model="editedItem.name"
                              label="Nom du processus"
                              required
                            ></v-text-field>
                          </v-col>
                        </v-row>
                      </v-container>
                    </v-card-text>

                    <v-card-actions>
                      <v-spacer></v-spacer>
                      <v-btn color="blue darken-1" text @click="close"
                        >Cancel</v-btn
                      >
                      <v-btn color="blue darken-1" text @click="save(processus)"
                        >Save</v-btn
                      >
                    </v-card-actions>
                  </v-card>
                </v-dialog>
              </v-toolbar>
            </template>
            <template v-slot:item.actions="{ item }">
              <v-icon small class="mr-2" @click="editItem(item, 'Processus')">
                mdi-pencil
              </v-icon>
              <v-icon small @click="deleteItem(item)">
                mdi-delete
              </v-icon>
            </template>
            <template v-slot:no-data>
              <v-btn color="primary" @click="initialize">Reset</v-btn>
            </template>
          </v-data-table>
          <div class="text-center pt-2">
            <v-pagination
              v-model="pageProcessus"
              :length="pageCountProcessus"
            ></v-pagination>
          </div>
        </v-col>
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
                  <tr
                    v-for="process in processusByWorkflow"
                    :key="process.name"
                  >
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
      </v-row>
      <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
        {{ snackbarMessage }}
        <v-btn dark text @click="snackbar = false">
          Close
        </v-btn>
      </v-snackbar>
    </v-container>
  </v-layout>
</template>

<script>
import ProcessData from '../data/Processus.json'
import WorkflowData from '../data/Workflow.json'
export default {
  data: () => ({
    dialogWorkflow: false,
    dialogProcessus: false,
    dialogDetailWorkflow: false,
    selectedProcessus: [],
    itemselect: '',
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
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
    headersProcessus: [
      {
        text: 'Nom',
        align: 'start',
        sortable: false,
        value: 'name'
      },
      { text: "Ordre d'éxecution", value: 'order' },
      { text: 'Actions', value: 'actions', sortable: false }
    ],
    workflows: [],
    processus: [],
    processusByWorkflow: [],
    editedIndex: -1,
    showAlertNoProcessus: false,
    editedItem: {
      name: '',
      calories: 0,
      fat: 0,
      carbs: 0,
      protein: 0
    },
    defaultItem: {
      name: '',
      calories: 0,
      fat: 0,
      carbs: 0,
      protein: 0
    },
    page: 1,
    pageCount: 0,
    pageProcessus: 1,
    pageCountProcessus: 0,
    itemsPerPage: 10,
    itemsPerPageProcessus: 10,
    nameRules: [
      (v) => !!v || 'Name is required',
      (v) => (v && v.length <= 10) || 'Name must be less than 10 characters'
    ],
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
    initialize() {
      this.processus = ProcessData
      this.workflows = WorkflowData
    },

    editItem(item, val) {
      this.editedIndex = this.processus.indexOf(item)
      this.editedItem = Object.assign({}, item)
      if (val === 'Workflow') {
        this.dialogWorkflow = true
      } else {
        this.dialogProcessus = true
      }
    },

    deleteItem(item) {
      const index = this.processus.indexOf(item)
      confirm('Are you sure you want to delete this item?') &&
        this.processus.splice(index, 1)
    },

    close() {
      this.dialogWorkflow = false
      setTimeout(() => {
        this.editedItem = Object.assign({}, this.defaultItem)
        this.editedIndex = -1
      }, 300)
    },

    save(val) {
      console.log(val)
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
      if (item.processus.length > 0) {
        for (let i = 0; i < item.processus.length; i++) {
          const details = {
            name: item.processus[i].name,
            order: item.processus[i].order
          }
          this.processusByWorkflow.push(details)
        }
      } else {
        this.showAlertNoProcessus = true
      }
    },

    affectItem(item) {
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
          'Il est nécessaire de selectionné au minimum un processus pour pouvoir en affecter à un workflow'
      }
    },

    test(event) {
      console.log(event)
    }
  }
}
</script>
