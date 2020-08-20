<template>
  <v-form ref="form" class="ma-6">
    <v-container class="fill-height" fluid>
      <v-data-table :headers="headers"
                    :items="items"
                    :items-per-page="6"
                    class="elevation-1 cursor mb-6"
                    @click:row="getReport($event)">
        <template v-slot:item.ID_STATUT="{ item }">
          <v-chip :color="getColor(item.ID_STATUT)" dark>
            {{
            item.ID_STATUT
            }}
          </v-chip>
        </template>
      </v-data-table>
    </v-container>
    <v-container class="fill-height" fluid>
      <v-stepper style="width=100%" v-model="e1" class="mb-6" @change="getSelectedStep($event)">
        <v-stepper-header>
          <template v-for="(step, idxStep) in steps">
            <v-stepper-step :key="idxStep"
                            :step="step.ORDER_ID"
                            :complete="step.ID_STATUT"
                            editable
                            :color="step.COLOR"
                            :edit-icon="step.ICON">
              {{ step.PROCESS_LABEL }}
            </v-stepper-step>
            <v-divider v-if="step !== steps" :key="idxStep"></v-divider>
          </template>
        </v-stepper-header>

        <v-stepper-items>
          <v-stepper-content v-for="(step, ixdContent) in steps"
                             :key="ixdContent"
                             :step="ixdContent">
            <ReportLivraison v-if="
                             Object.entries(JSON_TEMPLATE).length>
              0 && reportLivraison === true
              "
              :idPROCESS="currentID_PROCESS"
              :reportJsonData="JSON_TEMPLATE"
              :idInstanceWF="idInstanceWF"
              :workflowID="workflowID"
              :nbAvailablePack="nbAvailablePack"
              :currentID_STATUT="currentID_STATUT"
              :clientID="clientID"
              :clientName="clientName"
              />
              <ReportPreControle v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportPreControle === true"
                                 :idPROCESS="currentID_PROCESS"
                                 :reportJsonData="JSON_TEMPLATE"
                                 :idInstanceWF="idInstanceWF"
                                 :workflowID="workflowID"
                                 :currentID_STATUT="currentID_STATUT" />
              <ReportInitialisation v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportInitialisation === true"
                                    :idPROCESS="currentID_PROCESS"
                                    :reportJsonData="JSON_TEMPLATE"
                                    :idInstanceWF="idInstanceWF"
                                    :workflowID="workflowID"
                                    :currentID_STATUT="currentID_STATUT" />
              <ReportPackagingDependances v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportPackagingDependances === true"
                                          :idPROCESS="currentID_PROCESS"
                                          :reportJsonData="JSON_TEMPLATE"
                                          :idInstanceWF="idInstanceWF"
                                          :workflowID="workflowID"
                                          :currentID_STATUT="currentID_STATUT" />
              <ReportAnalyseData v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportAnalyseData === true"
                                 :idPROCESS="currentID_PROCESS"
                                 :reportJsonData="JSON_TEMPLATE"
                                 :idInstanceWF="idInstanceWF"
                                 :workflowID="workflowID"
                                 :currentID_STATUT="currentID_STATUT" />
              <ReportAnalyseLogique v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportAnalyseLogique === true"
                                    :idPROCESS="currentID_PROCESS"
                                    :reportJsonData="JSON_TEMPLATE"
                                    :idInstanceWF="idInstanceWF"
                                    :workflowID="workflowID"
                                    :currentID_STATUT="currentID_STATUT" />
              <ReportProcessusCritiques v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportProcessusCritiques === true"
                                        :idPROCESS="currentID_PROCESS"
                                        :reportJsonData="JSON_TEMPLATE"
                                        :idInstanceWF="idInstanceWF"
                                        :workflowID="workflowID"
                                        :currentID_STATUT="currentID_STATUT" />
              <ReportIntegration v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportIntegration === true"
                                 :idPROCESS="currentID_PROCESS"
                                 :reportJsonData="JSON_TEMPLATE"
                                 :idInstanceWF="idInstanceWF"
                                 :workflowID="workflowID"
                                 :currentID_STATUT="currentID_STATUT" />
              <ReportTNR v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportTNR === true"
                         :idPROCESS="currentID_PROCESS"
                         :reportJsonData="JSON_TEMPLATE"
                         :idInstanceWF="idInstanceWF"
                         :workflowID="workflowID"
                         :currentID_STATUT="currentID_STATUT" />
              <v-alert v-if="alertMessage"
                       icon='mdi-progress-clock'
                       prominent
                       text
                       type="primary">
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
  </v-form>
