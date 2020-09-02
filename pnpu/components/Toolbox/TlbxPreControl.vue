<template>
  <v-form ref="form" @submit.prevent="launch">
    <v-container class="fill-height" fluid>
      <v-row>
        <v-col cols="12" sm="6" md="6">
          <v-card>
            <v-card-title class="d-flex justify-space-between"
              >Packages<v-icon>mdi-microsoft-access</v-icon></v-card-title
            >
            <v-divider></v-divider>
            <v-card-text>
              <v-file-input
                    ref="files"
                    append-icon="mdi-folder-zip-outline"
                    prepend-icon=""
                    v-model="files"
                    :rules="[
                      (files) => !!files || 'Le fichier zip est obligatoire.'
                    ]"
                    color="primary"
                    counter
                    label="Fichier .zip *"
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
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>

      <v-col cols="2">
        <v-btn
          class="ma-2"
          :loading="loading"
          :disabled="!formIsValid"
          color="primary"
          @click="loader = 'loading'"
          type="submit"
        >
          <v-icon left>mdi-play</v-icon>Executer
          <template v-slot:loader>
            <span class="custom-loader">
              <v-icon light>mdi-loading</v-icon>
            </span>
          </template>
        </v-btn>
      </v-col>
    </v-container>
    <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
      {{ snackbarMessage }}
      <v-btn dark text @click="snackbar = false">
        Close
      </v-btn>
    </v-snackbar>
  </v-form>
</template>
<script>
import axios from 'axios'
export default {
  props: {
    client: {
      type: Number,
      default: null
    },
    workflowID: {
      type: Number,
      default: null
    }
  },
  data() {
    return {
      loader: null,
      loading: false,
      rules: {
        required: (value) => !!value || 'Champ obligatoire.'
      },
      selectedFile: null,
      files: [],
      snackbar: '',
      colorsnackbar: '',
      snackbarMessage: ''
    }
  },
  computed: {
    formIsValid() {
      return (
        this.files.length > 0 &&
        this.client !== ''
      )
    }
  },

  watch: {
    loader() {
      const l = this.loader
      this[l] = !this[l]

      setTimeout(() => (this[l] = false), 3000)

      this.loader = null
    }
  },
  methods: {

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

    async launch() {
      const fd = new FormData()
      fd.append('mdbFile', this.selectedFile, this.selectedFile.name)
      fd.append('clientID', this.client)
      fd.append('workflowID', this.workflowID)
      try {
        await axios.post(`${process.env.WEB_SERVICE_WCF}/toolbox`, fd)
      } catch (error) {
        this.showSnackbar('error', `${error.response.data}`)
      }
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
<style>
.custom-loader {
  animation: loader 1s infinite;
  display: flex;
}
@-moz-keyframes loader {
  from {
    transform: rotate(0);
  }
  to {
    transform: rotate(360deg);
  }
}
@-webkit-keyframes loader {
  from {
    transform: rotate(0);
  }
  to {
    transform: rotate(360deg);
  }
}
@-o-keyframes loader {
  from {
    transform: rotate(0);
  }
  to {
    transform: rotate(360deg);
  }
}
@keyframes loader {
  from {
    transform: rotate(0);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
