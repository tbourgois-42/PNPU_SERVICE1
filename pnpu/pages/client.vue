<template>
  <v-app>
    <v-container>
      <v-flex md12>
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title class="title">
              {{ client }}
            </v-list-item-title>
            <v-list-item-subtitle>
              Workflow {{ workflowDate }} | Step {{ etape }}
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
            <v-divider v-if="step !== steps" :key="step"></v-divider>
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
            <Report
              v-if="test.length > 0"
              :idPROCESS="currentID_PROCESS"
              :data="test"
            />
            <v-col else cols="12">
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
                v-else-if="currentID_STATUT === 'mdi-alert'"
                icon="mdi-information-outline"
                text
                color="error"
                >Le processsus a remonté des erreurs qui ont entrainés l'arrêt
                du Workflow. Pour plus d'information veuillez consulter le
                rapport d'éxecution ci-dessous</v-alert
              >
              <v-alert
                v-else-if="test.length === 0"
                icon="mdi-information-outline"
                text
                color="info"
                >Ce processus n'est pas terminé. Aucun rapport n'est disponible
                pour le moment</v-alert
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
import axios from 'axios'
import Report from '../components/Report.vue'
export default {
  components: { Report },
  data() {
    return {
      e1: 1,
      steps: [],
      client: '',
      etape: '',
      workflowDate: '',
      workflowID: '',
      textStatus: '',
      currentID_STATUT: '',
      currentID_PROCESS: '',
      snackbarMessage: '',
      snackbar: false,
      colorsnackbar: '',
      test: ''
    }
  },

  watch: {
    steps(val) {
      if (this.e1 > val) {
        this.e1 = val
      }
    }
  },

  created() {
    this.client = this.$route.params.client
    this.etape = this.$route.params.step
    this.e1 = this.$route.params.step
    this.workflowDate = this.$route.params.workflowDate
    this.workflowID = this.$route.params.workflowID
    this.textStatus = this.$route.params.textStatus
    if (this.$route.params.client === undefined) {
      return this.$nuxt.error({ statusCode: 404 })
    } else {
      this.getWorkflowProcesses()
    }
  },

  methods: {
    getSelectedStep(val) {
      const vm = this
      this.e1 = val
      this.steps.forEach((element, idx) => {
        if (idx === val) {
          vm.currentID_STATUT = element.ICON
          vm.currentID_PROCESS = element.ID_PROCESS
          this.getReportFromDB()
        }
      })
    },

    async getWorkflowProcesses() {
      try {
        const res = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/workflow/` +
            this.workflowID +
            `/processus`
        )
        this.steps = res.data.GetWorkflowProcessesResult
        for (let i = 0; i < this.e1; i++) {
          this.steps[i].ID_STATUT = 'COMPLETED'
          this.steps[i].ICON = 'mdi-check'
          this.steps[i].COLOR = 'light green'
        }
        if (this.textStatus === 'En erreur') {
          this.steps[this.e1 - 1].COLOR = 'error'
          this.steps[this.e1 - 1].ICON = 'mdi-alert'
          this.steps[this.e1 - 1].ID_STATUT = 'ERROR'
        } else if (this.textStatus === 'Manuel') {
          this.steps[this.e1 - 1].COLOR = 'warning'
          this.steps[this.e1 - 1].ICON = 'mdi-hand'
          this.steps[this.e1 - 1].ID_STATUT = 'WARNING'
        } else {
          this.steps[this.e1 - 1].COLOR = 'primary'
          this.steps[this.e1 - 1].ICON = 'mdi-pencil'
          this.steps[this.e1 - 1].ID_STATUT = 'IN PROGRESS'
        }
      } catch (e) {
        return e
      }
    },

    stopWorkflow() {
      if (
        confirm('Etes-vous sûr de bien vouloir stopper le workflow ?') === true
      ) {
        const vm = this
        axios
          .post(`${process.env.WEB_SERVICE_WCF}/Workflow/Client/Stop`, {
            WORKFLOW_ID: this.workflowID,
            CLIENT_ID: this.client
          })
          .then(function(response) {
            console.log(response)
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
            CLIENT_ID: this.client
          })
          .then(function(response) {
            console.log(response)
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

    getReportFromDB() {
      const vm = this
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/report/` +
            vm.workflowID +
            `/` +
            vm.currentID_PROCESS +
            `/` +
            vm.client
        )
        .then(function(response) {
          vm.test = response.data.getReportResult
          console.log(vm.test)
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} !`)
        })
    },

    showSnackbar(color, message) {
      this.snackbar = true
      this.colorsnackbar = color
      this.snackbarMessage = message
    }
  }
}
</script>
