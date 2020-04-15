<template>
  <v-layout>
    <v-card class="mb-4 mt-3" min-width="277">
      <v-card-title class="d-flex justify-space-between subtitle-1"
        >Lancement workflow<v-icon>mdi-cog-box</v-icon></v-card-title
      >
      <v-divider class="mx-4"></v-divider>
      <v-btn
        class="my-4 ml-6"
        color="primary"
        @click="dialog = !dialog"
        v-on="on"
      >
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
                  v-model="txtTypologie"
                  :items="typologie"
                  :rules="[verifyTypologie()]"
                  label="Typologie"
                  chips
                  multiple
                  solo
                ></v-select>
                <v-select
                  :items="lstClient"
                  label="Client"
                  chips
                  multiple
                  solo
                ></v-select>
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
                  :rules="controleAcceptFile()"
                  @change="selectFile"
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
          <v-btn color="primary" :disabled="launchWorkflow" @click="sendFile()"
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
    }
  },
  data: () => ({
    txtTypologie: '',
    lstClient: [],
    files: '',
    launchWorkflow: false,
    dialog: false,
    controleAcceptFile() {
      if (this.selectedFile !== null) {
        if (this.selectedFile.type !== 'application/x-zip-compressed') {
          this.launchWorkflow = true
          this.showSnackbar('error', 'Veuillez sélectionner un fichier .zip')
          return ' '
        } else {
          this.launchWorkflow = false
        }
      }
    },
    verifyTypologie() {
      if (
        this.txtTypologie.length > 1 &&
        this.txtTypologie.includes('SaaS Dédié')
      ) {
        this.launchWorkflow = true
        return 'Impossible de sélectionner SaaS Dédié avec une autre typologie'
      } else {
        this.launchWorkflow = false
      }
    },
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
    selectedFile: null
  }),

  watch: {
    txtTypologie() {
      this.lstClient = []
      this.clients.forEach((c) => {
        this.txtTypologie.forEach((t) => {
          if (t.replace(/é/g, 'e').toUpperCase() === c.TYPOLOGY) {
            this.lstClient.push(c.CLIENT_ID)
          }
        })
      })
    }
  },

  methods: {
    close() {
      this.dialog = false
    },

    selectFile(event) {
      event.forEach((element) => {
        this.selectedFile = element
      })
    },

    async sendFile() {
      const fd = new FormData()
      if (this.selectedFile !== null) {
        fd.append('mdbFile', this.selectedFile, this.selectedFile.name)
        try {
          await axios.post(
            `${process.env.WEB_SERVICE_WCF}/worflow/1/uploadFile`,
            fd
          )
          this.showSnackbar('success', 'Lancement effectué avec succès')
          this.close()
        } catch (error) {
          this.showSnackbar('error', `${error} !`)
        }
      } else {
        this.showSnackbar(
          'error',
          'Impossible de lancer un workflow sans sélectionner de fichier'
        )
      }
    },

    showSnackbar(color, message) {
      this.snackbar = true
      this.colorsnackbar = color
      this.snackbarMessage = message
    }
  }
}
</script>
