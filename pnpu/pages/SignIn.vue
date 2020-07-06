<template>
  <v-form v-model="valid" ref="form" @submit.prevent="validate">
    <v-container class="fill-height" fluid>
      <v-row align="center" justify="center">
        <v-col cols="12" sm="8" md="4">
          <v-alert
            v-if="showAlert"
            text
            prominent
            type="error"
            icon="mdi-account"
          >
            {{ message }}
          </v-alert>
          <v-card>
            <v-toolbar color="primary" dark flat>
              <v-toolbar-title>Se connecter</v-toolbar-title>
            </v-toolbar>
            <v-card-text>
              <v-form>
                <v-text-field
                  v-model="form.user"
                  :rules="[(v) => !!v || `Utilisateur est obligatoire`]"
                  label="Utilisateur"
                  name="login"
                  prepend-icon="mdi-account"
                  type="text"
                  required
                ></v-text-field>

                <v-text-field
                  v-model="form.password"
                  :rules="[(v) => !!v || `Mot de passe obligatoire`]"
                  label="Mot de passe"
                  name="password"
                  prepend-icon="mdi-lock"
                  type="password"
                  required
                ></v-text-field>
              </v-form>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn
                :disabled="!valid"
                color="primary"
                class="mr-4"
                type="submit"
                >Login</v-btn
              >
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
      <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
        {{ snackbarMessage }}
        <v-btn dark text @click="snackbar = false">
          Close
        </v-btn>
      </v-snackbar>
    </v-container>
  </v-form>
</template>

<script>
// import axios from 'axios'
import { mapActions } from 'vuex'
export default {
  components: {
    //
  },
  data: () => ({
    valid: true,
    form: {
      user: '',
      password: ''
    },
    snackbarMessage: '',
    snackbar: false,
    colorsnackbar: '',
    token: '',
    user: '',
    message: '',
    showAlert: false
  }),
  methods: {
    ...mapActions({
      signIn: 'modules/auth/signIn'
    }),
    validate() {
      this.signIn(this.form).then(() => {
        this.$router.push('/')
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
    },

    showAlertError(status) {
      switch (status) {
        case 401:
          this.message =
            "Le nom de l'utilisateur ou le mot de passe est incorrecte."
          break
        default:
          break
      }
      this.showAlert = true
    }
  }
}
</script>

<style></style>
