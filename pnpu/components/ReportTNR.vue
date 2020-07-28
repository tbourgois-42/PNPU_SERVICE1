<template>
  <v-layout row wrap>
    <v-container>
      <v-flex md12>
        <v-row>
          <v-col cols="12" class="pt-0 mt-0 d-flex justify-space-between">
        <v-card flat class="mr-auto">
          <v-card-title class="pt-0 mt-0"
            >Rapport d'execution du processus
          </v-card-title>
          <v-card-subtitle class="pb-0">
            Tests de Non Régression (TNR)
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
                  :items="reportJsonData"
                  selection-type="leaf"
                  :search="searchTreeview"
                  :filter="filter"
                  hoverable
                  returnObject
                  transition
                  open-all
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
                    ><v-icon
                      v-if="item.result === 'mdi-alert'"
                      color="yellow darken-2"
                      >{{ item.result }}</v-icon
                    >
                    <v-icon
                      v-if="item.result === 'mdi-information-outline'"
                      color="grey lighteen-1"
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
              <v-checkbox
                v-model="checkbox"
                label="Voir uniquement les TNR en écart"
                hide-details
                :style="displayCheckbox"
                @change="Filtered($event)"
              ></v-checkbox>
            </v-list-item-group>
            <v-alert
              v-if="noData === true"
              icon="mdi-check"
              prominent
              text
              type="success"
            >
              Aucun écart.
            </v-alert>
            <!--<v-chip class="ma-2" color="primary" label text-color="white">
              <v-avatar left class="primary darken-2">
                1
              </v-avatar>
              Filters
              <v-icon right>mdi-filter-variant</v-icon>
            </v-chip>
            <v-col class="d-flex ma-0 pa-0" cols="12">
              <v-select
                :items="itemsFilter"
                label="Filters"
                solo
                class="mr-4"
              ></v-select>
              <v-select
                :items="itemsFilterStatut"
                label="Statut"
                solo
                class="mr-4"
              ></v-select>
              <v-select
                :items="itemsFilterEcarts"
                label="Ecarts"
                solo
              ></v-select>
            </v-col>-->
            <!-- Début Data Table -->
            <v-card
              v-if="noData === false && showDataTable === true"
              class="mb-4"
            >
              <div v-if="showTxtMatricule" class="pt-4 pl-4 title">
                {{ txtMatriculeEcart }}
                <v-tooltip top>
                  <template v-slot:activator="{ on }">
                    <v-btn
                      v-if="showBtnPreviousTable === true"
                      x-small
                      fab
                      depressed
                      color="primary"
                      class="ma-4"
                      v-on="on"
                      @click="backToPreviewTable()"
                    >
                      <v-icon>mdi-undo</v-icon>
                    </v-btn>
                  </template>
                  <span>Retour</span>
                </v-tooltip>
              </div>
              <v-card-title>
                <v-text-field
                  v-model="searchTable"
                  append-icon="mdi-magnify"
                  label="Chercher un élément"
                  single-line
                  hide-details
                  class="mt-0 pt-0"
                ></v-text-field>
              </v-card-title>
              <v-data-table
                :headers="headers"
                :items="matriculeData"
                :search="searchTable"
                sort-by="name"
                class="elevation-1 cursor"
                @click:row="getSelectedRow($event)"
              ></v-data-table>
            </v-card>
            <!-- Fin Data Table -->
            <!-- Début Simple Table -->
            <v-card v-if="showSimpleTable === true">
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
                        <v-icon v-else color="yellow darken-2">{{
                          item.result
                        }}</v-icon>
                      </td>
                    </tr>
                  </tbody>
                </template>
              </v-simple-table>
            </v-card>
            <!-- Fin Simple Table -->
          </v-col>
        </v-row>
        <!-- Fin Report -->
      </v-flex>
    </v-container>
  </v-layout>
</template>

