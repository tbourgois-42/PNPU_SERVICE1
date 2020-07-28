import Vue from 'vue'
import Vuetify from 'vuetify'
import 'vuetify/dist/vuetify.min.css'
import colors from 'vuetify/es5/util/colors'
import '@mdi/font/css/materialdesignicons.css'

Vue.use(Vuetify)

export default (ctx) => {
  const vuetify = new Vuetify({
    theme: {
      themes: {
        light: {
          primary: colors.blue.darken2,
          accent: colors.grey.darken3,
          secondary: colors.amber.darken3,
          info: colors.teal.lighten1,
          warning: colors.amber.base,
          error: colors.red.lighten1,
          success: colors.green.lighten1,
          Pantone171C : "#FF5C35",
          Pantone540C : "#002C52"
        },
        options: {
          themeCache: {
            get: (key) => localStorage.getItem(key),
            set: (key, value) => localStorage.setItem(key, value)
          }
        }
      }
    }
  })
  ctx.app.vuetify = vuetify
  ctx.$vuetify = vuetify.framework
}
