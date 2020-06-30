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
      <v-col :cols="nbColsLeft" :style="displayNoneLeft">
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
              :items="JSON_TEMPLATE"
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
      <v-col :cols="nbColsRight">
        <v-list-item-group class="mb-0 d-flex">
          <v-list-item-icon>
            <v-icon>mdi-folder</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title class="subtitle-1 mb-0">
              {{ titleTable }}</v-list-item-title
            >
          </v-list-item-content>
          <v-tooltip top>
            <template v-slot:activator="{ on }">
              <v-btn
                x-small
                fab
                depressed
                color="primary"
                class="ma-4"
                :style="displayButton"
                v-on="on"
                @click="backToTreeView($event)"
              >
                <v-icon>mdi-undo</v-icon>
              </v-btn>
            </template>
            <span>Retourner à l'arborescence</span>
          </v-tooltip>
          <v-checkbox
            v-model="checkbox"
            label="Voir uniquement les contrôles en erreur"
            hide-details
            :style="displayCheckbox"
            @change="Filtered($event)"
          ></v-checkbox>
        </v-list-item-group>
        <transition v-if="noData === false" appear name="fade">
          <v-card v-if="titleTable !== 'Contrôle des dépendances du livrable'">
            <v-simple-table>
              <template v-slot:default>
                <thead>
                  <tr>
                    <th class="text-left">Nom</th>
                    <th v-if="hasMessage === false" class="text-left">
                      Statut
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(item, idxTableFiltered) in tableFiltered"
                    :key="idxTableFiltered"
                  >
                    <v-tooltip
                      v-if="item.Tooltip !== undefined"
                      top
                      color="primary"
                    >
                      <template v-slot:activator="{ on }">
                        <td v-on="on">{{ item.name }}</td>
                      </template>
                      <span class="mt-6">
                        <v-icon dark class="mr-4">
                          mdi-alert-circle
                        </v-icon>
                        {{ item.Tooltip }}
                      </span>
                    </v-tooltip>
                    <td v-else>{{ item.name }}</td>
                    <td v-if="hasMessage === false">
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
                    </td>
                  </tr>
                </tbody>
              </template>
            </v-simple-table>
          </v-card>
          <v-card v-else flat :style="displayButton">
            <v-card-title>
              <v-text-field
                v-model="searchInterDep"
                append-icon="mdi-magnify"
                label="Chercher un résultat ..."
                single-line
                hide-details
              ></v-text-field>
            </v-card-title>
            <v-data-table
              :headers="headers"
              :items="csvFile"
              :search="searchInterDep"
              :page.sync="page"
              :items-per-page="itemsPerPage"
              hide-default-footer
              multi-sort
              @page-count="pageCount = $event"
            ></v-data-table>
            <div class="text-center pa-2">
              <v-pagination
                v-model="page"
                :length="pageCount"
                circle
              ></v-pagination>
            </div>
          </v-card>
        </transition>
        <v-alert
          v-if="noData === true"
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
      <v-alert
        v-if="noData === true"
        :icon="iconValid"
        prominent
        text
        type="success"
      >
        Ce process s'est déroulé avec succés, vous pouvez consultez le rapport
        en visualisant l'étape concernée dans le Stepper d'erreur.
      </v-alert>
      <v-alert
        v-if="processInError === true"
        :icon="iconAlert"
        prominent
        text
        type="error"
      >
        Ce process s'est terminé avec des erreurs, vous pouvez consultez le
        rapport en visualisant l'étape concernée dans le Stepper
      </v-alert>
      <v-alert
        v-if="processInWarning === true"
        :icon="iconWarning"
        prominent
        text
        type="warning"
      >
        Ce process s'est déroulé avec des warning, vous pouvez consultez le
        rapport en visualisant l'étape concernée dans le Stepper
      </v-alert>
      <v-alert
        v-if="undefinedIcon === true"
        :icon="iconAlert"
        prominent
        text
        type="error"
      >
        Résultat du contrôle inconnu
      </v-alert>
    </v-row>
  </v-layout>
</template>

