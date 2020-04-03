<template>
  <v-layout>
    <v-flex>
      <v-list-item>
        <v-list-item-content>
          <v-list-item-title class="title">
            Paramétrage des workflows
          </v-list-item-title>
          <v-list-item-subtitle>
            subtext
          </v-list-item-subtitle>
        </v-list-item-content>
      </v-list-item>
      <v-divider class="my-2 mx-4" inset></v-divider>
      <v-row>
        <v-col cols="12" sm="6">
          <DataTableWorkflow :selectedProcessus="selectedProcessus" />
        </v-col>
        <v-col cols="12" sm="6">
          <DataTableProcessus @getValue="getSelectedProcessus" />
        </v-col>
      </v-row>
    </v-flex>
  </v-layout>
</template>

<script>
import DataTableProcessus from '../components/DataTableProcessus'
import DataTableWorkflow from '../components/DataTableWorkflow'
export default {
  components: { DataTableProcessus, DataTableWorkflow },
  data: () => ({
    selectedProcessus: []
  }),

  methods: {
    getSelectedProcessus(value) {
      this.selectedProcessus = value
    }
  }
}
</script>
