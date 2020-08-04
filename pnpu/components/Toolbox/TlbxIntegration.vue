<template>
  <v-form ref="form" @submit.prevent="launch">
    <v-container class="fill-height" fluid>
      <v-row>
        <v-col cols="12" sm="6" md="3">
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
  </v-form>
</template>
<script>
import axios from 'axios'
import aes from 'aes-js'
export default {
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
      }
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
        this.computedDateFormatted
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
      return `${month}/${day}/${year}`
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
      fd.append(
        'passwordBefore',
        aes.utils.utf8.toBytes(this.form.passwordBefore)
      )
      fd.append('serverAfter', this.form.serverAfter)
      fd.append('databaseAfter', this.form.databaseAfter)
      fd.append(
        'passwordAfter',
        aes.utils.utf8.toBytes(this.form.passwordAfter)
      )
      fd.append('dtPaie', this.computedDateFormatted)
      try {
        await axios.post(`${process.env.WEB_SERVICE_WCF}/toolbox/TNR`, fd)
      } catch (error) {
        console.log(error)
      }
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
