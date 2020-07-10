<template>
  <v-app>
    <v-navigation-drawer
      v-if="authenticated"
      v-model="drawer"
      :mini-variant="miniVariant"
      :clipped="clipped"
      fixed
      app
      dark
    >
      <v-list-item>
        <v-list-item-content>
          <v-list-item-title class="title font-weight-medium">
            Bonjour, {{ user }}
          </v-list-item-title>
          <v-chip class="mt-2 mb-2" color="primary" label>
            <v-icon left>mdi-account</v-icon>
            {{ profil }}
          </v-chip>
        </v-list-item-content>
      </v-list-item>
      <v-divider class="mx-4"></v-divider>
      <v-list dense>
        <template v-for="item in items">
          <v-row v-if="item.heading" :key="item.heading" align="center">
            <v-col cols="6">
              <v-subheader v-if="item.heading">
                {{ item.heading }}
              </v-subheader>
            </v-col>
            <v-col cols="6" class="text-center">
              <a href="#!" class="body-2 black--text">EDIT</a>
            </v-col>
          </v-row>
          <v-list-group
            v-else-if="item.children"
            :key="item.text"
            v-model="item.model"
            :prepend-icon="item.model ? item.icon : item['icon-alt']"
            append-icon=""
          >
            <template v-slot:activator>
              <v-list-item-content>
                <v-list-item-title>
                  {{ item.text }}
                </v-list-item-title>
              </v-list-item-content>
            </template>
            <v-list-item
              v-for="(child, i) in item.children"
              :key="i"
              :to="child.to"
              router
              exact
            >
              <v-list-item-action v-if="child.icon">
                <v-icon>{{ child.icon }}</v-icon>
              </v-list-item-action>
              <v-list-item-content>
                <v-list-item-title>
                  {{ child.text }}
                </v-list-item-title>
              </v-list-item-content>
            </v-list-item>
          </v-list-group>
          <v-list-item v-else :key="item.text" :to="item.to" router exact>
            <v-list-item-action>
              <v-icon>{{ item.icon }}</v-icon>
            </v-list-item-action>
            <v-list-item-content>
              <v-list-item-title>
                {{ item.text }}
              </v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </template>
      </v-list>
    </v-navigation-drawer>
    <v-app-bar :clipped-left="clipped" app flat dark dense color="primary">
      <v-app-bar-nav-icon v-if="authenticated" @click.stop="drawer = !drawer" />
      <v-btn text to="/">
        <v-toolbar-title class="title"
          >PeopleNet Platform Update</v-toolbar-title
        >
      </v-btn>
      <v-spacer />
      <v-btn
        v-if="authenticated"
        @click.prevent="signOut"
        text
        class="ma-2"
        to="/logout"
      >
        <v-icon left>mdi-logout</v-icon> Se déconnecter
      </v-btn>
      <v-btn v-if="!authenticated" text class="ma-2" to="/signin">
        <v-icon left>mdi-account</v-icon> Se connecter
      </v-btn>
    </v-app-bar>
    <v-content>
      <v-container fluid>
        <nuxt />
      </v-container>
    </v-content>
    <v-footer :fixed="fixed" app dark dense color="primary">
      <span>&copy; {{ new Date().getFullYear() }} - Cegid</span>
    </v-footer>
  </v-app>
</template>

<script>
import { mapGetters, mapActions } from 'vuex'
export default {
  data() {
    return {
      clipped: false,
      drawer: false,
      fixed: false,
      items: [
        {
          icon: 'mdi-view-dashboard',
          text: 'Dashboard',
          to: '/'
        },
        {
          icon: 'mdi-sitemap',
          text: 'Workflow',
          to: '/workflow'
        },
        {
          icon: 'mdi-chart-bubble',
          text: 'Parameters',
          to: '/parameters'
        },
        {
          icon: 'mdi-comment-quote',
          text: 'Feedback',
          to: '/feedback'
        },
        {
          icon: 'mdi-file-chart',
          text: 'Report',
          to: '/report'
        },
        {
          icon: 'mdi-chart-bubble',
          text: 'Toolbox',
          to: '/toolbox'
        },
        {
          icon: 'mdi-microsoft-access',
          text: 'HF1213',
          to: '/hotfix/HF1213'
        },
        {
          icon: 'mdi-microsoft-access',
          text: 'HF1214',
          to: '/hotfix/HF1214'
        },
        {
          icon: 'mdi-microsoft-access',
          text: 'HF1215',
          to: '/hotfix/HF1215'
        },
        {
          icon: 'mdi-chevron-up',
          'icon-alt': 'mdi-chevron-down',
          text: 'Exemple Rapport (Alpha)',
          model: false,
          children: [
            {
              icon: 'mdi-file-chart',
              text: 'TNR',
              to: '/reportAlpha/rapportTNR'
            }
          ]
        },
        {
          icon: 'mdi-microsoft-access',
          text: 'Livraison',
          to: '/rapportLivraison'
        },
        {
          icon: 'mdi-login',
          text: 'Login',
          to: '/signin'
        }
      ],
      miniVariant: false,
      right: true,
      rightDrawer: false,
      title: 'PeopleNet Platform Update'
    }
  },

  computed: {
    ...mapGetters({
      authenticated: 'modules/auth/authenticated',
      user: 'modules/auth/user',
      profil: 'modules/auth/profil'
    })
  },

  methods: {
    ...mapActions({
      signOutAction: 'modules/auth/signOut'
    }),

    signOut() {
      this.signOutAction().then(() => {
        this.$router.push('/signin')
      })
    }
  }
}
</script>

<style lang="css" scoped>
.v-application .title {
  font-size: 1rem !important;
}

.v-navigation-drawer .v-icon.v-icon {
  font-size: 24px;
}
</style>
