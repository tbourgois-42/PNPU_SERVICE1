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
    </v-container>

    <v-alert v-if="textStatus == 'Erreur'" type="error">
      Le process <strong>***</strong> a renvoyé une erreur de type
      <strong>***</strong>. Pour plus de détail veuillez vous référer aux
      données de Logs ci-dessous.
    </v-alert>
    <v-alert v-if="textStatus == 'Terminé'" type="success">
      Le workflow <strong>{{ workflowDate }}</strong> s'est déroulé avec succès
      ! Les rapports d'éxécutions sont disponibles ci-dessous.
    </v-alert>
    <v-alert v-if="textStatus == 'Manuel'" type="warning">
      Le process <strong>***</strong> nécesite l'intervention d'un utilisateur
      pour pouvoir passer à l'étape suivante.
    </v-alert>
    <v-alert v-if="textStatus == 'En cours'" type="info">
      Le workflow <strong>{{ workflowDate }}</strong> est en cours d'éxécution
      sur <strong>l'étape {{ etape }}</strong
      >.
    </v-alert>

    <v-stepper v-model="e1" class="mt-3">
      <v-stepper-header>
        <template v-for="step in steps">
          <v-stepper-step
            :key="`${step}-step`"
            :step="step.etape"
            :complete="step.completed"
            editable
            editIcon="mdi-check"
            :color="step.color"
          >
            {{ step.name }}
          </v-stepper-step>
          <v-divider v-if="step !== steps" :key="step"></v-divider>
        </template>
      </v-stepper-header>

      <v-stepper-items>
        <v-stepper-content
          v-for="step in steps"
          :key="`${step}-step`"
          :step="step.etape"
        >
          <v-row cols="12" flex-wrap>
            <v-col md="6">
              <v-card class="ml-1">
                <v-container>
                  <v-subheader>Logs</v-subheader>
                  <v-container>
                    <p class="text-justify caption">
                      {{ step.content }}
                    </p>
                  </v-container>
                </v-container>
              </v-card>
            </v-col>
            <v-col md="6">
              <v-card class="mr-1">
                <v-container>
                  <v-subheader>Report</v-subheader>
                  <v-container>
                    <p class="text-justify caption">
                      {{ step.content }}
                    </p>
                  </v-container>
                </v-container>
              </v-card>
            </v-col>
          </v-row>
        </v-stepper-content>
      </v-stepper-items>
    </v-stepper>
  </v-app>
</template>
<script>
export default {
  data() {
    return {
      select: ['Workflow', 'Programming'],
      items: ['Programming', 'Design', 'Vue', 'Vuetify'],
      e1: 1,
      steps: [
        {
          name: 'Pré-contrôle MDB',
          etape: '1',
          completed: false,
          msgError: false,
          color: '',
          icon: '',
          content: 'lorem '
        },
        {
          name: 'Initialisation',
          etape: '2',
          completed: false,
          msgError: false,
          color: '',
          icon: ''
        },
        {
          name: "Analyse d'impact",
          etape: '3',
          completed: false,
          msgError: false,
          color: '',
          icon: ''
        },
        {
          name: 'Gestion dépendances',
          etape: '4',
          completed: false,
          msgError: false,
          color: '',
          icon: ''
        },
        {
          name: 'Tests intégration',
          etape: '5',
          completed: false,
          msgError: false,
          color: '',
          icon: ''
        },
        {
          name: 'Test processus critiques',
          etape: '6',
          completed: false,
          msgError: false,
          color: '',
          icon: ''
        },
        {
          name: 'TNR Standard',
          etape: '7',
          completed: false,
          msgError: false,
          color: '',
          icon: ''
        },
        {
          name: 'Livraison',
          etape: '8',
          completed: false,
          msgError: false,
          color: '',
          icon: ''
        }
      ],
      client: '',
      etape: '',
      workflowDate: '',
      textStatus: ''
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
    this.workflowDate = this.$route.params.workflowDate
    this.textStatus = this.$route.params.textStatus
    this.e1 = this.$route.params.step
    for (let i = 0; i < this.e1; i++) {
      this.steps[i].completed = true
      this.steps[i].color = 'light green'
    }
    if (this.textStatus === 'Erreur') {
      this.steps[this.e1 - 1].color = 'error'
      this.steps[this.e1 - 1].icon = 'mdi-alert'
    } else {
      this.steps[this.e1 - 1].color = 'primary'
    }
  },

  methods: {
    onInput(val) {
      this.steps = parseInt(val)
    },
    nextStep(step) {
      if (step === this.steps) {
        this.e1 = 1
      } else {
        this.e1 = step + 1
      }
    }
  }
}
</script>
