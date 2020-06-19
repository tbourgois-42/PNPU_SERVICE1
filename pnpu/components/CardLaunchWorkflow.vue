<template>
  <v-layout>
    <v-card class="mb-4 mt-3" min-width="277">
      <v-card-title class="d-flex justify-space-between subtitle-1"
        >Lancement workflow<v-icon>mdi-cog-box</v-icon></v-card-title
      >
      <v-divider class="mx-4"></v-divider>
      <v-btn class="my-4 ml-6" color="primary" @click="dialog = !dialog">
        <v-icon left>mdi-play</v-icon>Lancer un workflow
      </v-btn>
    </v-card>
    <!-- Lancer un process -->
    <v-dialog v-model="dialog" max-width="600px">
      <v-card ref="form">
        <v-card-title class="d-flex justify-space-between headline pt-6 pb-6"
          >Lancement du workflow<v-icon class="mr-2 pr-1"
            >mdi-play-circle</v-icon
          ></v-card-title
        >
        <v-divider></v-divider>
        <v-card-text>
          <v-container>
            <v-row>
              <v-col cols="12" sm="12" md="12">
                <v-card class="d-flex justify-space-between align-center" flat>
                  <v-select
                    ref="txtWorkflow"
                    v-model="txtWorkflow"
                    :rules="[
                      () => !!txtWorkflow || 'Le workflow est obligatoire.'
                    ]"
                    :items="lstWorkflows"
                    label="Workflow *"
                    chips
                    solo
                    @input="getSelectedWorkflow()"
                    required
                  ></v-select>
                  <iconTooltip
                    text="Sélection du workflow qui sera instancié."
                  />
                </v-card>
                <v-card class="d-flex justify-space-between" flat>
                  <v-select
                    ref="txtTypologie"
                    v-model="txtTypologie"
                    :items="typologie"
                    :rules="[verifyTypologie()]"
                    label="Typologie *"
                    multiple
                    chips
                    required
                    solo
                  ></v-select>
                  <iconTooltip
                    text="Typologie(s) de client(s) concernée(s) par le workflow. Il n'est pas possible de sélectionner la typologie SaaS Dédié avec une autre typologie. Si vous souhaitez instancier cette typologie en plus d'une autre il vous faudra réaliser deux instances distinctes."
                  />
                </v-card>
                <v-card class="d-flex justify-space-between" flat>
                  <v-select
                    ref="clientSelected"
                    v-model="clientSelected"
                    :items="lstClient"
                    label="Client"
                    chips
                    multiple
                    solo
                  ></v-select>
                  <iconTooltip
                    text="Si aucun client n'est sélectionné, l'instance du workflow lancé prendra en compte l'ensemble des clients pour la/les typologie(s) sélectionnée(s)."
                  />
                </v-card>
                <v-card class="d-flex justify-space-between" flat>
                  <v-text-field
                    ref="txtInstanceName"
                    v-model="txtInstanceName"
                    :counter="255"
                    :rules="[
                      (value) =>
                        !!value || `Le nom de l'instance est obligatoire.`,
                      (value) =>
                        (value && value.length <= 255) ||
                        `Le nom de l'instance doit contenir moins de 255 caractères`
                    ]"
                    label="Nom de l'instance *"
                    required
                    solo
                  ></v-text-field>
                  <iconTooltip
                    text="A chaque lancement d'un workflow celui-ci est instancié. Le nom de l'instance permet de différencier un workflow d'un autre."
                  />
                </v-card>
                <v-card class="d-flex justify-space-between" flat>
                  <v-file-input
                    ref="files"
                    v-model="files"
                    :rules="[
                      (files) => !!files || 'Le fichier zip est obligatoire.'
                    ]"
                    color="primary"
                    counter
                    label="Fichier .zip *"
                    prepend-icon=""
                    solo
                    multiple
                    :show-size="1000"
                    accept=".zip, .7zip, .rar"
                    required
                    @change="selectFile($event)"
                  >
                    <template v-slot:selection="{ index, text }">
                      <v-chip v-if="index < 2" color="primary" dark label small>
                        {{ text }}
                      </v-chip>

                      <span
                        v-else-if="index === 2"
                        class="overline grey--text text--darken-3 mx-2"
                      >
                        +{{ files.length - 2 }} Fichier(s)
                      </span>
                    </template>
                  </v-file-input>
                  <iconTooltip
                    text="La sélection doit contenir un fichier zippé contenant le/les fichier(s) .mdb"
                  />
                </v-card>
                <v-card class="d-flex justify-space-between align-center" flat>
                  <v-checkbox
                    ref="packStandard"
                    v-model="packStandard"
                    label="Package standard *"
                  ></v-checkbox>
                  <iconTooltip
                    margingPadding=""
                    text="La case à cocher 'Package standard' doit être cochée si l'instance du workflow concerne un package standard. Cela a pour objectif de lancer des contrôles spécialement conçu pour ce type de pack."
                  />
                </v-card>
                <v-card-text class="ma-0 pa-0">
                  (*) Champs obligatoires
                </v-card-text>
              </v-col>
            </v-row>
          </v-container>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="error" @click="close"
            ><v-icon left>mdi-cancel</v-icon> Annuler</v-btn
          >
          <v-btn
            :loading="loadingLaunchWorkflow"
            color="primary"
            :disabled="launchWorkflow"
            @click="sendFile()"
            ><v-icon left>mdi-play</v-icon> Lancer</v-btn
          >
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
      {{ snackbarMessage }}
      <v-btn dark text @click="snackbar = false">
        Close
      </v-btn>
    </v-snackbar>
  </v-layout>
