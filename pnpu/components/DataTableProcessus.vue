<template>
  <v-col>
    <v-data-table
      :headers="headersProcessus"
      :items="processus"
      :page.sync="pageProcessus"
      :items-per-page="itemsPerPageProcessus"
      hide-default-footer
      class="elevation-1"
      show-select
      @page-count="pageCountProcessus = $event"
      @input="$emit('getValue', $event)"
      :loading="loadingData"
      loading-text="Chargement des processus"
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
                        v-model="editedItem.PROCESS_LABEL"
                        label="Nom du processus"
                        required
                      ></v-text-field>
                      <v-text-field
                        v-model="editedItem.ID_PROCESS"
                        label="Ordre d'éxecution"
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
        <v-icon small class="mr-2" @click="editItem(item)">
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
</template>

<script>
import axios from 'axios'
export default {
  data: () => ({
    headersProcessus: [
      {
        text: 'Nom',
        align: 'start',
        sortable: false,
        value: 'PROCESS_LABEL'
      },
      { text: "Ordre d'éxecution", value: 'ID_PROCESS' },
      { text: 'Actions', value: 'actions', sortable: false }
    ],
    processus: [],
    pageProcessus: 1,
    itemsPerPageProcessus: 10,
    pageCountProcessus: 0,
    dialogProcessus: false,
    snackbar: false,
    colorsnackbar: '',
    snackbarMessage: '',
    editedIndex: -1,
    editedItem: {
      name: '',
      order: 0
    },
    defaultItem: {
      name: '',
      order: 0
    },
    loadingData: true
  }),

  created() {
    this.initialize()
  },

  watch: {
    dialogProcessus(val) {
      val || this.close()
    }
  },

  methods: {
    async initialize() {
      try {
        const res = await axios.get(
          'http://localhost:63267/Service1.svc/process'
        )
        this.processus = res.data.GetAllProcessesResult
        this.loadingData = false
      } catch (e) {
        console.log(e)
      }
    },

    editItem(item) {
      this.editedIndex = this.processus.indexOf(item)
      this.editedItem = Object.assign({}, item)
      this.dialogProcessus = true
    },

    deleteItem(item) {
      const index = this.processus.indexOf(item)
      if (confirm('Etes-vous sûr de supprimer ce processus ?') === true) {
        axios
          .delete('/process/' + item.ID_PROCESS)
          .then(function(response) {
            console.log(response)
            this.processus.splice(index, 1)
          })
          .catch(function(error) {
            console.log(error)
          })
      }
    },

    close() {
      this.dialogProcessus = false
      setTimeout(() => {
        this.editedItem = Object.assign({}, this.defaultItem)
        this.editedIndex = -1
      }, 300)
    },

    save() {
      if (this.editedIndex > -1) {
        axios
          .put('/processus/update', {
            PROCESS_LABEL: this.editedItem.PROCESS_LABEL
          })
          .then(function(response) {
            Object.assign(this.processus[this.editedIndex], this.editedItem)
          })
          .catch(function(error) {
            console.log(error)
          })
      } else {
        console.log('ajout', this.editedItem.WORKFLOW_LABEL)
        if (this.editedItem.PROCESS_LABEL !== '') {
          axios
            .post('/processus/insert', {
              PROCESS_LABEL: this.editedItem.PROCESS_LABEL
            })
            .then(function(response) {
              this.processus.push(this.editedItem)
            })
            .catch(function(error) {
              console.log(error)
            })
        }
      }
      this.close()
    }
  },

  computed: {
    formTitle() {
      return this.editedIndex === -1
        ? "Ajout d'un processus"
        : "Edition d'un processus"
    }
  }
}
</script>
