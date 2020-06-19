<template>
  <v-app>
    <v-container>
      <v-flex md12>
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title class="title">
              {{ clientName }}
            </v-list-item-title>
            <v-list-item-subtitle>
              {{ workflowDate }} | Step {{ etape }}
            </v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-flex>
      <v-divider class="my-2 mx-4" inset></v-divider>

      <v-stepper v-model="e1" class="mt-6" @change="getSelectedStep($event)">
        <v-stepper-header>
          <template v-for="(step, idxStep) in steps">
            <v-stepper-step
              :key="idxStep"
              :step="step.ORDER_ID"
              :complete="step.ID_STATUT"
              editable
              :color="step.COLOR"
              :edit-icon="step.ICON"
            >
              {{ step.PROCESS_LABEL }}
            </v-stepper-step>
            <v-divider v-if="step !== steps" :key="idxStep"></v-divider>
          </template>
        </v-stepper-header>

        <v-stepper-items>
          <v-stepper-content
            v-for="(step, ixdContent) in steps"
            :key="ixdContent"
            :step="ixdContent"
          >
            <v-col cols="12" class="pt-0 mt-0 d-flex justify-space-between">
              <v-card flat class="mr-auto">
                <v-card-title class="pt-0 mt-0"
                  >Rapport d'execution du processus
                </v-card-title>
                <v-card-subtitle class="pb-0">{{
                  step.PROCESS_LABEL
                }}</v-card-subtitle> </v-card
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
            <v-divider class="mx-4 mb-4"></v-divider>
            <!-- v-if="e1 === 0" -->
            <ReportTNR
              v-if="
                Object.entries(JSON_TEMPLATE).length > 0 && reportTNR == true
              "
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
            />
            <!-- Report -->
            <v-col cols="12">
              <v-alert
                v-if="currentID_STATUT === 'mdi-hand'"
                icon="mdi-information-outline"
                text
                color="warning"
                >Ce processus demande l'intervention d'un utilisateur pour
                pouvoir continuer ou non le workflow. Pour plus d'information
                veuillez consulter le rapport d'éxecution ci-dessous</v-alert
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
            <v-row
              v-if="
                Object.entries(JSON_TEMPLATE).length > 0 && reportTNR == false
              "
            >
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
                    v-if="
                      titleTable !== 'Contrôle des dépendances inter packages'
                    "
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
                              <v-icon v-else color="error">{{
                                item.result
                              }}</v-icon>
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
                  Ce contrôle s'est déroulé avec succès, il n'a généré aucun
                  message d'erreur.
                </v-alert>
                <v-alert
                  v-if="showInfo === true"
                  icon="mdi-information-variant"
                  prominent
                  text
                  type="primary"
                >
                  Pour visualiser les résultats de ce contrôle, veuillez cliquer
                  sur {{ titleTable }} dans l'arborescence.
                </v-alert>
              </v-col>
            </v-row>
            <!-- Fin Report -->
            <v-col else cols="12">
              <v-alert
                v-if="Object.entries(JSON_TEMPLATE).length === 0"
                :icon="alertIcon"
                text
                color="info"
                >{{ alertMessage }}</v-alert
              >
            </v-col>
          </v-stepper-content>
        </v-stepper-items>
      </v-stepper>
      <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
        {{ snackbarMessage }}
        <v-btn dark text @click="snackbar = false">
          Close
        </v-btn>
      </v-snackbar>
    </v-container>
  </v-app>
