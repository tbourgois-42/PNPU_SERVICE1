<template>
  <v-app>
    <v-navigation-drawer v-model="drawer"
                         :mini-variant="miniVariant"
                         :clipped="clipped"
                         fixed
                         app
                         dark>
      <v-list-item>
        <v-list-item-content>
          <v-list-item-title class="title font-weight-medium">
            PeopleNet Platform Update
          </v-list-item-title>
        </v-list-item-content>
      </v-list-item>
      <v-divider class="mx-4"></v-divider>
      <v-list>
        <v-list-item v-for="(item, i) in items"
                     :key="i"
                     :to="item.to"
                     router
                     exact>
          <v-list-item-action>
            <v-icon>{{ item.icon }}</v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title class="body-2 font-weight-light"
                               v-text="item.title" />
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>
    <v-app-bar :clipped-left="clipped" app flat dark color="primary">
      <v-app-bar-nav-icon @click.stop="drawer = !drawer" />
      <v-btn text to="/">
        <v-toolbar-title class="title">PeopleNet Platform Update</v-toolbar-title>
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
    <v-footer :fixed="fixed" app dark color="primary">
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
            title: 'Dashboard',
            to: '/'
          },
          {
            icon: 'mdi-sitemap',
            title: 'Workflow',
            to: '/workflow'
          },
          {
            icon: 'mdi-chart-bubble',
            title: 'Parameters',
            to: '/parameters'
          },
          {
            icon: 'mdi-comment-quote',
            title: 'Feedback',
            to: '/feedback'
          },
          {
            icon: 'mdi-file-chart',
            title: 'Report',
            to: '/report'
          },
          {
            icon: 'mdi-chart-bubble',
            title: 'Toolbox',
            to: '/toolbox'
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
