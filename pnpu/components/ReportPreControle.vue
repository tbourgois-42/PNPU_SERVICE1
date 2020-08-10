<template>
  <v-layout>
    <v-row>
      <v-col cols="12" class="pt-0 mt-0 d-flex justify-space-between">
        <v-card flat class="mr-auto">
          <v-card-title class="pt-0 mt-0"
            >Rapport d'execution du processus
          </v-card-title>
          <v-card-subtitle class="pb-0">
            Pré-Contrôle .mdb
          </v-card-subtitle> </v-card
        ><v-btn
          v-if="currentID_STATUT === 'mdi-hand'"
          depressed
          class="mr-4 mt-2 pr-4"
          color="error"
          @click="stopWorkflow()"
          ><v-icon left>mdi-hand</v-icon> Stopper le workflow
        </v-btn>
        <v-btn
          v-if="currentID_STATUT === 'mdi-hand'"
          depressed
          class="mr-4 mt-2 pr-4"
          color="warning"
          @click="continueWorkflow()"
          ><v-icon left>mdi-hand</v-icon> Valider le processus
        </v-btn>
        <v-btn depressed class="mr-4 mt-2 pr-4" color="primary">
          <v-icon left>mdi-file-excel-outline</v-icon> Exporter
        </v-btn>
      </v-col>
      <v-col cols="12">
        <v-divider class="mx-4 mb-4"></v-divider>
      </v-col>
      <v-col cols="4">
        <v-card class="mx-auto" max-width="500">
          <v-sheet class="pa-4 primary">
            <v-text-field
              v-model="searchTreeview"
              append-icon="mdi-magnify"
              label="Chercher un élément"
              dark
              flat
              solo-inverted
              hide-details
              clearable
              clear-icon="mdi-close-circle-outline"
            ></v-text-field>
          </v-sheet>
          <v-card-text>
            <v-treeview
              v-model="selection"
              :items="treeviewFiltered"
              :search="searchTreeview"
              :filter="filter"
              hoverable
              returnObject
              transition
              activatable
              open-on-click
              @update:active="getSelected($event)"
              @update:open="getSelected($event)"
            >
              <template v-slot:prepend="{ item, open }">
                <v-icon v-if="!item.result">
                  {{ open ? 'mdi-folder-open' : 'mdi-folder' }}
                </v-icon>
                <v-icon v-else>
                  mdi-folder
                </v-icon>
              </template>
              <template v-slot:append="{ item }">
                <v-icon
                  v-if="item.result === 'mdi-check-circle'"
                  color="success"
                  >{{ item.result }}</v-icon
                >
                <v-icon
                  v-if="item.result === 'mdi-alert'"
                  color="yellow darken-2"
                  >{{ item.result }}</v-icon
                >
                <v-icon
                  v-if="item.result === 'mdi-alert-circle'"
                  color="error"
                  >{{ item.result }}</v-icon
                >
              </template>
            </v-treeview>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="8">
        <v-list-item-group class="mb-0 d-flex">
          <v-list-item-icon>
            <v-icon>mdi-folder</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title class="subtitle-1 mb-0">
              {{ titleTable }}</v-list-item-title
            >
          </v-list-item-content>
          <v-checkbox
            v-model="checkbox"
            label="Voir uniquement les contrôles en erreur / warning"
            hide-details
            @change="Filtered($event)"
          ></v-checkbox>
        </v-list-item-group>
        <transition appear name="fade">
          <v-data-table
            :headers="headers"
            :items="tableFiltered"
            :search="searchDataTable"
            :hide-default-footer="hideFooterDataTable(tableFiltered)"
            :items-per-page="itemsPerPage"
            multi-sort
            class="elevation-1 mt-4 mr-2"
            v-if="selectedItemTable.length > 0"
          >
            <template v-slot:top>
              <v-text-field v-model="searchDataTable" label="Chercher un élément" class="mx-4" append-icon="mdi-magnify"></v-text-field>
            </template>
            <template v-slot:item.result="{ item }">
              <v-tooltip bottom>
                <template v-slot:activator="{ on, attrs }">
                  <v-icon v-bind="attrs" v-on="on" :color=getgetColorIconResult(item.result)>{{ item.result }}</v-icon>
                </template>
                <span>{{ tooltipMessage }}</span>
              </v-tooltip>
            </template>
          </v-data-table>
        </transition>
        <v-alert
          v-if="selectedItemTable.length === 0"
          icon="mdi-check"
          prominent
          text
          type="success"
        >
          Ce contrôle s'est déroulé avec succès, il n'a généré aucun message
          d'erreur.
        </v-alert>
        <v-alert
          v-if="showInfo === true"
          icon="mdi-information-variant"
          prominent
          text
          type="primary"
        >
          Pour visualiser les résultats de ce contrôle, veuillez cliquer sur
          {{ titleTable }} dans l'arborescence.
        </v-alert>
      </v-col>

      <v-col cols="12">
        <v-alert
          v-if="currentID_STATUT === 'mdi-hand'"
          icon="mdi-information-outline"
          text
          color="warning"
          >Ce processus demande l'intervention d'un utilisateur pour pouvoir
          continuer ou non le workflow. Pour plus d'information veuillez
          consulter le rapport d'éxecution ci-dessous</v-alert
        >
        <v-alert
          v-if="currentID_STATUT === 'mdi-alert'"
          icon="mdi-information-outline"
          text
          color="error"
          >Le processus a remonté des erreurs qui ont entrainés l'arrêt du
          Workflow. Pour plus d'information veuillez consulter le rapport
          d'éxecution ci-dessous</v-alert
        >
      </v-col>
    </v-row>
  </v-layout>
