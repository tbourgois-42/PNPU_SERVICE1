<template>
  <v-layout column justify-center align-center>
    <v-flex xs12 sm8 md6>
      <div class="text-center">
        <v-content class="pa-0">
          <v-toolbar dense flat>
            <v-icon right class="mr-5">mdi-view-dashboard</v-icon>
            <v-toolbar-title
              >Dashboard | Workflow {{ workflowDate }}</v-toolbar-title
            >

            <v-menu>
              <template v-slot:activator="{ on }">
                <v-btn icon v-on="on">
                  <v-icon>mdi-dots-vertical</v-icon>
                </v-btn>
              </template>

              <v-list v-model="workflows.name">
                <v-list-item
                  v-for="workflow in workflows"
                  :key="workflow"
                  @click="() => (workflowDate = workflow.name)"
                >
                  <v-list-item-title>{{ workflow.name }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>

            <v-text-field
              v-model="search"
              hide-details
              label="Chercher un client ..."
              append-icon="mdi-magnify"
              solo
              class="mr-6 ml-6"
            ></v-text-field>
            <v-btn icon @click.prevent="filter = 'InProgress'">
              <v-badge color="grey lighten-1" :content="countInProgress">
                <v-icon color="grey lighten-1">mdi-progress-clock</v-icon>
              </v-badge>
            </v-btn>
            <v-btn icon @click.prevent="filter = 'InError'">
              <v-badge color="red lighten-1" :content="countInError">
                <v-icon color="red lighten-1">mdi-close-circle</v-icon>
              </v-badge>
            </v-btn>
            <v-btn icon @click.prevent="filter = 'Manuel'">
              <v-badge color="orange lighten-1" :content="countManuel">
                <v-icon color="orange lighten-1">mdi-hand</v-icon>
              </v-badge>
            </v-btn>
            <v-btn icon @click.prevent="filter = 'Done'">
              <v-badge color="green lighten-1" :content="countDone">
                <v-icon color="green lighten-1">mdi-check-circle</v-icon>
              </v-badge>
            </v-btn>
            <v-btn icon @click.prevent="filter = 'All'">
              <v-icon color="grey darken-4">mdi-filter-remove</v-icon>
            </v-btn>
          </v-toolbar>
        </v-content>
      </div>
      <div>
        <v-row cols="12" app>
          <v-col v-for="avancement in avancements" :key="avancement.id" md="4">
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
          </v-col>
        </v-row>
      </div>
      <div>
        <v-item-group>
          <v-row cols="12">
            <v-col v-for="item in filteredBadge" :key="item.id" md="3">
              <v-hover>
                <template v-slot="{ hover }">
                  <router-link
                    :to="{
                      name: 'client',
                      params: {
                        client: item.client,
                        step: item.step,
                        workflowDate: workflowDate,
                        textStatus: item.textStatus
                      }
                    }"
                    append
                    tag="span"
                    class="route"
                  >
                    <v-card
                      :elevation="hover ? 6 : 1"
                      class="d-flex justify-lg-space-between transition-swing "
                      transition="slide-y-transition"
                    >
                      <div>
                        <v-card-title
                          class="headline subtitle-2 text-uppercase"
                          >{{ item.client }}</v-card-title
                        >
                        <v-card-subtitle
                          >Step {{ item.step }} / {{ max }}</v-card-subtitle
                        >
                        <v-card-subtitle>{{ item.typologie }}</v-card-subtitle>
                        <v-chip
                          v-if="item.step == 10"
                          color="green lighten-1"
                          class="ml-3 mb-3"
                          text-color="white"
                        >
                          <v-icon left color="white">mdi-check-circle</v-icon>
                          Terminé
                        </v-chip>
                        <v-chip
                          v-else-if="item.step < 10"
                          :color="item.colorIconStatus"
                          class="ml-3 mb-3"
                          text-color="white"
                        >
                          <v-icon left color="white">
                            {{ item.iconStatus }}
                          </v-icon>
                          {{ item.textStatus }}
                        </v-chip>
                      </div>
                      <v-progress-circular
                        :rotate="-90"
                        :size="120"
                        :width="10"
                        :value="item.percent"
                        :color="item.color"
                        class="ma-5 d-none d-sm-flex"
                      >
                        {{ item.percent }}
                      </v-progress-circular>
                    </v-card>
                  </router-link>
                </template>
              </v-hover>
            </v-col>
          </v-row>
        </v-item-group>
      </div>
      <div>
        <v-row>
          <v-col cols="12">
            <v-pagination v-model="page" :length="6"> </v-pagination>
          </v-col>
        </v-row>
      </div>
      <v-card> AlaconResult : {{ test.AlaconResult }} </v-card>
      <v-btn @click="testpost">Test</v-btn>
    </v-flex>
  </v-layout>
</template>

