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
          <ReportHF1214 />
        </v-flex>
      </v-row>
    </v-container>
  </v-app>
</template>

<script>
import ReportHF1214 from '../../components/ReportHF1214'
export default {
  components: { ReportHF1214 },
  data: () => ({
    title: "Rapport d'éxecution du HF1214",
    subTitle: 'Pré contrôle des .mdb'
  })
}
</script>
