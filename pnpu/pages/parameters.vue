<template>
  <v-layout>
    <v-container row wrap>
      <v-flex md12>
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title class="title">
              {{ title }}
            </v-list-item-title>
            <v-list-item-subtitle>
              {{ subTitle }}
            </v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-flex>
      <v-flex md12>
        <v-divider class="my-2 mx-4" inset></v-divider>
        <v-col cols="4">
          <v-card>
            <v-card-title>{{ progress }}</v-card-title>

            <v-divider></v-divider>

            <v-card-actions>
              <v-btn color="primary" @click="downloadZip()">
                Télécharger
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-flex>
    </v-container>
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
  components: {},
  data: () => ({
    title: 'Paramètres',
    subTitle: 'Subtitle',
    snackbar: false,
    colorsnackbar: '',
    snackbarMessage: '',
    progress: ''
  }),

  methods: {
    /**
     * Download available zip file
     */
    downloadZip() {
      const vm = this
      axios({
        method: 'GET',
        url: `${process.env.WEB_SERVICE_WCF}/clients/livraison/1/24/101`,
        responseType: 'arraybuffer'
      })
        .then((response) => {
          if (response.status === 200) {
            const url = window.URL.createObjectURL(new Blob([response.data]))
            const link = document.createElement('a')
            link.href = url
            link.setAttribute('download', 'file.zip')
            document.body.appendChild(link)
            link.click()
          }
        })
        .catch(function(error) {
          vm.showSnackbar(
            'error',
            `${error} ! Impossible de récupérer les packages`
          )
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
