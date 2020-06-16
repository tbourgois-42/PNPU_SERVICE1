<template>
  <v-app>
    <v-navigation-drawer
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
            PeopleNet Platform Update
          </v-list-item-title>
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
      <v-app-bar-nav-icon @click.stop="drawer = !drawer" />
      <v-btn text to="/">
        <v-toolbar-title class="title"
          >PeopleNet Platform Update</v-toolbar-title
        >
      </v-btn>
      <v-spacer />
      <v-btn icon>
        <v-icon>mdi-account</v-icon>
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
        }
      ],
      miniVariant: false,
      right: true,
      rightDrawer: false,
      title: 'PeopleNet Platform Update'
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
