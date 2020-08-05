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
          <ReportIntegration />
        </v-flex>
      </v-row>
    </v-container>
  </v-app>
</template>

<script>
import ReportIntegration from '../components/ReportIntegration'
export default {
  components: { ReportIntegration },
  data: () => ({
    title: "Rapport d'intégration",
    subTitle: ''
  })
}
</script>