</template>
<script>
import Papa from 'papaparse'
import axios from 'axios'
import ReportTNR from '../../components/ReportTNR'
export default {
  components: { ReportTNR },
  data() {
    return {
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
      idInstanceWF: ''
    }
  },

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

  created() {
    // Récupération des informations du client sélectionné au niveau du Dashboard
    this.clientId = this.$route.params.clientId
    this.clientName = this.$route.params.clientName
    this.etape = this.$route.params.step + 1
    this.e1 = this.$route.params.step + 1
    this.workflowDate = this.$route.params.workflowDate
    this.workflowID = this.$route.params.workflowID
    this.textStatus = this.$route.params.textStatus
    this.idInstanceWF = this.$route.params.idInstanceWF
    if (this.$route.params.clientId === undefined) {
      return this.$nuxt.error({ statusCode: 404 })
    } else {
      this.getWorkflowProcesses()
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
     * Etape sélectionnée dans le Stepper.
     * @param {number} val - Step sélectionnée dans le stepper.
     */
    getSelectedStep(val) {
      const vm = this
      vm.e1 = val
      vm.steps.forEach((element, idx) => {
        if (idx === val) {
          vm.currentID_STATUT = element.ICON
          vm.currentID_PROCESS = element.ID_PROCESS
          vm.getReportFromDB()
        }
      })
    },

    /**
     * Récupère les processus associés à un worflow pour permettre la génération du stepper.
     */
    async getWorkflowProcesses() {
      const vm = this
      try {
        const res = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/workflow/` +
            vm.workflowID +
            `/processus`
        )
        vm.steps = res.data.GetWorkflowProcessesResult
        for (let i = 0; i < vm.e1; i++) {
          vm.steps[i].ID_STATUT = true
          vm.steps[i].ICON = 'mdi-check'
          vm.steps[i].COLOR = 'light green'
        }
        if (vm.textStatus === 'ERROR') {
          vm.steps[vm.e1 - 1].COLOR = 'error'
          vm.steps[vm.e1 - 1].ICON = 'mdi-alert'
          vm.steps[vm.e1 - 1].ID_STATUT = true
        } else if (vm.textStatus === 'WARNING') {
          vm.steps[vm.e1 - 1].COLOR = 'warning'
          vm.steps[vm.e1 - 1].ICON = 'mdi-hand'
          vm.steps[vm.e1 - 1].ID_STATUT = true
        } else {
          vm.steps[vm.e1 - 1].COLOR = 'primary'
          vm.steps[vm.e1 - 1].ICON = 'mdi-pencil'
          vm.steps[vm.e1 - 1].ID_STATUT = true
        }
      } catch (e) {
        return e
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
     * Récupère le rapport JSON depuis la base de données.
     * Valorise les variables nécessaire pour l'arborescence
     */
    getReportFromDB() {
      const vm = this
      vm.startLoadingDatas()
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/report/` +
            vm.workflowID +
            `/` +
            vm.idInstanceWF +
            `/` +
            vm.currentID_PROCESS +
            `/` +
            vm.clientId
        )
        .then(function(response) {
          if (response.data.getReportResult.length > 0) {
            vm.JSON_TEMPLATE = JSON.parse(
              response.data.getReportResult[0].JSON_TEMPLATE
            )
            vm.isTNRReport(vm.JSON_TEMPLATE[0].name)
            vm.selectedItemTable = vm.JSON_TEMPLATE[0].children
            vm.tableCtrlDepInterPack =
              vm.JSON_TEMPLATE[0].children[
                vm.JSON_TEMPLATE[0].children.length - 1
              ]
            vm.titleTable = vm.JSON_TEMPLATE[0].name
            vm.Filtered(vm.checkboxValue)
          } else {
            vm.JSON_TEMPLATE = {}
          }
          vm.endLoadingDatas(vm.JSON_TEMPLATE)
          vm.loadingReport = false
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} !`)
        })
    },

    /**
     * Check if is a TNR report.
     * @param {string} reportName - Name of report.
     */
    isTNRReport(reportName) {
      if (reportName === 'Tests de Non Régression (TNR)') {
        this.reportTNR = true
      } else {
        this.reportTNR = false
      }
    },

    /**
     * Start Loading datas
     */
    startLoadingDatas() {
      this.alertIcon = 'mdi-progress-download'
      this.alertMessage = 'Chargement du report en cours. Veuillez patienter...'
      this.loadingReport = true
    },

    /**
     * End Loading datas
     * @param {object} datas - Json report
     */
    endLoadingDatas(datas) {
      this.alertMessage = ''
      if (Object.entries(datas).length === 0) {
        this.setNoData()
      }
      this.alertIcon = 'mdi-information-outline'
      this.loadingReport = false
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
