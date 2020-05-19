<template>
  <v-layout>
    <v-row>
      <v-col :cols="nbColsLeft" :style="displayNoneLeft">
        <v-card class="mx-auto" max-width="500">
          <v-sheet class="pa-4 primary">
            <v-text-field
              v-model="search"
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
              :items="reportJsonData"
              selection-type="leaf"
              :search="search"
              :filter="filter"
              hoverable
              returnObject
              transition
              activatable
              @update:active="getSelected($event)"
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
                ><v-icon v-else color="error">{{ item.result }}</v-icon>
              </template>
            </v-treeview>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col :cols="nbColsRight">
        <v-list-item-group class="mb-5 d-flex">
          <v-list-item-icon>
            <v-icon>mdi-folder</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title class="subtitle-1 mb-1">
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
          <v-card
            v-if="titleTable !== 'Contrôle des dépendances inter packages'"
          >
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
                  <tr v-for="item in tableFiltered" :key="item.name">
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
                      <v-icon v-else color="error">{{ item.result }}</v-icon>
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
    </v-row>
  </v-layout>
</template>

<script>
import Papa from 'papaparse'
import Report from '../data/Report.json'
export default {
  props: {
    idPROCESS: {
      type: String,
      default: '1'
    },
    reportJsonData: {
      type: Object,
      default: () => {}
    }
  },
  data: () => ({
    rapport: Report,
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
    search: null,
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
    displayCheckbox: ''
  }),

  computed: {
    filter() {
      return this.caseSensitive
        ? (item, search, textKey) => item[textKey].includes(search) > -1
        : undefined
    }
  },

  created() {
    this.selectedItemTable = this.reportJsonData[0].children
    this.tableCtrlDepInterPack = this.reportJsonData[0].children[
      this.reportJsonData[0].children.length - 1
    ]
    this.titleTable = this.reportJsonData[0].name
    this.Filtered(this.checkboxValue)
  },

  mounted() {
    this.parseCSVFile()
  },

  methods: {
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

    backToTreeView(value) {
      this.displayNoneLeft = ''
      this.nbColsRight = 8
      this.displayButton = 'display:none'
      this.showInfo = true
      this.displayCheckbox = ''
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
