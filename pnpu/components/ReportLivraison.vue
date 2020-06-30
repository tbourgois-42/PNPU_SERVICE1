<template>
  <v-layout>
    <v-row>
      <v-col cols="12" class="pt-0 mt-0 d-flex justify-space-between">
        <v-card flat class="mr-auto">
          <v-card-title class="pt-0 mt-0"
            >Rapport d'execution du processus
          </v-card-title>
          <v-card-subtitle class="pb-0">
            Livraison
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
        <v-btn
          depressed
          class="mr-4 mt-2 pr-4"
          color="primary"
          :disabled="downloadDisable"
          @click="downloadZip()"
          ><v-icon left>mdi-download</v-icon> Télécharger</v-btn
        >
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
                <v-chip v-if="item.name === 'Eléments à localiser'">{{
                  item.nbElements
                }}</v-chip>
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
            <v-list-item-title class="subtitle-1 mb-1">
              {{ titleTable }}</v-list-item-title
            >
          </v-list-item-content>
        </v-list-item-group>
        <v-card flat class="mb-4 tile">
          <v-card-title class="headline"
            >{{ nbAvailablePack }} Package(s) disponible(s)</v-card-title
          >
          <v-card-subtitle v-if="nbAvailablePack > 0"
            >L'instance #{{ idInstanceWF }} du workflow #{{ workflowID }} a
            généré un/des package(s).</v-card-subtitle
          >
          <v-alert
            v-if="nbLocalisation > 0"
            icon="mdi-check"
            prominent
            text
            type="success"
          >
            La tâche de localisation {{ clientTaskName }} a été générée le
            {{ new Date().toLocaleString() }} sur l'environnement QA1
          </v-alert>
          <v-alert
            v-if="nbAvailablePack === 0"
            icon="mdi-check"
            prominent
            text
            type="success"
          >
            Aucun tâche de localisation n'a été créé sur l'environnement client
            car aucun élément à localiser n'a été détécté lors de l'éxecution du
            workflow
          </v-alert>
        </v-card>
        <transition v-if="noData === false" appear name="fade">
          <v-card>
            <v-simple-table v-if="showSimpleTable">
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
                  <tr v-for="item in selectedItemTable" :key="item.name">
                    <td>{{ item.name }}</td>
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
            <v-data-table
              v-if="showDataTable"
              :headers="headersDataTable"
              :items="itemsDataTable"
              :search="searchDataTable"
              :page.sync="page"
              :items-per-page="itemsPerPage"
              hide-default-footer
              multi-sort
              @page-count="pageCount = $event"
            ></v-data-table>
          </v-card>
        </transition>
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
      </v-col>
    </v-row>
  </v-layout>
</template>

<script>
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
    },
    clientID: {
      type: String,
      default: ''
    }
  },
  data: () => ({
    search: null,
    caseSensitive: false,
    open: ['public'],
    selection: [],
    items: [],
    headersDataTable: [
      { text: 'Object Type', value: 'objectType' },
      { text: 'Object ID', value: 'objectID' }
    ],
    active: [],
    selectedItemTable: [],
    titleTable: '',
    checkboxValue: false,
    noData: false,
    hasMessage: false,
    searchDataTable: '',
    page: 1,
    pageCount: 0,
    itemsPerPage: 10,
    itemsDataTable: [],
    showDataTable: false,
    showSimpleTable: true,
    iconValid: 'mdi-check-circle',
    iconAlert: 'mdi-alert-circle',
    iconWarning: 'mdi-alert',
    undefinedIcon: '',
    processInError: false,
    processInWarning: false,
    clientTaskName: '',
    nbLocalisation: 0
  }),

  computed: {
    filter() {
      return this.caseSensitive
        ? (item, search, textKey) => item[textKey].includes(search) > -1
        : undefined
    },

    downloadDisable() {
      if (this.nbAvailablePack === 0) {
        return true
      } else {
        return false
      }
    }
  },

  created() {
    this.titleTable = this.reportJsonData[0].name
    this.selectedItemTable = this.reportJsonData[0].children
  },

  methods: {
    getSelected(e) {
      if (e.length > 0) {
        this.setDefaultAlert()
        for (const selectedItem of e) {
          switch (selectedItem.name) {
            case 'Eléments à localiser':
              this.showLocalisation(selectedItem)
              break
            case 'Livraison':
              this.showLivraison(selectedItem)
              break
            default:
              this.showDefault(selectedItem)
              break
          }
        }
      }
    },

    setDefaultAlert() {
      this.processInError = false
      this.processInWarning = false
      this.undefinedIcon = false
      this.noData = false
    },

    showDefault(selectedItem) {
      switch (selectedItem.result) {
        case this.iconValid:
          this.noData = true
          break
        case this.iconAlert:
          this.processInError = true
          break
        case this.iconWarning:
          this.processInWarning = true
          break
        default:
          this.showSimpleTable = false
          this.undefinedIcon = true
          break
      }
    },

    showLivraison(selectedItem) {
      this.showSimpleTable = false
      if (selectedItem.children !== undefined) {
        this.showSimpleTable = true
        this.selectedItemTable = selectedItem.children
      }
      this.showDataTable = false
    },

    showLocalisation(selectedItem) {
      this.titleTable = selectedItem.name
      this.clientTaskName = selectedItem.cctTaskID
      this.itemsDataTable = selectedItem.elements
      this.nbLocalisation = selectedItem.nbElements
      this.headersDataTable = [
        { text: 'Object Type', value: 'objectType' },
        { text: 'Object ID', value: 'objectID' },
        { text: 'Parent Object ID', value: 'parentObj' },
        { text: 'Aux Object', value: 'auxObj' },
        { text: 'Aux 2 Object', value: 'aux2Obj' },
        { text: 'Aux 3 Object', value: 'aux3Obj' }
      ]
      this.showSimpleTable = false
      this.showDataTable = true
    },

    /**
     * Download available zip file
     */
    downloadZip() {
      const vm = this
      axios({
        method: 'GET',
        url:
          `${process.env.WEB_SERVICE_WCF}/clients/livraison/` +
          this.workflowID +
          `/` +
          this.idInstanceWF +
          `/` +
          this.clientID,
        responseType: 'arraybuffer'
      })
        .then((response) => {
          if (response.status === 200) {
            const url = window.URL.createObjectURL(new Blob([response.data]))
            const link = document.createElement('a')
            link.href = url
            link.setAttribute(
              'download',
              this.clientName +
                '_' +
                this.workflowID +
                '_' +
                this.idInstanceWF +
                '_.zip'
            )
            document.body.appendChild(link)
            link.click()
          }
        })
        .catch(function(error) {
          vm.showSnackbar(
            'error',
            `${error} ! Impossible de récupérer les packages`
          )
        })
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
