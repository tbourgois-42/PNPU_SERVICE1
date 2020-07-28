<template>
  <v-form ref="form" @submit.prevent="validate">
    <v-container class="fill-height" fluid>
      <v-row align="center" justify="center">
        <v-col cols="12" sm="8" md="4">
          <v-card>
            <v-toolbar color="primary" dark flat>
              <v-toolbar-title>Se connecter</v-toolbar-title>
            </v-toolbar>
            <v-card-text>
              <v-form>
                <v-text-field
                  v-model="form.user"
                  :rules="[rules.required]"
                  label="Utilisateur"
                  name="login"
                  prepend-icon="mdi-account"
                  type="text"
                ></v-text-field>

                <v-text-field
                  :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                  v-model="form.password"
                  :rules="[rules.required]"
                  :type="showPassword ? 'text' : 'password'"
                  label="Mot de passe"
                  name="password"
                  prepend-icon="mdi-lock"
                  @click:append="showPassword = !showPassword"
                ></v-text-field>
              </v-form>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn
                :disabled="!formIsValid"
                color="primary"
                class="mr-4"
                type="submit"
                >Login</v-btn
              >
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>

<script>
import { mapActions } from 'vuex'
export default {
  data: () => ({
    form: {
      user: '',
      password: ''
    },
    showPassword: false,
    rules: {
      required: value => !!value || 'Champ obligatoire.'
    }
  }),

  computed: {
    formIsValid () {
      return (
        this.form.user && this.form.password
      )
    }
  },

  methods: {
    ...mapActions({
      signIn: 'modules/auth/signIn'
    }),
    validate() {
      this.signIn(this.form).then(() => {
        this.$router.push('/dashboard')
      })
    }
  }
}
</script>