</template>
<script>
  import { mapGetters } from 'vuex'
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
    components: {
      ReportTNR,
      ReportLivraison,
      ReportPreControle,
      ReportInitialisation,
      ReportPackagingDependances,
      ReportAnalyseData,
      ReportAnalyseLogique,
      ReportProcessusCritiques,
      ReportIntegration
    },
    data() {
      return {
        headers: [
          {
            text: 'Workflow ID',
            align: 'start',
            value: 'WORKFLOW_ID'
          },
          {
            text: 'Instance ID',
            value: 'ID_H_WORKFLOW'
          },
          {
            text: 'Nom du client',
            value: 'CLIENT_NAME'
          },
          {
            text: 'Client ID',
            value: 'CLIENT_ID'
          },
          { text: "Nom de l'instance", value: 'INSTANCE_NAME' },
          { text: 'Nom du processus', value: 'PROCESS_LABEL' },
          { text: 'Process ID', value: 'ID_PROCESS' },
          { text: 'Lancé le', value: 'LAUNCHING_DATE' },
          { text: 'Statut', value: 'ID_STATUT' }
        ],
        e1: 1,
        steps: [],
        items: [],
        JSON_TEMPLATE: {},
        idPROCESS: '',
        idInstanceWF: '',
        workflowID: '',
        currentID_STATUT: '',
        reportName: '',
        currentID_PROCESS: '',
        nbAvailablePack: 0,
        clientName: '',
        clientID: '',
        snackbar: false,
        colorsnackbar: '',
        snackbarMessage: '',
        workflowDate: '',
        textStatus: '',
        alertMessage: '',
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
        checkbox: false,
        tableFiltered: [],
        checkboxValue: false
      }
    },
    computed: {
      /**
       * Get habilitation from vuex
       */
      ...mapGetters({
        user: 'modules/auth/user',
        profil: 'modules/auth/profil'
      })
    },

    created() {
      this.initialize()
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

    methods: {
      /**
       * Load informations for data table
       */
      async initialize() {
        try {
          const response = await axios.get(
            `${process.env.WEB_SERVICE_WCF}/toolbox/Dashboard/`,
            {
              params: {
                user: this.user,
                habilitation: this.profil
              }
            }
          )
          if (response.status === 200) {
            this.items = response.data.GetInfoLaunchToolBoxResult
          }
        } catch (error) {
          this.showSnackbar('error', `${error} !`) 
        }
      },

      /**
       * Get color according to workflow statut
       * @param {string} - workflow statut
       */
      getColor(statut) {
        if (statut === 'IN PROGRESS') return 'grey lighten-1'
        else if (statut === 'ERROR') return 'error'
        else if (statut === 'WARNING') return 'warning'
        else return 'success'
      },

      /**
       * Get report from database
       * @param {object} - Row selected
       */
      async getReport(row) {

        // Récupération des informations du client sélectionné au niveau du Dashboard
        this.idInstanceWF = row.ID_H_WORKFLOW
        this.workflowID = row.WORKFLOW_ID
        this.etape = row.CURRENT_ORDER_ID_PROCESS + 1
        this.e1 = row.CURRENT_ORDER_ID_PROCESS + 1
        this.workflowDate = ''
        this.textStatus = ''
        this.clientID = row.CLIENT_ID
        this.clientName = row.CLIENT_NAME
        this.getWorkflowProcesses()
        this.GetNbAvailablePack()
        
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
          //TBO Je veux systématiquement le dernier
          //vm.e1 = vm.steps.length
          //vm.currentID_PROCESS = vm.steps[vm.steps.length - 1].ID_PROCESS

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
          //vm.getReportFromDB()
        } catch (e) {
          return e
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
            vm.clientID
          )
          .then(function (response) {
            debugger
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
          .catch(function (error) {
            vm.showSnackbar('error', `${error} !`)
          })
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
            vm.clientID
          )
          .then(function (response) {
            debugger
            if (response.status === 200) {
              vm.nbAvailablePack = response.data
            }
          })
          .catch(function (error) {
            vm.showSnackbar('error', `${error} !`)
          })
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
          case 'Gestion des dépendances':
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
  .cursor {
    cursor: pointer;
  }

  .v-treeview-node__root {
    cursor: pointer !important;
  }

  .v-stepper {
      width : 100%;
      height : 100%;
  }

  .fade-enter-active,
  .fade-leave-active {
    transition: opacity 0.5s;
  }

  .fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
    opacity: 0;
  }
</style>
