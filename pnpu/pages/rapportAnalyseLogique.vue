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
          <ReportAnalyseLogique />
        </v-flex>
      </v-row>
    </v-container>
  </v-app>
</template>

<script>
import ReportAnalyseLogique from '../components/ReportAnalyseLogique'
export default {
  components: { ReportAnalyseLogique },
  data: () => ({
    title: "Rapport d'analyse logique",
    subTitle: ''
  })
}
</script>
