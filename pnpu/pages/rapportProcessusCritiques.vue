<template>
  <v-app>
    <v-container>
      <v-row>
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
          <ReportProcessusCritiques />
        </v-flex>
      </v-row>
    </v-container>
  </v-app>
</template>

<script>
import ReportProcessusCritiques from '../components/ReportProcessusCritiques'
export default {
  components: { ReportProcessusCritiques },
  data: () => ({
    title: "Rapport d'éxecution des processus critiques",
    subTitle: ''
  })
}
</script>
