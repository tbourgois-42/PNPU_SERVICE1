<template>
  <v-row>
    <v-col cols="12" class="d-flex justify-between py-0">
      <v-col cols="10">
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title class="title">
              {{ title }}
              <v-menu>
                <template v-slot:activator="{ on }">
                  <v-btn icon class="ml-8" v-on="on">
                    <v-icon>mdi-dots-vertical</v-icon>
                  </v-btn>
                </template>

                <v-list v-model="workflows.name">
                  <v-list-item
                    v-for="(workflow, id) in workflows"
                    :key="id"
                    @click="() => (workflowDate = workflow.name)"
                  >
                    <v-list-item-title>{{ workflow.name }}</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </v-list-item-title>
            <v-list-item-subtitle>
              Workflow {{ workflowDate }}
            </v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-col>

      <v-col cols="2">
        <v-text-field
          v-model="search"
          hide-details
          label="Chercher un client"
          append-icon="mdi-magnify"
          class="mt-4 pl-2"
          solo
        ></v-text-field>
      </v-col>
    </v-col>
    <v-col cols="12">
      <v-divider class="mx-4 pa-0"></v-divider>
    </v-col>

    <v-col cols="10" class="d-flex flex-wrap justify-start">
      <v-col v-for="(item, id) in visibleItems" :key="id" cols="3">
        <transition appear name="slide-in">
          <CardPnpu
            :client-name="item.client"
            :maxStep="maxStep"
            :clientTypolgie="item.typologie"
            :textStatus="item.textStatus"
            :currentStep="item.step"
            :percentCircular="item.percent"
            :workflowDate="workflowDate"
          />
        </transition>
      </v-col>
    </v-col>
    <v-col cols="2">
      <CardLaunchWorkflow :clients="items" :typologie="typologie" />
      <CardProgressTypologie :clients="items" />
      <CardIndicateurs :clients="items" />
    </v-col>
    <v-col cols="12">
      <v-pagination
        v-model="currentPage"
        :length="totalPages()"
        @input="updatePage(currentPage)"
        circle
        class="bottomPagination mb-6"
      ></v-pagination>
    </v-col>
  </v-row>
</template>

<script>
import CardPnpu from '../components/Card.vue'
import CardLaunchWorkflow from '../components/CardLaunchWorkflow'
import CardIndicateurs from '../components/CardIndicateurs'
import CardProgressTypologie from '../components/CardProgressTypologie'
import ClientData from '../data/Clients.json'
import Workflow from '../data/Workflow.json'
export default {
  components: {
    CardPnpu,
    CardLaunchWorkflow,
    CardProgressTypologie,
    CardIndicateurs
  },
  data: () => ({
    items: ClientData,
    workflows: Workflow,
    workflowDate: '03/2020',
    title: 'Dashboard',
    pageSize: 12,
    currentPage: 1,
    visibleItems: [],
    maxStep: 8,
    search: '',
    filter: '',
    typologie: ['SaaS Dédié', 'SaaS Mutualisé', 'SaaS Désynchronisé'],
    getapi: ''
  }),
  beforeMount() {
    this.updateVisibleItems()
    this.totalPages()
  },
  methods: {
    updatePage(pageNumber) {
      this.currentPage = pageNumber
      this.updateVisibleItems()
    },
    updateVisibleItems() {
      this.visibleItems = this.items.slice(
        (this.currentPage - 1) * this.pageSize,
        this.currentPage * 12
      )
    },
    totalPages() {
      return Math.ceil(this.items.length / this.pageSize)
    }
  },
  computed: {
    filteredItems() {
      if (this.filter === 'InProgress') {
        return this.items.filter((items) => items.textStatus === 'En cours')
      } else if (this.filter === 'InError') {
        return this.items.filter((items) => items.textStatus === 'En erreur')
      } else if (this.filter === 'Manuel') {
        return this.items.filter((items) => items.textStatus === 'Manuel')
      } else if (this.filter === 'Done') {
        return this.items.filter((items) => items.step === this.maxStep)
      } else if (this.filter === 'Plateforme') {
        return this.items.filter((items) => items.typologie === 'Plateforme')
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
      } else {
        this.updateVisibleItems()
      }
      return this.items
    }
  }
}
</script>

<style lang="css" scoped>
.container {
  max-width: max-content;
}

.bottomPagination {
  bottom: 0;
  position: absolute;
}

.slide-in-enter {
  opacity: 0;
  transform: scale(0.5);
}

.slide-in-enter-active {
  transition: all 0.4s ease;
}
</style>
