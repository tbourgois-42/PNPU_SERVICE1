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
          <ReportPackagingDependances />
        </v-flex>
      </v-row>
    </v-container>
  </v-app>
</template>

<script>
import ReportPackagingDependances from '../components/ReportPackagingDependances'
export default {
  components: { ReportPackagingDependances },
  data: () => ({
    title: "Rapport packaging de dépendances",
    subTitle: ''
  })
}
</script>