</template>
<script>
import axios from 'axios'
import iconTooltip from '../components/IconTooltip'
export default {
  components: { iconTooltip },
  props: {
    clients: {
      type: Array,
      default: () => []
    },
    typologie: {
      type: Array,
      default: () => []
    },
    workflowID: {
      type: String,
      default: ''
    }
  },
  data: () => ({
    txtTypologie: [],
    idTypologie: '',
    lstClient: [],
    lstClientHidden: [],
    clientsTypo: [],
    files: [],
    launchWorkflow: false,
    dialog: false,
    txtWorkflow: '',
    workflows: [],
    lstSelectedClient: [],
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
    selectedFile: null,
    lstWorkflows: [],
    idSelectedWorkflow: '',
    packStandard: true,
    loadingLaunchWorkflow: false,
    txtInstanceName: '',
    formHasErrors: false,
    clientSelected: [],
    /**
     * Rules - Contrôle sur la sélection des typologies.
     */
    verifyTypologie() {
      let saasDedieSelect = false
      this.txtTypologie.forEach((idTypo) => {
        if (idTypo === 256) {
          saasDedieSelect = true
        }
      })
      if (this.txtTypologie.length > 1 && saasDedieSelect) {
        this.launchWorkflow = true
        return 'Impossible de sélectionner SaaS Dédié avec une autre typologie'
      } else {
        this.launchWorkflow = false
      }
      if (this.txtTypologie.length === 0) {
        return 'La typologie est obligatoire.'
      }
    }
  }),

  computed: {
    form() {
      return {
        txtWorkflow: this.txtWorkflow,
        txtTypologie: this.txtTypologie,
        clientSelected: this.clientSelected,
        txtInstanceName: this.txtInstanceName,
        packStandard: this.packStandard,
        files: this.files
      }
    }
  },

  watch: {
    txtTypologie() {
      this.lstClient = []
      for (const client of this.clientsTypo) {
        for (const idTypo of this.txtTypologie) {
          if (idTypo.toString() === client.TYPOLOGY_ID) {
            this.lstClient.push({
              value: client.ID_CLIENT,
              text: client.CLIENT_NAME
            })
          }
        }
      }
    }
  },

  created() {
    this.initialize()
  },

  methods: {
    close() {
      this.dialog = false
    },

    selectFile(event) {
      if (event.length > 0) {
        event.forEach((element) => {
          this.selectedFile = element
        })
        if (this.selectedFile.type !== 'application/x-zip-compressed') {
          this.files = []
          this.showSnackbar('error', 'Veuillez sélectionner un fichier .zip')
        }
      }
    },

    check() {
      return false
    },

    async sendFile() {
      this.formHasErrors = false
      Object.keys(this.form).forEach((f) => {
        if (!this.form[f]) this.formHasErrors = true

        this.$refs[f].validate(true)
      })
      this.loadingLaunchWorkflow = true
      const fd = new FormData()
      if (
        this.selectedFile !== null &&
        this.txtTypologie !== -1 &&
        this.txtInstanceName !== null
      ) {
        fd.append('mdbFile', this.selectedFile, this.selectedFile.name)
        fd.append('typology', this.txtTypologie)
        fd.append('clients', this.clientSelected)
        fd.append('packStandard', this.packStandard)
        fd.append('instanceName', this.txtInstanceName)
        try {
          await axios.post(
            `${process.env.WEB_SERVICE_WCF}/worflow/` +
              this.idSelectedWorkflow +
              `/uploadFile`,

            fd
          )
          this.loadingLaunchWorkflow = false
          this.showSnackbar('success', 'Lancement effectué avec succès')
          this.close()
        } catch (error) {
          this.showSnackbar('error', `${error} !`)
        }
      } else if (this.selectedFile === null) {
        this.showSnackbar(
          'error',
          'Impossible de lancer un workflow sans sélectionner de fichier'
        )
      } else if (this.txtTypologie === -1) {
        this.showSnackbar(
          'error',
          'Impossible de lancer un workflow sans sélectionner une typologie'
        )
      }
    },

    async initialize() {
      const vm = this
      try {
        const res = await axios.get(`${process.env.WEB_SERVICE_WCF}/workflow`)
        this.workflows = res.data.GetAllWorkFLowResult
        this.workflows.forEach((element) => {
          this.lstWorkflows.push(element.WORKFLOW_LABEL)
        })
      } catch (error) {
        vm.showSnackbar('error', `${error} !`)
      }

      try {
        const res2 = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/clientsByTypo`
        )
        vm.clientsTypo = res2.data
        /* vm.clients.forEach((element) => {
          vm.lstClientHidden.push(element.CLIENT_NAME)
        }) */
      } catch (error) {
        vm.showSnackbar('error', `${error} !`)
      }
    },

    getSelectedWorkflow() {
      this.workflows.forEach((element) => {
        if (element.WORKFLOW_LABEL === this.txtWorkflow) {
          this.idSelectedWorkflow = element.WORKFLOW_ID
        }
      })
    },

    showSnackbar(color, message) {
      this.snackbar = true
      this.colorsnackbar = color
      this.snackbarMessage = message
      this.loadingLaunchWorkflow = false
    }
  }
}
</script>
