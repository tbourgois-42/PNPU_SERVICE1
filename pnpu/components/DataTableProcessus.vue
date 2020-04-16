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
      :loading="loadingData"
      loading-text="Chargement des processus"
      @page-count="pageCountProcessus = $event"
      @input="$emit('getValue', $event)"
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
                      <v-form ref="form" v-model="valid" lazy-validation>
                        <v-text-field
                          v-model="editedItem.PROCESS_LABEL"
                          label="Nom du processus"
                          :rules="[
                            (v) => !!v || 'Le nom du processus est obligatoire'
                          ]"
                          required
                        ></v-text-field>
                        <v-select
                          v-model="editedItem.IS_LOOPABLE"
                          :items="loopableItems"
                          :rules="[
                            (v) => !!v || 'Le champ réitération est obligatoire'
                          ]"
                          label="Réitération"
                          required
                        ></v-select>
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
  data: () => ({
    headersProcessus: [
      {
        text: 'Nom',
        align: 'start',
        sortable: false,
        value: 'PROCESS_LABEL'
      },
      { text: 'Identifiant', value: 'ID_PROCESS', sortable: true },
      { text: 'Réitération', value: 'IS_LOOPABLE', sortable: true },
      { text: 'Actions', value: 'actions', sortable: false }
    ],
    processus: [],
    loopableItems: ['0', '1'],
    pageProcessus: 1,
    itemsPerPageProcessus: 10,
    pageCountProcessus: 0,
    dialogProcessus: false,
    snackbar: false,
    colorsnackbar: '',
    snackbarMessage: '',
    editedIndex: -1,
    editedItem: {
      PROCESS_LABEL: '',
      ID_PROCESS: '',
      IS_LOOPABLE: ''
    },
    defaultItem: {
      PROCESS_LABEL: '',
      ID_PROCESS: '',
      IS_LOOPABLE: ''
    },
    loadingData: true,
    valid: true
  }),

  computed: {
    formTitle() {
      return this.editedIndex === -1
        ? "Ajout d'un processus"
        : "Edition d'un processus"
    }
  },

  watch: {
    dialogProcessus(val) {
      val || this.close()
    }
  },

  created() {
    this.initialize()
  },

  methods: {
    async initialize() {
      const vm = this
      try {
        const res = await axios.get(`${process.env.WEB_SERVICE_WCF}/process`)
        this.processus = res.data.GetAllProcessesResult
        this.loadingData = false
      } catch (error) {
        vm.showSnackbar('error', `${error} !`)
      }
    },

    editItem(item) {
      this.editedIndex = this.processus.indexOf(item)
      this.editedItem = Object.assign({}, item)
      this.dialogProcessus = true
    },

    deleteItem(item) {
      const index = this.processus.indexOf(item)
      const vm = this
      if (confirm('Etes-vous sûr de supprimer ce processus ?') === true) {
        axios
          .delete(
            `${process.env.WEB_SERVICE_WCF}/process/` +
              item.ID_PROCESS +
              `/Delete`
          )
          .then(function(response) {
            if (response.status !== 200) {
              vm.showSnackbar(
                'error',
                `Modification impossible - ${response.status} !`
              )
            } else {
              vm.processus.splice(index, 1)
              vm.showSnackbar(
                'success',
                'Suppression du processus effectuée avec succès !'
              )
            }
          })
          .catch(function(error) {
            vm.showSnackbar('error', `${error} !`)
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
        const vm = this
        axios
          .put(
            `${process.env.WEB_SERVICE_WCF}/process/` +
              this.editedItem.ID_PROCESS,
            {
              PROCESS_LABEL: this.editedItem.PROCESS_LABEL,
              ID_PROCESS: this.editedItem.ID_PROCESS,
              IS_LOOPABLE: this.editedItem.IS_LOOPABLE
            }
          )
          .then(function(response) {
            Object.assign(vm.processus[vm.editedIndex], vm.editedItem)
            vm.showSnackbar(
              'success',
              'Modification du processus effectuée avec succès !'
            )
          })
          .catch(function(error) {
            vm.showSnackbar('error', `${error} !`)
          })
      } else if (this.editedItem.PROCESS_LABEL !== '') {
        const vm = this
        axios
          .post(`${process.env.WEB_SERVICE_WCF}/process/CreateProcess/`, {
            PROCESS_LABEL: this.editedItem.PROCESS_LABEL,
            IS_LOOPABLE: this.editedItem.IS_LOOPABLE
          })
          .then((response) => {
            debugger
            console.log(response.config.data[0].PROCESS_LABEL)
            this.processus.push({
              PROCESS_LABEL: this.editedItem.PROCESS_LABEL,
              ID_PROCESS: response.data,
              IS_LOOPABLE: this.editedItem.IS_LOOPABLE
            })
            vm.showSnackbar(
              'success',
              'Création du processus effectuée avec succès !'
            )
          })
          .catch((error) => {
            vm.showSnackbar('error', `${error} !`)
          })
      }
      this.close()
    },

    showSnackbar(color, message) {
      this.snackbar = true
      this.colorsnackbar = color
      this.snackbarMessage = message
    }
  }
}
</script>
