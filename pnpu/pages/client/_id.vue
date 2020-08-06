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
      <v-divider
        v-if="reportLivraison === false"
        class="my-2 mx-4"
        inset
      ></v-divider>

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
            <ReportLivraison
              v-if="
                Object.entries(JSON_TEMPLATE).length > 0 && reportLivraison === true
              "
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :nbAvailablePack="nbAvailablePack"
              :currentID_STATUT="currentID_STATUT"
              :clientID="clientId"
              :clientName="clientName"
            />
            <ReportPreControle v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportPreControle === true" 
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <ReportInitialisation v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportInitialisation === true"
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <ReportPackagingDependances v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportPackagingDependances === true"
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <ReportAnalyseData v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportAnalyseData === true"
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <ReportAnalyseLogique v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportAnalyseLogique === true"
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <ReportProcessusCritiques v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportProcessusCritiques === true"
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <ReportIntegration v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportIntegration === true"
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <ReportTNR v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportTNR === true"
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :currentID_STATUT="currentID_STATUT"
            />
            <v-alert
              v-if="alertMessage"
              icon='mdi-progress-clock'
              prominent
              text
              type="primary"
            >
              {{ alertMessage }}
            </v-alert>
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
import ReportPreControle from '../../components/ReportPreControle'
import ReportTNR from '../../components/ReportTNR'
import ReportLivraison from '../../components/ReportLivraison'
import ReportInitialisation from '../../components/ReportInitialisation'
import ReportPackagingDependances from '../../components/ReportPackagingDependances'
import ReportAnalyseData from '../../components/ReportAnalyseData'
import ReportAnalyseLogique from '../../components/ReportAnalyseLogique'
import ReportProcessusCritiques from '../../components/ReportProcessusCritiques'
import ReportIntegration from '../../components/ReportIntegration'
export default {
  components: { ReportTNR, ReportLivraison, ReportPreControle, ReportInitialisation, ReportPackagingDependances, ReportAnalyseData, ReportAnalyseLogique, ReportProcessusCritiques, ReportIntegration },
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
      alertMessage: '',
      alertIcon: 'mdi-information-outline',
      idInstanceWF: '',
      nbAvailablePack: 0,
      reportLivraison: false,
      reportTNR: false,
      reportPreControle: false,
      reportInitialisation: false,
      reportPackagingDependances: false,
      reportAnalyseData: false,
      reportAnalyseLogique: false,
      reportProcessusCritiques: false,
      reportIntegration: false,
      clientTaskName: ''
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
      this.GetNbAvailablePack()
    }
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
          if (response.data.GetReportResult.length > 0) {
            vm.JSON_TEMPLATE = JSON.parse(
              response.data.GetReportResult[0].JSON_TEMPLATE
            )
            vm.getWichReport(vm.JSON_TEMPLATE[0].name)
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
     * Check wich report.
     * @param {string} reportName - Name of report.
     */
    getWichReport(reportName) {
      this.setDefaultValue()
      switch (reportName) {
        case 'Pré contrôle des .mdb':
          this.reportPreControle = true
          break
        case 'Initialisation':
          this.reportInitialisation = true
          break
        case 'Packaging des dépendances':
          this.reportPackagingDependances = true
          break
        case 'Intégration':
          this.reportIntegration = true
          break
        case "Analyse d'impact sur les données":
          this.reportAnalyseData = true
          break
        case "Analyse d'impacte logique":
          this.reportAnalyseLogique = true
          break
        case "Tests des processus critiques":
          this.reportProcessusCritiques = true
          break
        case 'Tests de Non Régression (TNR)':
          this.reportTNR = true
          break
        case 'Livraison':
          this.reportLivraison = true
          break
        default:
          this.setDefaultValue()
          break
      }
    },

    setDefaultValue() {
      this.reportLivraison = false
      this.reportTNR = false
      this.reportPreControle = false
      this.reportInitialisation = false
      this.reportPackagingDependances = false
      this.reportAnalyseData = false
      this.reportAnalyseLogique = false
      this.reportProcessusCritiques = false
      this.reportIntegration = false
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

    /***
     * Get available packages from database
     */
    GetNbAvailablePack() {
      const vm = this
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/livraison/availablePack/` +
            vm.workflowID +
            `/` +
            vm.idInstanceWF +
            `/` +
            vm.clientId
        )
        .then(function(response) {
          if (response.status === 200) {
            vm.nbAvailablePack = response.data
          }
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} !`)
        })
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
