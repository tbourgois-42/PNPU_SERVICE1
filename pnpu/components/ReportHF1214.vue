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
        name: 'Pré contrôle des .mdb',
        result: 'mdi-alert-circle',
        debut: '09/06/20 17:48:25',
        fin: '09/06/20 17:48:37',
        children: [
          {
            id: '2',
            name: '8.1.6_HF1214.mdb',
            result: 'mdi-alert-circle',
            children: [
              {
                id: '201',
                name: 'Contrôle des commandes interdites',
                Tooltip:
                  'Vérifie si le mdb standard ne contient pas de commande risquant de provoquer un dysfonctionnement. Par exemple les commandes TRANSFER "SECURITY... ou KILL "SECURITY... -  - Liste des commandes interdites : - TRANSFER "SECURITY - KILL "SECURITY - REPLACE M4RSC_APPROLE',
                result: 'mdi-check-circle'
              },
              {
                id: '202',
                name: 'Contrôle des données Replace',
                Tooltip:
                  'Vérifie si les données des tables présentes dans les scripts Replace Row sont bien présentes dans le mdb afin de détecter une erreur de packaging ou un oubli de livraison de paramétrage',
                result: 'mdi-check-circle'
              },
              {
                id: '203',
                name: 'Contrôle des ID Synonym',
                Tooltip:
                  "Vérifie si les items de paie standards livrés dans le mdb ne sont pas livrés sur des plages d'ID Synonym réservées au client.Cela permet d'éviter de livrer un item standard ayant le même ID Synonym qu'un item spécifique -  - Liste des plages réservées aux clients : - De 5001 à 9999 - De 10301 à 10999 - De 11301 à 11999 - De 13301 à 13999 - De 14501 à 14999 - De 17000 à 25000 - De 27001 à 29999 - De 33001 à 39999 - De 43001 à 49999 - De 53001 à 59999 - De 60501 à 60999 - De 63001 à 69999 - De 73001 à 79999 - De 83001 à 89999 - De 90301 à 95999 - De 97000 à 99999 - De 150000 à 199999 - De 250000 à 299999 - De 350000 à 399999 - De 450000 à 499999 - De 550000 à 599999 - De 650000 à 799999 - De 950000 à 999999 - De 1150000 à 1199999 - De 1250000 à 1299999 - De 1350000 à 1399999 - De 1450000 à 1499999 - De 1550000 à 1599999 - De 1650000 à 1999999 - De 2500000 à 2999999 - De 3500000 à 4999999 - De 5400000 à 5999999",
                result: 'mdi-check-circle'
              },
              {
                id: '204',
                name: 'Contrôles des ID Synonym existant',
                Tooltip:
                  "Vérifie si les items de paie livrés dans le mdb n'utilisent pas un ID Synonym déjà pris sur la base de référence afin d'éviter le risque de sélectionner le mauvais item si on utilise la sélection par CODE dans la saisie de EV par exemple",
                result: 'mdi-check-circle'
              },
              {
                id: '205',
                name: 'Contrôle des totaux',
                Tooltip:
                  "Vérifie que les items de paie utilisés dans les totaux livrés dans le mdb standard existent sur la base de référence ou dans le mdb standard afin d'éviter d'avoir une erreur lors du calcul de paie",
                result: 'mdi-check-circle'
              },
              {
                id: '206',
                name: 'Contrôle livraison bulletin électronique',
                Tooltip:
                  "Vérifie dans le pack standard si la présentation SCO_DP_PAYROLL_CHANNEL est livrée. Si c'est le cas un warning est déclenché pour indiquer qu'une action est à prévoir pour les clients n'utilisant pas la présentation SFR",
                result: 'mdi-alert',
                message: [
                  {
                    id: '2061',
                    name:
                      'La présentation SCO_DP_PAYROLL_CHANNEL est livrée dans le pack 8.1.6_HF1214_SFR_127702_L.'
                  }
                ]
              },
              {
                id: '207',
                name: "Contrôle des niveaux d'héritage",
                Tooltip:
                  "Vérifie que les éléments standards livrés sont au niveau d'héritage le plus fin afin d'éviter que la modification livrée ne soit neutralisée par une surécriture standard",
                result: 'mdi-check-circle'
              },
              {
                id: '208',
                name: 'Contrôle des niveaux de saisies',
                Tooltip:
                  "Vérifie qu'il n'y a pas de perte de niveau de saisie d'un item de paie entre le mdb standard et la base de référence pour éviter les régressions et rappels intempestifs sur cet item lors du calcul de paie",
                result: 'mdi-check-circle'
              },
              {
                id: '209',
                name: "Contrôle livraison d'objets techno",
                Tooltip:
                  "Vérifie que le mdb standard ne livre pas d'élément techno. Les éléments techno doivent être livrés exclusivement dans des HF Technos",
                result: 'mdi-check-circle'
              },
              {
                id: '210',
                name: 'Contrôle des paramètres applicatifs',
                Tooltip:
                  'Vérifie que le mdb standard ne livre pas des paramètres applicatifs risquant de provoquer un dysfonctionnement. Par exemple les paramètres concernant le machine to machine. -  - Liste des clés interdites : - ENC_CONN_STR_RAMDL - FILESERVICE_URI - M2M_PROXY_HOST - M2M_PROXY_LOGIN - M2M_PROXY_PORT - M2M_PROXY_PROTOCOLE - M2M_PROXY_PWD - PROXY_HOST - PROXY_PORT - PROXY_USER - PROXY_USER_DOMAIN - PROXY_USER_PASSWORD - PARTNER_PROCESS_SERVER - PARTNER_PROCESS_USER - SERVER_URL - MAIL_SERVICE_URL -  - Liste des sections interdites : - AUTHENTICATION - PORTS - SERVERS - SMTP_TRANSPORT',
                result: 'mdi-check-circle'
              },
              {
                id: '211',
                name: 'Contrôle propagation des données',
                Tooltip:
                  "Vérifie si les données des tables multi orga livrées dans le mdb sont propagées pour qu'elles soient appliquées sur les ID Organization des clients",
                result: 'mdi-check-circle'
              },
              {
                id: '212',
                name: 'Contrôle propagation dans project explorer',
                Tooltip:
                  "Vérifie si les données du project explorer livrées en standard sur SOC_0001 sont propagées sur SOC_0002 pour qu'elles soient appliquées sur les clients. Si ce n'est pas le cas un warning est déclenché pour qu'une action soit prévue",
                result: 'mdi-check-circle'
              },
              {
                id: '213',
                name: 'Contrôle de sécurité sur les tables',
                Tooltip:
                  "Vérifie que les tables livrées sont sécurisées afin d'éviter que tous les utilisateurs puissent accéder aux données sans avoir les accès",
                result: 'mdi-alert-circle',
                message: [
                  {
                    id: '2131',
                    name: 'Table SFR_DSN_FICHES_PARAM non sécurisée.'
                  }
                ]
              },
              {
                id: '214',
                name: 'Contrôle de sécurité sur les tâches',
                Tooltip:
                  "Vérifie que les business process livrés sont sécurisés afin d'éviter que tous les utilisateurs puissent lancer la tâche sans avoir les accès",
                result: 'mdi-alert-circle',
                message: [
                  {
                    id: '2141',
                    name: 'Tâche SFR_JS_DSN_CONTROL_FPOC non sécurisée.'
                  },
                  {
                    id: '2142',
                    name: 'Tâche SFR_JS_DSN_CONTROL_FPOC_COMP non sécurisée.'
                  },
                  {
                    id: '2143',
                    name: 'Tâche SFR_JS_DSN_FILE_INTEG_SINGLE non sécurisée.'
                  },
                  {
                    id: '2144',
                    name: 'Tâche SFR_JS_DSN_FILE_INTEGRATOR non sécurisée.'
                  },
                  {
                    id: '2145',
                    name: 'Tâche SFR_JS_DSN_LIST non sécurisée.'
                  },
                  {
                    id: '2146',
                    name: 'Tâche SFR_JS_DSN_LIST_SINGLE non sécurisée.'
                  }
                ]
              },
              {
                id: '215',
                name: 'Contrôle des types de packages',
                Tooltip:
                  'Vérifie la cohérence entre le contenu des commandes et le type de package pour éviter par exemple la livraison d\'une commande TRANSFER "ITEM" dans un pack de données (D) au lieu d\'un pack logique (L)',
                result: 'mdi-check-circle'
              }
            ]
          },
          {
            id: '3',
            name: 'Contrôle des dépendances inter packages',
            result: 'mdi-check-circle',
            children: []
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
