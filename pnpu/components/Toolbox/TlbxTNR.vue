<template>
  <v-form ref="form" @submit.prevent="launch">
    <v-container class="fill-height" fluid>
      <v-row>
        <v-col cols="12" sm="6" md="5">
          <v-card>
            <v-card-title class="d-flex justify-space-between"
              >Base de données
              <v-chip text-color="white" color="Pantone540C" label
                ><v-icon left>mdi-label</v-icon>
                avant
              </v-chip></v-card-title
            >
            <v-divider></v-divider>
            <v-card-text>
              <v-col cols="12" sm="12" md="12">
                <v-text-field
                  v-model="form.serverBefore"
                  label="Serveur"
                  :rules="[rules.required]"
                ></v-text-field>
                <v-text-field
                  v-model="form.databaseBefore"
                  label="Database"
                  :rules="[rules.required]"
                ></v-text-field>
                <v-text-field
                  :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                  v-model="form.passwordBefore"
                  :rules="[rules.required]"
                  :type="showPassword ? 'text' : 'password'"
                  label="Mot de passe"
                  name="password"
                  @click:append="showPassword = !showPassword"
                ></v-text-field>
              </v-col>
            </v-card-text>
          </v-card>
        </v-col>
        <v-col cols="12" sm="6" md="5">
          <v-card>
            <v-card-title class="d-flex justify-space-between"
              >Base de données
              <v-chip text-color="white" color="Pantone171C" label
                ><v-icon left>mdi-label</v-icon>
                après
              </v-chip></v-card-title
            >
            <v-divider></v-divider>
            <v-card-text>
              <v-col cols="12" sm="12" md="12">
                <v-text-field
                  v-model="form.serverAfter"
                  label="Serveur"
                  :rules="[rules.required]"
                ></v-text-field>
                <v-text-field
                  v-model="form.databaseAfter"
                  label="Database"
                  :rules="[rules.required]"
                ></v-text-field>
                <v-text-field
                  :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                  v-model="form.passwordAfter"
                  :rules="[rules.required]"
                  :type="showPassword ? 'text' : 'password'"
                  label="Mot de passe"
                  name="password"
                  @click:append="showPassword = !showPassword"
                ></v-text-field>
              </v-col>
            </v-card-text>
          </v-card>
        </v-col>
        <v-col cols="12" sm="6" md="3">
          <v-card>
            <v-card-title class="d-flex justify-space-between"
              >Date de paiement<v-icon>mdi-calendar</v-icon></v-card-title
            >
            <v-divider></v-divider>
            <v-card-text>
              <v-menu
                v-model="menu2"
                :close-on-content-click="false"
                transition="scale-transition"
                offset-y
                max-width="290px"
                min-width="290px"
                :rules="[rules.required]"
              >
                <template v-slot:activator="{ on, attrs }">
                  <v-text-field
                    v-model="computedDateFormatted"
                    hint="Format MM/DD/YYYY"
                    persistent-hint
                    prepend-icon="mdi-calendar"
                    readonly
                    v-bind="attrs"
                    v-on="on"
                  ></v-text-field>
                </template>
                <v-date-picker
                  color="primary"
                  v-model="date"
                  no-title
                  @input="menu2 = false"
                ></v-date-picker>
              </v-menu>
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
import aes from 'aes-js'
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
      date: new Date().toISOString().substr(0, 10),
      dateFormatted: this.formatDate(new Date().toISOString().substr(0, 10)),
      menu1: false,
      menu2: false,
      form: {
        serverBefore: null,
        serverAfter: null,
        databaseBefore: null,
        databaseAfter: null,
        passwordBefore: null,
        passwordAfter: null
      },
      showPassword: false,
      rules: {
        required: (value) => !!value || 'Champ obligatoire.'
      },
      snackbar: '',
      colorsnackbar: '',
      snackbarMessage: ''
    }
  },
  computed: {
    computedDateFormatted() {
      return this.formatDate(this.date)
    },
    formIsValid() {
      return (
        this.form.serverBefore &&
        this.form.serverAfter &&
        this.form.databaseBefore &&
        this.form.databaseAfter &&
        this.form.passwordBefore &&
        this.form.passwordAfter &&
        this.computedDateFormatted &&
        this.client !== null
      )
    }
  },

  watch: {
    loader() {
      const l = this.loader
      this[l] = !this[l]

      setTimeout(() => (this[l] = false), 3000)

      this.loader = null
    },
    date(val) {
      this.dateFormatted = this.formatDate(this.date)
    }
  },
  methods: {
    formatDate(date) {
      if (!date) return null

      const [year, month, day] = date.split('-')
      return `${year}/${month}/${day}`
    },
    parseDate(date) {
      if (!date) return null

      const [month, day, year] = date.split('/')
      return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`
    },
    async launch() {
      const fd = new FormData()
      fd.append('serverBefore', this.form.serverBefore)
      fd.append('databaseBefore', this.form.databaseBefore)
      fd.append('passwordBefore', this.form.passwordBefore)
      fd.append('serverAfter', this.form.serverAfter)
      fd.append('databaseAfter', this.form.databaseAfter)
      fd.append('passwordAfter', this.form.passwordAfter)
      fd.append('dtPaie', this.computedDateFormatted)
      fd.append('clientID', this.client)
      fd.append('workflowID', this.workflowID)
      try {
        await axios.post(`${process.env.WEB_SERVICE_WCF}/toolbox`, fd)
      } catch (error) {
        vm.showSnackbar('error', `${error} !`)
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
