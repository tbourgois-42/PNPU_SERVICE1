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

      <!--<v-alert v-if="textStatus == 'En erreur'" type="error">
        Le process <strong>***</strong> a renvoyé une erreur de type
        <strong>***</strong>. Pour plus de détail veuillez vous référer aux
        données de Logs ci-dessous.
      </v-alert>
      <v-alert v-if="textStatus == 'Terminé'" type="success">
        Le workflow <strong>{{ workflowDate }}</strong> s'est déroulé avec
        succès ! Les rapports d'éxécutions sont disponibles ci-dessous.
      </v-alert>
      <v-alert v-if="textStatus == 'Manuel'" type="warning">
        Le process <strong>***</strong> nécesite l'intervention d'un utilisateur
        pour pouvoir passer à l'étape suivante.
      </v-alert>
      <v-alert v-if="textStatus == 'En cours'" type="info">
        Le workflow <strong>{{ workflowDate }}</strong> est en cours d'éxécution
        sur <strong>l'étape {{ etape }}</strong
        >.
      </v-alert>-->

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
                }}</v-card-subtitle>
              </v-card>
              <v-btn
                v-if="currentID_STATUT === 'mdi-hand'"
                depressed
                class="mr-4 mt-2 pr-4"
                color="warning"
                ><v-icon left>mdi-hand</v-icon> Valider le processus
              </v-btn>

              <v-btn depressed class="mr-4 mt-2 pr-4" color="primary">
                <v-icon left>mdi-file-excel-outline</v-icon> Exporter
              </v-btn>
            </v-col>
            <v-divider class="mx-4 mb-4"></v-divider>
            <Report v-if="e1 === 0" />
            <v-col v-else cols="12">
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
              <v-alert v-else icon="mdi-information-outline" text color="info"
                >Ce processus n'est pas terminé. Aucun rapport n'est disponible
                pour le moment</v-alert
              >
            </v-col>
          </v-stepper-content>
        </v-stepper-items>
      </v-stepper>
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
      currentID_STATUT: ''
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
      this.e1 = val
      this.steps.forEach((element, idx) => {
        if (idx === val) {
          this.currentID_STATUT = element.ICON
        }
      })
      console.log('currentID_STATUT', this.currentID_STATUT)
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
        console.log(this.steps)
      } catch (e) {
        return e
      }
    }
  }
}
</script>