<script>
import axios from 'axios'
export default {
  components: {},
  data: () => ({
    items: [
      {
        id: '1',
        step: '5',
        percent: '62.5',
        client: 'Platform',
        color: 'light-blue',
        typologie: 'Platform',
        iconStatus: 'mdi-progress-clock',
        colorIconStatus: 'grey lighten-1',
        textStatus: 'En cours'
      },
      {
        id: '2',
        step: '2',
        percent: '25',
        client: 'SANEF',
        color: 'teal darken-1',
        typologie: 'SaaS Dédié',
        iconStatus: 'mdi-close-circle',
        colorIconStatus: 'red lighten-1',
        textStatus: 'Erreur'
      },
      {
        id: '3',
        step: '7',
        percent: '70',
        client: 'ICL',
        color: 'teal darken-1',
        typologie: 'SaaS Dédié',
        iconStatus: 'mdi-progress-circle',
        colorIconStatus: 'grey lighten-1',
        textStatus: 'En cours'
      },
      {
        id: '4',
        step: '8',
        percent: '75',
        client: 'Client3',
        color: 'lime',
        typologie: 'SaaS Désynchronisé',
        iconStatus: 'mdi-close-circle',
        colorIconStatus: 'red lighten-1',
        textStatus: 'Erreur'
      },
      {
        id: '5',
        step: '5',
        percent: '62.5',
        client: 'Client4',
        color: 'lime',
        typologie: 'SaaS Désynchronisé',
        iconStatus: 'mdi-progress-clock',
        colorIconStatus: 'grey lighten-1',
        textStatus: 'En cours'
      },
      {
        id: '6',
        step: '8',
        percent: '100',
        client: 'FRAMATOME',
        color: 'light-blue',
        typologie: 'Platform',
        iconStatus: 'mdi-check-clock',
        colorIconStatus: 'grey lighten-1',
        textStatus: 'Terminé'
      },
      {
        id: '7',
        step: '2',
        percent: '25',
        client: 'Client1',
        color: 'teal darken-1',
        typologie: 'SaaS Dédié',
        iconStatus: 'mdi-progress-clock',
        colorIconStatus: 'grey lighten-1',
        textStatus: 'En cours'
      },
      {
        id: '8',
        step: '10',
        percent: '100',
        client: 'Client2',
        color: 'teal darken-1',
        typologie: 'SaaS Dédié',
        iconStatus: 'mdi-check-circle',
        colorIconStatus: 'green lighten-1',
        textStatus: 'Terminé'
      },
      {
        id: '9',
        step: '6',
        percent: '75',
        client: 'Client3',
        color: 'lime',
        typologie: 'SaaS Désynchronisé',
        iconStatus: 'mdi-progress-clock',
        colorIconStatus: 'grey lighten-1',
        textStatus: 'En cours'
      },
      {
        id: '10',
        step: '8',
        percent: '80',
        client: 'Client4',
        color: 'lime',
        typologie: 'SaaS Désynchronisé',
        iconStatus: 'mdi-close-circle',
        colorIconStatus: 'red lighten-1',
        textStatus: 'Erreur'
      },
      {
        id: '11',
        step: '5',
        percent: '50',
        client: 'DASSAULT SYSTEMES',
        color: 'light-blue',
        typologie: 'SaaS Désynchronisé',
        iconStatus: 'mdi-hand',
        colorIconStatus: 'orange lighten-1',
        textStatus: 'Manuel'
      },
      {
        id: '12',
        step: '10',
        percent: '100',
        client: 'Client1',
        color: 'teal darken-1',
        typologie: 'SaaS Dédié',
        iconStatus: 'mdi-check-circle',
        colorIconStatus: 'green lighten-1',
        textStatus: 'Terminé'
      }
    ],
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
    workflows: [
      { name: '01/2020' },
      { name: '02/2020' },
      { name: '03/2020' },
      { name: '04/2020' }
    ],
    workflowDate: '03/2020',
    search: '',
    filter: '',
    filtre: ['SaaS Dédié', 'Platform', 'SaaS Désynchro'],
    max: '8',
    test: ''
  }),
  computed: {
    countDone() {
      return this.items.filter((items) => items.step === '10').length
    },
    countInProgress() {
      return this.items.filter((items) => items.textStatus === 'En cours')
        .length
    },
    countInError() {
      return this.items.filter((items) => items.textStatus === 'Erreur').length
    },
    countManuel() {
      return this.items.filter((items) => items.textStatus === 'Manuel').length
    },
    filteredBadge() {
      if (this.filter === 'InProgress') {
        return this.items.filter((items) => items.textStatus === 'En cours')
      } else if (this.filter === 'InError') {
        return this.items.filter((items) => items.textStatus === 'Erreur')
      } else if (this.filter === 'Manuel') {
        return this.items.filter((items) => items.textStatus === 'Manuel')
      } else if (this.filter === 'Done') {
        return this.items.filter((items) => items.step === '10')
      } else if (this.filter === 'Platform') {
        return this.items.filter((items) => items.typologie === 'Platform')
      } else if (this.filter === 'SaaS Dédié') {
        return this.items.filter((items) => items.typologie === 'SaaS Dédié')
      } else if (this.filter === 'SaaS Désynchronisé') {
        return this.items.filter(
          (items) => items.typologie === 'SaaS Désynchronisé'
        )
      } else if (this.search) {
        return this.items.filter((item) => {
          return item.client.toUpperCase().match(this.search.toUpperCase())
        })
      }
      return this.items
    }
  },
  created() {
    axios
      .get('http://localhost:63267/Service1.svc/Alacon/1')
      .then((res) => {
        this.test = res.data
      })
      .catch((err) => {
        return err
      })
  },
  methods: {
    testpost() {
      axios
        .post('http://localhost:63267/Service1.svc/Workflow/CreateWorkflow/', {
          id: 'Fred',
          name: 'Flintstone'
        })
        .then(function(response) {
          return response
        })
        .catch(function(error) {
          return error
        })
    }
  }
}
</script>
