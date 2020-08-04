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
              :items="items"
              selection-type="leaf"
              :search="search"
              :filter="filter"
              hoverable
              returnObject
              transition
              activatable
              @update:active="getSelected($event)"
              ref="trigger"
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
                ><v-icon
                  v-if="item.result === 'mdi-alert'"
                  color="yellow darken-2"
                  >{{ item.result }}</v-icon
                ><v-icon
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
                v-on="on"
                :style="displayButton"
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
            @change="Filtered($event)"
            :style="displayCheckbox"
          ></v-checkbox>
        </v-list-item-group>
        <v-alert
          v-if="showTooltip === true"
          icon="mdi-help-circle"
          prominent
          text
          type="primary"
        >
          {{ tooltip }}
        </v-alert>
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
                      <v-icon
                        v-if="item.result === 'mdi-information-variant'"
                        >{{ item.result }}</v-icon
                      >
                    </td>
                  </tr>
                </tbody>
              </template>
            </v-simple-table>
          </v-card>
          <v-card v-else :style="displayButton">
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
export default {
  props: {
    idPROCESS: {
      type: String,
      default: '1'
    },
    data: {
      type: String,
      default: ''
    }
  },
  data: () => ({
    search: null,
    caseSensitive: false,
    open: ['public'],
    selection: [],
    items: [
      {
        id: '1',
        name: "Analyse d'impact sur les données",
        idClient: '111',
        result: 'mdi-alert-circle',
        debut: '22/06/20 14:45:34',
        fin: '22/06/20 14:45:35',
        children: [
          {
            id: '101',
            name: 'TESTS_ANALYSE_DATA.mdb',
            result: 'mdi-alert-circle',
            tooltip:
              "Analyse d'impact des données livrées dans le fichier TESTS_ANALYSE_DATA.mdb",
            children: [
              {
                id: '10101',
                name: '8.1_HF2001_PLFR_142112_D',
                numCommande: '1',
                result: 'mdi-check-circle',
                tooltip: '',
                children: [
                  {
                    id: '1010101',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.011' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010102',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.012' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010103',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.013' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010104',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.014' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010105',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT05 From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.011' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010106',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT05 From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.012' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010107',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT05 From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.013' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010108',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT05 From Origin To Destination Where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.40.014' and SFR_ID_ORIG_PARAM = 'CLI' \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010109',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT08  from origin to destination where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.78.004' and SFR_ID_CODE_DSN = '02'  \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010110',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT08  from origin to destination where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.78.004' and SFR_ID_CODE_DSN = '03'  \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010111',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT08  from origin to destination where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.78.004' and SFR_ID_CODE_DSN = '23'  \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  },
                  {
                    id: '1010112',
                    name:
                      "Replace M4SFR_DSN_PARAM_RUB_NAT08  from origin to destination where \"ID_ORGANIZATION = '0001' and SFR_ID_RUBRIQUE = 'S21.G00.78.004' and SFR_ID_CODE_DSN = '43'  \"",
                    result: 'mdi-information-variant',
                    message: 'Pas de contrôle automatique sur cette commande.'
                  }
                ]
              },
              {
                id: '10102',
                name: '8.1_HF2001_PLFR_142112_D',
                numCommande: '2',
                result: 'mdi-alert-circle',
                tooltip: '',
                children: [
                  {
                    id: '1010201',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.011'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010202',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.012'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010203',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.013'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010204',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.014'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010205',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT05', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.011'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010206',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT05', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.012'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010207',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT05', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.013'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010208',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT05', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.40.014'' and SFR_ID_ORIG_PARAM = ''CLI'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010209',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT08', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.78.004'' and SFR_ID_CODE_DSN = ''02'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010210',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT08', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.78.004'' and SFR_ID_CODE_DSN = ''03'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010211',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT08', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.78.004'' and SFR_ID_CODE_DSN = ''23'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-check-circle',
                    message:
                      'Les données du client sont standards. La propagation peut-être faite.'
                  },
                  {
                    id: '1010212',
                    name:
                      "EXEC M4SFR_COPY_DATA_ORG @table='M4SFR_DSN_PARAM_RUB_NAT08', @id_orga_origin = '0001', @id_orgas_dest = '9999', @opt_where = 'ID_ORGANIZATION = ''0001'' and SFR_ID_RUBRIQUE = ''S21.G00.78.004'' and SFR_ID_CODE_DSN = ''43'' ', @opt_suppr = 1, @debug = 0",
                    result: 'mdi-alert-circle',
                    message:
                      'Les données du client contiennent du spécifique. Commande à traiter à la main.'
                  }
                ]
              }
            ]
          }
        ]
      }
    ],
    headers: [
      {
        text: 'Mdb',
        align: 'start',
        filterable: false,
        value: '0'
      },
      { text: 'Pack', value: '1' },
      { text: 'Mdb2', value: '2' },
      { text: 'Pack2', value: '3' },
      { text: 'Classe elt1 / Classe elt2', value: '4' },
      { text: 'Elt1', value: '5' },
      { text: 'Elt2', value: '6' }
    ],
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
    tooltip: '',
    showTooltip: false
  }),

  computed: {
    filter() {
      return this.caseSensitive
        ? (item, search, textKey) => item[textKey].includes(search) > -1
        : undefined
    }
  },

  created() {
    this.selectedItemTable = this.items[0].children
    this.tableCtrlDepInterPack = this.items[0].children[
      this.items[0].children.length - 1
    ]
    this.titleTable = this.items[0].name
    this.Filtered(this.checkboxValue)
  },

  mounted() {
    this.csv()
  },

  methods: {
    getSelected(e) {
      if (e.length > 0) {
        for (const selectedItem of e) {
          this.showTooltip = false
          if (selectedItem.children !== undefined) {
            this.selectedItemTable = selectedItem.children
            this.hasMessage = false
          } else {
            this.tooltip = selectedItem.Tooltip
            this.showTooltip = true
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

    csv() {
      const csvString = ``
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
