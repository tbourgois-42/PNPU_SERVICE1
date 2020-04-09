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
                  v-model="txtClient"
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
                  label=".mdb .zip"
                  multiple
                  placeholder="Selection des fichiers"
                  prepend-icon="mdi-paperclip"
                  outlined
                  :show-size="1000"
                  accept=".zip, .mdb, .7zip, .rar"
                  :rules="[controleAcceptFile()]"
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
              <input type="file" @change="selectFile" />
            </v-row>
          </v-container>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="error" @click="close"
            ><v-icon left>mdi-cancel</v-icon> Annuler</v-btn
          >
          <v-btn color="primary" @click="sendFile()" :disabled="launchWorkflow"
            ><v-icon left>mdi-play</v-icon> Lancer</v-btn
          >
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-snackbar v-model="snackbar" color="success" :timeout="6000" top>
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
  props: ['workflowDate', 'Clients', 'typologie'],
  data: () => ({
    txtTypologie: '',
    lstClient: [],
    files: '',
    launchWorkflow: false,
    dialog: false,
    verifyTypologie() {
      if (
        this.txtTypologie.length > 1 &&
        this.txtTypologie.includes('SaaS Dédié')
      ) {
        this.launchWorkflow = true
        return 'Impossible de sélectionner SaaS Dédié avec une autre typologie'
      } else if (
        this.txtTypologie.length === 1 &&
        this.txtTypologie.includes('SaaS Dédié')
      ) {
        this.launchWorkflow = false
        this.Clients.forEach((element) => {
          if (this.lstClient.includes(element.client) === false) {
            if (
              this.txtTypologie === 'SaaS dédié' &&
              this.txtTypologie.includes('SaaS Dédié')
            ) {
              this.lstClient.push(element.client)
            }
          }
        })
        console.log(this.lstClient)
        console.log(this.typologie)
      }
    },
    controleAcceptFile() {},
    snackbarMessage: '',
    snackbar: false,
    file: '',
    selectedFile: null
  }),

  methods: {
    close() {
      this.dialog = false
    },

    selectFile(event) {
      console.log(event.target.files[0])
      this.selectedFile = event.target.files[0]
    },

    async sendFile() {
      const fd = new FormData()
      fd.append('mdbFile', this.selectedFile, this.selectedFile.name)
      try {
        await axios.post(
          'http://localhost:63267/Service1.svc/worflow/1/uploadFile',
          fd,
          {
            onUploadProgress: (uploadEvent) => {
              console.log(
                'Upload progress: ' +
                  Math.round((uploadEvent.loaded / uploadEvent.total) * 100) +
                  '%'
              )
            }
          }
        )
      } catch (err) {
        console.log(err)
      }
    }
  }
}
</script>
