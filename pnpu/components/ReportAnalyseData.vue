<template>
  <v-layout>
    <v-row>
      <v-col cols="12" class="pt-0 mt-0 d-flex justify-space-between">
        <v-card flat class="mr-auto">
          <v-card-title class="pt-0 mt-0"
            >Rapport d'execution du processus
          </v-card-title>
          <v-card-subtitle class="pb-0">
            Analyse d'impact sur les données
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
      </v-col>
      <v-col cols="4">
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
              :search="search"
              :filter="filter"
              hoverable
              return-object
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
                ><v-icon
                  v-if="item.result === 'mdi-alert'"
                  color="yellow darken-2"
                  >{{ item.result }}</v-icon
                ><v-icon
                  v-if="item.result === 'mdi-alert-circle'"
                  color="error"
                  >{{ item.result }}</v-icon
                ><v-icon
                  v-if="item.result === 'mdi-information-outline'"
                  color="grey darken-1"
                  >{{ item.result }}</v-icon
                >
              </template>
            </v-treeview>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="8">
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
            v-model="checkboxValue"
            label="Voir uniquement les commandes en erreur / info"
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
              @click:row="getSelectedRow($event)"
            >
              <template v-slot:top>
                <v-text-field v-model="searchDataTable" label="Chercher un élément" class="mx-4" append-icon="mdi-magnify"></v-text-field>
              </template>
              <template v-slot:item.result="{ item }">
                <v-tooltip bottom @input="getSelectedRow(item)">
                  <template v-slot:activator="{ on, attrs }">
                    <v-icon v-bind="attrs" v-on="on" :color=getgetColorIconResult(item.result)>{{ item.result }}</v-icon>
                  </template>
                  <span>{{ tooltipMessage }}</span>
                </v-tooltip>
              </template>
            </v-data-table>
        </transition>
      </v-col>
    </v-row>
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
    search: null,
    caseSensitive: false,
    open: ['public'],
    selection: [],
    headers: [
      { text: 'Nom', value: 'name' },
      { text: 'Status', value: 'result' }
    ],
    active: [],
    selectedItemTable: [],
    titleTable: '',
    tableFiltered: [],
    checkboxValue: false,
    searchDataTable: '',
    page: 1,
    pageCount: 0,
    itemsPerPage: 15,
    tooltip: '',
    tooltipMessage: ''
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
    this.titleTable = this.reportJsonData[0].name
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
     * Get treeview item selected in order to generate table values
     * @param {object} - Element
     */
    getSelected(e) {
      if (e.length > 0) {
        for (const selectedItem of e) {
          this.titleTable = selectedItem.name
          if (selectedItem.listCommand !== undefined) {
            this.headers = [
              { text: 'Nom de la commande RDL', value: 'name' },
              { text: 'Status', value: 'result' }
            ]
          } else {
            this.headers = [
              { text: 'Nom', value: 'name' },
              { text: 'Status', value: 'result' }
            ]
          }
          this.GenerateTableValues(selectedItem.children)
          this.GenerateTableValues(selectedItem.listCommand)
          
        }
      }
      this.Filtered(this.checkboxValue)
    },

    /**
     * Get table selected row
     * @param {array} - Row
     */
    getSelectedRow(row) {
      this.tooltipMessage = row.message
    },

    /**
     * Generate table values according to treeview items selected
     * @param {array} items - Treeview items selected 
     */
    GenerateTableValues(items) {
      const vm = this
      if (items !== undefined) {
        this.selectedItemTable = items
      }
    },

    /**
     * Filter items selected into treeview in order to generate a table with only error control
     * @param {boolean} checkboxValue - Checkbox value
     */
    Filtered(checkboxValue) {
      this.checkboxValue = checkboxValue
      if (checkboxValue) {
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