</template>

<script>
import axios from 'axios'
export default {
  props: {
    idPROCESS: {
      type: Number,
      default: 1
    },
    reportJsonData: {
      type: Array,
      default: () => []
    },
    idInstanceWF: {
      type: String,
      default: ''
    },
    workflowID: {
      type: String,
      default: ''
    },
    currentID_STATUT: {
      type: String,
      default: ''
    }
  },
  data: () => ({
    e1: 1,
    steps: [],
    clientId: '',
    clientName: '',
    etape: '',
    workflowDate: '',
    workflowID: '',
    textStatus: '',
    currentID_STATUT: '',
    currentID_PROCESS: '',
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
    searchTreeview: null,
    caseSensitive: false,
    open: ['public'],
    selection: [],
    active: [],
    selectedItemTable: [],
    titleTable: '',
    csvFile: [],
    csvFileHeader: [],
    checkbox: false,
    tableFiltered: [],
    checkboxValue: false,
    page: 1,
    pageCount: 0,
    itemsPerPage: 15,
    showInfo: false,
    loadingReport: false,
    reportTNR: false,
    alertMessage: '',
    alertIcon: 'mdi-information-outline',
    idInstanceWF: '',
    reportLivraison: false,
    clientTaskName: '',
    undefinedIcon: false,
    headers: [
      { text: 'Nom', value: 'name' },
      { text: 'Status', value: 'result' }
    ],
    searchDataTable: '',
    treeviewFiltered: []
  }),

  computed: {
    /**
     * Filtre l'arborescence avec saisi de texte.
     */
    filter() {
      return this.caseSensitive
        ? (item, searchTreeview, textKey) =>
            item[textKey].includes(searchTreeview) > -1
        : undefined
    }
  },

  created() {
    this.titleTable = this.reportJsonData[0].name
    this.selectedItemTable = this.reportJsonData[0].children
  },

  watch: {
    /**
     * Step sélectionné dans le stepper.
     * @param {number} val - Valeur du step.
     */
    steps(val) {
      if (this.e1 > val) {
        this.e1 = val
      }
    }
  },

  methods: {
    /**
     * Generate color icon according to icon word
     */
    getgetColorIconResult(icon) {
      if (icon === 'mdi-alert-circle') {
        return 'error'
      }
      if (icon === 'mdi-alert') {
        return 'warning'
      }
      if (icon === 'mdi-check-circle') {
        return 'success'
      }
    },

    /**
     * Hide data table footer
     * @param {array} - Treeview items selected 
     */
    hideFooterDataTable(items) {
      return items.length < this.itemsPerPage ? true : false
    },

    /**
     * Elément sélectionné dans l'arborescence.
     * @param {object} e - $event.
     */
    getSelected(e) {
      if (e.length > 0) {
        for (const selectedItem of e) {
          if (selectedItem.children !== undefined) {
            this.GenerateTableValues(selectedItem.children, selectedItem.name)
          } else if (selectedItem.elements !== undefined) {
            this.GenerateTableValues(selectedItem.elements, selectedItem.name)
            this.headers = [
              { text: 'Object Type', value: 'objectType' },
              { text: 'Object ID', value: 'objectID' }
            ]
          } else if (selectedItem.message !== undefined) {
            this.GenerateTableValues(selectedItem.message, selectedItem.name)
          } else {
            this.selectedItemTable = []
          }
        }
      }
      this.showInfo = false
      this.Filtered(this.checkboxValue)
    },

    /**
     * Generate table values according to treeview items selected
     * @param {array} items - Treeview items selected 
     * @param {string} tableName - Name shown above table
     */
    GenerateTableValues(items, tableName) {
      const vm = this
      vm.titleTable = tableName
      if (items !== undefined) {
        this.selectedItemTable = items
        items.forEach((children) => {
          if (children.result === undefined) {
            this.headers = [
              { text: 'Nom', value: 'name' }
            ]
          } else {
            this.headers = [
              { text: 'Nom', value: 'name' },
              { text: 'Status', value: 'result' }
            ]
          }
        })
      }
    },

    /**
     * Filtre l'affichage pour n'avoir que les contrôles KO.
     * @param {bool} checkboxValue - Valeur de la checkbox.
     */
    Filtered(checkboxValue) {
      this.checkboxValue = checkboxValue
      this.FilterTreeview(checkboxValue)
      if (checkboxValue === true) {
        this.tableFiltered = []
        if (this.selectedItemTable !== undefined) {
          this.selectedItemTable.forEach((element) => {
            if (element.result !== 'mdi-check-circle') {
              this.tableFiltered.push(element)
            }
          })
        }
      } else {
        this.tableFiltered = this.selectedItemTable
      }
    },

    /**
     * Filter treeview
     * @param {bool} checkboxValue
     */
    FilterTreeview(checkboxValue) {
      if (checkboxValue) {
        // Remove all if global control is OK
        if (this.treeviewFiltered[0].result === 'mdi-check-circle') {
          this.treeviewFiltered.splice(0, 1)
        } else {
          // Remove "Controle des dépendances du livrable" level
          this.removeElements(this.treeviewFiltered[0].children)
          // Remove all check control
          this.removeElements(this.treeviewFiltered[0].children[0].children)
        }
      } else {
        // Deep Copy of this.items
        this.treeviewFiltered = JSON.parse(JSON.stringify(this.reportJsonData))
      }
    },

    /**
     * Remove elements from array
     * @param {array} array - Array to check for remove elements
     */
    removeElements(array) {
      for (let index = 0; index < array.length; index++) {
        if (array[index].result === 'mdi-check-circle') {
          array.splice(index, 1)
          index --
        }
      }
    },

    /**
     * Lance l'arrêt du workflow.
     */
    stopWorkflow() {
      if (
        confirm('Etes-vous sûr de bien vouloir stopper le workflow ?') === true
      ) {
        const vm = this
        axios
          .post(`${process.env.WEB_SERVICE_WCF}/Workflow/Client/Stop`, {
            WORKFLOW_ID: this.workflowID,
            CLIENT_ID: this.clientId
          })
          .then(function(response) {
            if (response.status !== 200) {
              vm.showSnackbar(
                'error',
                `Modification impossible - HTTP error ${response.status} !`
              )
            } else {
              vm.showSnackbar(
                'success',
                'Le workflow a été stoppé avec succès !'
              )
            }
          })
          .catch(function(error) {
            vm.showSnackbar('error', `${error} !`)
          })
      }
    },

    /**
     * On continue le workflow si on est sur une étape nécessitant une action manuelle.
     */
    continueWorkflow() {
      if (
        confirm(
          'Etes-vous sûr de bien vouloir valider manuellement le processus ?'
        ) === true
      ) {
        const vm = this
        axios
          .post(`${process.env.WEB_SERVICE_WCF}/Workflow/Client/Continue`, {
            WORKFLOW_ID: this.workflowID,
            CLIENT_ID: this.clientId
          })
          .then(function(response) {
            if (response.status !== 200) {
              vm.showSnackbar(
                'error',
                `Modification impossible - HTTP error ${response.status} !`
              )
            } else {
              vm.showSnackbar(
                'success',
                'Le processus a été manuellement validé avec succès !'
              )
            }
          })
          .catch(function(error) {
            vm.showSnackbar('error', `${error} !`)
          })
      }
    },

    /**
     * Set no data
     *
     */
    setNoData() {
      this.alertMessage =
        "Ce processus n'est pas terminé. Aucun rapport n'est disponible pour le moment"
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

<style lang="css">
.v-treeview-node__root {
  cursor: pointer !important;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s;
}
.fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
  opacity: 0;
}
</style>
