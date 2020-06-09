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
      <v-card>
        <v-card-title class="headline">Lancement du workflow</v-card-title>
        <v-card-subtitle class="mt-1">{{ workflowDate }}</v-card-subtitle>
        <v-divider></v-divider>
        <v-card-text>
          <v-container>
            <v-row>
              <v-col cols="12" sm="12" md="12">
                <v-select
                  v-model="txtWorkflow"
                  :items="lstWorkflows"
                  label="Workflow"
                  chips
                  solo
                  @input="getSelectedWorkflow()"
                ></v-select>
                <v-select
                  v-model="txtTypologie"
                  :items="typologie"
                  :rules="[verifyTypologie()]"
                  label="Typologie"
                  chips
                  multiple
                  solo
                ></v-select>
                <v-select
                  v-model="clientSelected"
                  :items="lstClient"
                  label="Client"
                  chips
                  multiple
                  solo
                ></v-select>
                <v-checkbox
                  v-model="packStandard"
                  label="Package standard"
                ></v-checkbox>
              </v-col>
              <v-col cols="12" sm="12" md="12">
                <v-file-input
                  v-model="files"
                  color="primary"
                  counter
                  label=".zip"
                  placeholder="Selection des fichiers"
                  prepend-icon="mdi-paperclip"
                  outlined
                  multiple
                  :show-size="1000"
                  accept=".zip, .7zip, .rar"
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
              </v-col>
            </v-row>
          </v-container>
        </v-card-text>

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
export default {
  props: {
    workflowDate: {
      type: String,
      default: ''
    },
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
    files: '',
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
    }
  }),

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
      if (event !== '') {
        event.forEach((element) => {
          this.selectedFile = element
        })
        if (this.selectedFile.type !== 'application/x-zip-compressed') {
          this.showSnackbar('error', 'Veuillez sélectionner un fichier .zip')
        }
      }
    },

    async sendFile() {
      this.loadingLaunchWorkflow = true
      const fd = new FormData()
      if (this.selectedFile !== null && this.txtTypologie !== -1) {
        fd.append('mdbFile', this.selectedFile, this.selectedFile.name)
        fd.append('typology', this.txtTypologie)
        fd.append('clients', this.clientSelected)
        fd.append('packStandard', this.packStandard)
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
    }
  }
}
</script>
