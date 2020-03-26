<template>
  <v-layout row wrap>
    <v-container row wrap>
      <v-flex md6>
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title class="title">
              Dashboard
              <v-btn icon v-on="on" class="ml-8">
                <v-icon>mdi-dots-vertical</v-icon>
              </v-btn>
            </v-list-item-title>
            <v-list-item-subtitle>
              Workflow 03/2020
            </v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-flex>
      <v-flex md2>
        <v-btn x-large class="mt-4" outlined color="primary">
          <v-icon left>mdi-play</v-icon>Lancer un workflow
        </v-btn>
      </v-flex>
      <v-flex md2>
        <v-btn x-large class="mt-4" outlined color="error">
          <v-icon left>mdi-stop</v-icon>Stopper le workflow
        </v-btn>
      </v-flex>
      <v-flex md2>
        <v-text-field
          v-model="search"
          hide-details
          label="Chercher un client ..."
          append-icon="mdi-magnify"
          outlined
          class="mt-4"
        ></v-text-field>
      </v-flex>
      <v-divider class="my-2 mx-4" inset></v-divider>
    </v-container>
    <v-container class="d-flex justify-end">
      <v-card-text class="pt-0">
        SaaS Dédié (50%)
        <v-progress-linear
          :color="primary"
          height="10"
          :value="50"
          striped
          class="mt-4"
        ></v-progress-linear>
      </v-card-text>
      <v-card-text class="pt-0">
        SaaS Désynchronisé (50%)
        <v-progress-linear
          color="error"
          height="10"
          :value="50"
          striped
          class="mt-4"
        ></v-progress-linear>
      </v-card-text>
      <v-card-text class="pt-0">
        SaaS Mutualisé (50%)
        <v-progress-linear
          color="warning"
          height="10"
          :value="50"
          striped
          class="mt-4"
        ></v-progress-linear>
      </v-card-text>
      <v-subheader class="pb-2">Indicateurs</v-subheader>
      <v-btn icon class="mx-2"> <v-icon class="mx-4">mdi-filter</v-icon></v-btn>
      <v-subheader class="pb-2">Filtrer</v-subheader>
      <v-btn icon class="mx-2"> <v-icon class="mx-4">mdi-filter</v-icon></v-btn>
      <v-subheader class="pb-2">Filtrer</v-subheader>
      <v-btn icon class="mx-2"> <v-icon class="mx-4">mdi-filter</v-icon></v-btn>
      <v-subheader class="pb-2">Filtrer</v-subheader>
      <v-btn icon class="mx-2"> <v-icon class="mx-4">mdi-filter</v-icon></v-btn>
      <v-subheader class="pb-2">Filtrer</v-subheader>
      <v-btn icon class="mx-2"> <v-icon class="mx-4">mdi-filter</v-icon></v-btn>
      <v-subheader class="pb-2">Ordre</v-subheader>
      <v-btn icon class="mx-2">
        <v-icon class="mx-4">mdi-sort-ascending</v-icon>
      </v-btn>
    </v-container>

    <!--<v-divider class="my-2 mx-4" inset></v-divider>
    <v-col
      v-for="avancement in avancements"
      :key="avancement.id"
      cols="12"
      xs="6"
      sm="6"
      lg="4"
    >
      <v-card @click.prevent="filter = avancement.title">
        <v-card-text>
          {{ avancement.title }} ({{ avancement.pourcent }})
          <v-progress-linear
            :color="avancement.color"
            height="10"
            :value="avancement.avancement"
            striped
            class="mt-4"
          ></v-progress-linear>
        </v-card-text>
      </v-card>
    </v-col>-->
    <v-col v-for="n in 10" :key="n" cols="12" sm="6" lg="3">
      <CardPnpu />
    </v-col>
  </v-layout>
</template>

<script>
import CardPnpu from '../components/Card.vue'
export default {
  components: { CardPnpu },
  data: () => ({
    workflows: [
      { name: '01/2020' },
      { name: '02/2020' },
      { name: '03/2020' },
      { name: '04/2020' }
    ],
    workflowDate: '03/2020',
    avancements: [
      {
        id: '1',
        title: 'Platform',
        pourcent: '50%',
        color: 'light-blue',
        avancement: '50'
      },
      {
        id: '2',
        title: 'SaaS Dédié',
        pourcent: '75%',
        color: 'teal',
        avancement: '75'
      },
      {
        id: '3',
        title: 'SaaS Désynchronisé',
        pourcent: '25%',
        color: 'lime',
        avancement: '25'
      }
    ],
    btnLancementWF: 'false'
  })
}
</script>

<style lang="css" scoped>
.v-input__slot {
  min-height: 52px !important;
}
</style>