<script>
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
    titleTable: '',
    headers: [],
    searchTreeview: null,
    searchTable: null,
    caseSensitive: false,
    open: ['public'],
    selection: [],
    active: [],
    selectedItemTable: [],
    checkbox: false,
    tableFiltered: [],
    checkboxValue: false,
    noData: false,
    hasMessage: false,
    page: 1,
    pageCount: 0,
    itemsPerPage: 10,
    nbColsRight: 8,
    nbColsLeft: 4,
    displayNoneLeft: '',
    displayButton: 'display:none',
    showInfo: false,
    displayCheckbox: '',
    classification: '',
    showEcartClassif: false,
    showEcartSalarie: false,
    showCriteres: false,
    matriculeData: [],
    showDataTable: false,
    showSimpleTable: true,
    lstMatriculesEcarts: [],
    txtMatriculeEcart: '',
    showTxtMatricule: false,
    matriculeDataCopy: [],
    showBtnPreviousTable: false,
    ecartMatricule: false
  }),

  computed: {
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
    this.Filtered(this.checkboxValue)
  },

  methods: {
    /**
     * Elément sélectionné dans l'arborescence.
     * @param {object} e - $event.
     */
    getSelected(e) {
      this.txtMatriculeEcart = ''
      this.showBtnPreviousTable = false
      if (e.length > 0) {
        this.noData = false
        for (const selectedItem of e) {
          if (selectedItem.children !== undefined) {
            switch (selectedItem.name) {
              case 'Ecarts agrégés par classification':
                this.showSimpleTable = true
                this.showDataTable = false
                this.selectedItemTable = selectedItem.children
                this.matriculeData = selectedItem.children
                this.headers = [
                  {
                    text: 'Nom',
                    align: 'start',
                    sortable: false,
                    value: 'name'
                  },
                  { text: 'Statut', value: 'result' }
                ]
                break
              case 'Ecarts par salarié':
                this.showDataTable = true
                this.showDataTable = false
                this.selectedItemTable = selectedItem.children[0].children
                this.matriculeData = selectedItem.children[0].salaries
                this.headers = [
                  {
                    text: 'Date de paiement',
                    align: 'start',
                    sortable: false,
                    value: 'dtpaie'
                  },
                  {
                    text: "Date d'imputation",
                    sortable: false,
                    value: 'dtalloc'
                  },
                  {
                    text: 'ID Organisation',
                    value: 'idorga'
                  },
                  {
                    text: 'Société',
                    value: 'entity'
                  },
                  {
                    text: 'Etablissmement',
                    value: 'etab'
                  },
                  {
                    text: 'Matricule',
                    value: 'matricule'
                  },
                  {
                    text: 'Période',
                    value: 'period'
                  },
                  {
                    text: 'Item',
                    value: 'name'
                  },
                  {
                    text: 'Valeur avant',
                    value: 'valueBefore'
                  },
                  {
                    text: 'Valeur après',
                    value: 'valueAfter'
                  },
                  {
                    text: 'Ecart',
                    value: 'difference'
                  }
                ]
                break

              default:
                this.selectedItemTable = selectedItem.children
                this.showDataTable = false
                this.showSimpleTable = true
                break
            }
          } else if (selectedItem.name !== "Critères d'identification") {
            this.matriculeData = []
            if (
              selectedItem.ecarts !== undefined &&
              selectedItem.ecarts.length > 0
            ) {
              selectedItem.ecarts.forEach((matricule) => {
                this.matriculeData.push(matricule)
              })
              this.headers = [
                {
                  text: 'Item',
                  value: 'name'
                },
                {
                  text: 'Valeur avant',
                  value: 'valueBefore'
                },
                {
                  text: 'Valeur après',
                  value: 'valueAfter'
                },
                {
                  text: 'Ecart',
                  value: 'difference'
                },
                {
                  text: 'Commentaire',
                  value: 'comment'
                }
              ]
              this.showDataTable = true
              this.showSimpleTable = false
            } else if (selectedItem.name === 'Ecarts par salarié') {
              this.showDataTable = true
              this.showSimpleTable = false
              // this.selectedItemTable = selectedItem.children[0].children
              this.matriculeData = selectedItem.salaries
              this.headers = [
                {
                  text: 'Date de paiement',
                  align: 'start',
                  sortable: false,
                  value: 'dtpaie'
                },
                {
                  text: "Date d'imputation",
                  sortable: false,
                  value: 'dtalloc'
                },
                {
                  text: 'ID Organisation',
                  value: 'idorga'
                },
                {
                  text: 'Société',
                  value: 'entity'
                },
                {
                  text: 'Etablissmement',
                  value: 'etab'
                },
                {
                  text: 'Matricule',
                  value: 'matricule'
                },
                {
                  text: 'Période',
                  value: 'period'
                }
              ]
            } else {
              this.showDataTable = false
              this.showSimpleTable = false
              this.noData = true
            }
          }
          this.titleTable = selectedItem.name
        }
      }
      this.showInfo = false
      this.Filtered(this.checkboxValue)
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
     * Affiche la liste des salariés avec leurs écarts individuels sur un item de paie en particulier.
     */
    getSelectedRow(row) {
      if (row.matricule !== undefined && row.name === undefined) {
        const lstItemsEcarts = []
        for (let index = 0; index < row.ecarts.length; index++) {
          lstItemsEcarts.push(row.ecarts[index])
        }
        this.matriculeDataCopy = this.matriculeData
        this.matriculeData = lstItemsEcarts
        this.headers = [
          {
            text: 'Item',
            align: 'start',
            sortable: false,
            value: 'name'
          },
          {
            text: 'Valeur avant',
            value: 'valueBefore'
          },
          {
            text: 'Valeur après',
            value: 'valueAfter'
          },
          {
            text: 'Ecart',
            value: 'difference'
          },
          {
            text: 'Commentaire',
            value: 'comment'
          }
        ]
        this.showBtnPreviousTable = true
        this.txtMatriculeEcart = row.matricule + ' / ' + row.periode
        this.showTxtMatricule = true
        this.ecartMatricule = true
        this.showBtnPreviousTable = true
      } else {
        const lstMatriculesEcarts = []
        for (let index = 0; index < row.matricules.length; index++) {
          lstMatriculesEcarts.push(row.matricules[index])
        }
        this.matriculeDataCopy = this.matriculeData
        this.matriculeData = lstMatriculesEcarts
        this.headers = [
          {
            text: 'Date de paiement',
            align: 'start',
            sortable: false,
            value: 'dtpaie'
          },
          {
            text: "Date d'imputation",
            sortable: false,
            value: 'dtalloc'
          },
          {
            text: 'ID Organisation',
            value: 'idorga'
          },
          {
            text: 'Société',
            value: 'societe'
          },
          {
            text: 'Etablissmement',
            value: 'etablissement'
          },
          {
            text: 'Matricule',
            value: 'matricule'
          },
          {
            text: 'Période',
            value: 'period'
          },
          {
            text: 'Valeur avant',
            value: 'valueBefore'
          },
          {
            text: 'Valeur après',
            value: 'valueAfter'
          },
          {
            text: 'Ecart',
            value: 'difference'
          }
        ]
        this.showBtnPreviousTable = true
        this.txtMatriculeEcart = row.name
        this.showTxtMatricule = true
        this.ecartMatricule = false
        this.showBtnPreviousTable = true
      }
    },

    /**
     * Permet de retouner à l'affichage au niveau de l'arbre supérieur.
     */
    backToPreviewTable() {
      switch (this.ecartMatricule) {
        case true:
          this.headers = [
            {
              text: 'Date de paiement',
              value: 'dtpaie'
            },
            {
              text: "Date d'imputation",
              sortable: false,
              value: 'dtalloc'
            },
            {
              text: 'ID Organisation',
              value: 'idorga'
            },
            {
              text: 'Société',
              value: 'entity'
            },
            {
              text: 'Etablissement',
              value: 'etab'
            },
            {
              text: 'Matricule',
              value: 'matricule'
            },
            {
              text: 'Période',
              value: 'period'
            }
          ]
          break

        case false:
          this.headers = [
            {
              text: 'Item',
              value: 'name'
            },
            {
              text: 'Valeur avant',
              value: 'valueBefore'
            },
            {
              text: 'Valeur après',
              value: 'valueAfter'
            },
            {
              text: 'Ecart',
              value: 'difference'
            },
            {
              text: 'Commentaire',
              value: 'comment'
            }
          ]
          break

        default:
          break
      }
      this.showTxtMatricule = false
      this.showBtnPreviousTable = false
      this.matriculeData = this.matriculeDataCopy
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
    }
  }
}
</script>

<style lang="css">
.cursor {
  cursor: pointer;
}
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
