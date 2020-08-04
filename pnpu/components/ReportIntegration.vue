<template>
  <v-layout>
    <v-row>
      <v-col cols="12" class="pt-0 mt-0 d-flex justify-space-between">
        <v-card flat class="mr-auto">
          <v-card-title class="pt-0 mt-0"
            >Rapport d'execution du processus
          </v-card-title>
          <v-card-subtitle class="pb-0">
            Intégration
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
                  v-if="item.result === iconValid"
                  color="success"
                  >{{ item.result }}</v-icon
                ><v-icon
                  v-if="item.result === iconWarning"
                  color="yellow darken-2"
                  >{{ item.result }}</v-icon
                ><v-icon
                  v-if="item.result === iconAlert"
                  color="error"
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
            label="Voir uniquement les installation de pack en erreur"
            hide-details
            @change="Filtered($event)"
          ></v-checkbox>
        </v-list-item-group>
        <v-alert
          v-if="showTooltip === true && tooltip !== ''"
          icon="mdi-help-circle"
          prominent
          text
          type="primary"
        >
          {{ tooltip }}
        </v-alert>
        <transition v-if="noData === false" appear name="fade">
          <v-card>
            <v-simple-table>
              <template v-slot:default>
                <thead>
                  <tr>
                    <th class="text-left">Nom</th>
                    <th v-if="hasResult === true" class="text-left">
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
                      max-width="700px"
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
                    <td v-if="hasResult === true">
                      <v-icon
                        v-if="item.result === iconValid"
                        color="success"
                        >{{ item.result }}</v-icon
                      >
                      <v-icon
                        v-if="item.result === iconWarning"
                        color="yellow darken-2"
                        >{{ item.result }}</v-icon
                      >
                      <v-icon
                        v-if="item.result === iconAlert"
                        color="error"
                        >{{ item.result }}</v-icon
                      >
                    </td>
                  </tr>
                </tbody>
              </template>
            </v-simple-table>
          </v-card>
        </transition>
        <v-alert
          v-if="noData === true"
          :icon=iconValid
          prominent
          text
          type="success"
        >
          L'installation du package s'est déroulée avec succès.
        </v-alert>
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
    active: [],
    selectedItemTable: [],
    titleTable: '',
    tableFiltered: [],
    checkboxValue: false,
    noData: false,
    hasResult: true,
    tooltip: '',
    showTooltip: false,
    treeviewFiltered: [],
    iconValid: 'mdi-check-circle',
    iconAlert: 'mdi-alert-circle',
    iconWarning: 'mdi-alert'
  }),

  computed: {

    /**
     * Treeview searchbar
     */
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
     * Get treeview item selected in order to generate table values
     * @param {object} - Element
     */
    getSelected(e) {
      if (e.length > 0) {
        for (const selectedItem of e) {
          if (selectedItem.message === undefined && selectedItem.children === undefined) {
            this.noData = true
          } else {
            this.GenerateTableValues(selectedItem.message)
            this.GenerateTableValues(selectedItem.children)
            this.titleTable = selectedItem.name
            this.noData = false
          }
        }
      }
      this.Filtered(this.checkboxValue)
    },

    /**
     * Generate table values according to treeview items selected
     * @param {array} items - Treeview items selected 
     */
    GenerateTableValues(items) {
      const vm = this
      if (items !== undefined) {
        this.selectedItemTable = items
        items.forEach((children) => {
          (children.result === undefined) ? vm.hasResult = false : vm.hasResult = true
        })
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
            if (element.result !== this.iconValid) {
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