<script>
import Papa from 'papaparse'
import axios from 'axios'
export default {
  props: {
    idPROCESS: {
      type: String,
      default: '1'
    },
    reportJsonData: {
      type: Object,
      default: () => {}
    },
    idInstanceWF: {
      type: String,
      default: ''
    },
    workflowID: {
      type: String,
      default: ''
    },
    nbAvailablePack: {
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
    JSON_TEMPLATE: {},
    headers: [
      {
        text: 'Contrôle',
        align: 'start',
        sortable: false,
        value: 'id'
      },
      { text: 'Debut', value: 'debut' },
      { text: 'Fin', value: 'fin' },
      { text: 'Source', value: 'source' },
      { text: 'Id', value: 'id' },
      { text: 'Contrôle', value: 'controle' },
      { text: 'Id', value: 'id' },
      { text: 'Result', value: 'result' },
      { text: 'Message', value: 'message' }
    ],
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
    noData: false,
    hasMessage: false,
    searchInterDep: '',
    page: 1,
    pageCount: 0,
    itemsPerPage: 10,
    nbColsRight: 8,
    nbColsLeft: 4,
    displayNoneLeft: '',
    displayButton: 'display:none',
    tableCtrlDepInterPack: [],
    showInfo: false,
    displayCheckbox: '',
    loadingReport: false,
    reportTNR: false,
    alertMessage: '',
    alertIcon: 'mdi-information-outline',
    idInstanceWF: '',
    nbAvailablePack: 0,
    reportLivraison: false,
    clientTaskName: ''
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

  mounted() {
    this.parseCSVFile()
  },

  methods: {
    /**
     * Elément sélectionné dans l'arborescence.
     * @param {object} e - $event.
     */
    getSelected(e) {
      if (e.length > 0) {
        for (const selectedItem of e) {
          if (selectedItem.children !== undefined) {
            this.selectedItemTable = selectedItem.children
            this.hasMessage = false
          } else {
            this.selectedItemTable = selectedItem.message
            this.hasMessage = true
          }
          this.titleTable = selectedItem.name
          this.noData = false
          // On change l'affichage si on est sur le Contrôle des dépendances inter packages (CSV)
          if (this.titleTable === 'Contrôle des dépendances inter packages') {
            this.displayNoneLeft = 'display:none'
            this.nbColsRight = 12
            this.displayButton = ''
            this.displayCheckbox = 'display:none'
          } else {
            this.displayNoneLeft = ''
            this.nbColsRight = 8
            this.displayButton = 'display:none'
            this.displayCheckbox = ''
          }
        }
        if (
          e[0].children === undefined &&
          this.selectedItemTable === undefined
        ) {
          this.noData = true
        }
      }
      this.showInfo = false
      this.Filtered(this.checkboxValue)
    },

    /**
     * Parse le CSV.
     * TODO : Récupérer le fichier CSV depuis la base de données
     */
    parseCSVFile() {
      const csvString = `Mdb;Pack;Mdb2;Pack2;Classe elt1 / Classe elt2;Elt1;Elt2
8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152794_L;8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152208_L;ITEM / ITEM;SCO_HRPERIOD_CALC.SFR_MAJ_TOT_ECRETEMENT;SCO_HRPERIOD_CALC.SFR_MAJ_TOT_ECRETEMENT
8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152972_L;8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_153221_L;FIELD / FIELD;SCO_AC_HR_PERIOD.SFR_TOT_INDEM_ACT_PARTIEL;SCO_AC_HR_PERIOD.SFR_TOT_INDEM_ACT_PARTIEL
8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152972_L;8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_153221_L;ITEM / ITEM;SCO_HRPERIOD_CALC.SFR_TOT_INDEM_ACT_PARTIEL;SCO_HRPERIOD_CALC.SFR_TOT_INDEM_ACT_PARTIEL
8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152208_L;8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152794_L;ITEM / ITEM;SCO_HRPERIOD_CALC.SFR_MAJ_TOT_ECRETEMENT;SCO_HRPERIOD_CALC.SFR_MAJ_TOT_ECRETEMENT
8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_153221_L;8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152972_L;FIELD / FIELD;SCO_AC_HR_PERIOD.SFR_TOT_INDEM_ACT_PARTIEL;SCO_AC_HR_PERIOD.SFR_TOT_INDEM_ACT_PARTIEL
8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_153221_L;8.1_HF2003_PLFR_152971.mdb;8.1_HF2003_SFR_152972_L;ITEM / ITEM;SCO_HRPERIOD_CALC.SFR_TOT_INDEM_ACT_PARTIEL;SCO_HRPERIOD_CALC.SFR_TOT_INDEM_ACT_PARTIEL
`
      const config = {
        delimiter: '', // auto-detect
        newline: '', // auto-detect
        quoteChar: '"',
        escapeChar: '"',
        header: false,
        transformHeader: undefined,
        dynamicTyping: false,
        preview: 0,
        encoding: '',
        worker: false,
        comments: false,
        step: undefined,
        complete: undefined,
        error: undefined,
        download: false,
        downloadRequestHeaders: undefined,
        downloadRequestBody: undefined,
        skipEmptyLines: false,
        chunk: undefined,
        fastMode: undefined,
        beforeFirstChunk: undefined,
        withCredentials: undefined,
        transform: undefined,
        delimitersToGuess: [',', '\t', '|', ';', Papa.RECORD_SEP, Papa.UNIT_SEP]
      }
      this.csvFileHeader = Papa.parse(csvString, config).data[0]
      this.csvFile = Papa.parse(csvString, config).data.slice(1)
      this.csvFile = this.csvFile.slice(0, this.csvFile.length - 1)
    },

    /**
     * Filtre l'affichage pour n'avoir que les contrôles KO.
     * @param {bool} checkboxValue - Valeur de la checkbox.
     */
    Filtered(checkboxValue) {
      this.checkboxValue = checkboxValue
      if (checkboxValue === true) {
        this.tableFiltered = []
        if (this.selectedItemTable !== undefined) {
          this.selectedItemTable.forEach((element) => {
            if (element.result === 'mdi-alert-circle') {
              this.tableFiltered.push(element)
            }
          })
        }
      } else {
        this.tableFiltered = this.selectedItemTable
      }
    },

    /**
     * Permet de retouner à l'affichage avec l'arborescence quand on est sur l'affichage du CSV.
     * @param {object} value - $event.
     */
    backToTreeView(value) {
      this.displayNoneLeft = ''
      this.nbColsRight = 8
      this.displayButton = 'display:none'
      this.showInfo = true
      this.displayCheckbox = ''
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
