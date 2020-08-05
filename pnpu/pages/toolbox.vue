<template>
  <v-layout>
    <v-container>
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
        <v-row>
          <v-col cols="12">
            <div>
              <v-alert
                v-model="alert"
                icon="mdi-information-outline"
                text
                type="info"
                class="mx-4 mb-0"
                border="left"
                dark
                dismissible
              >
                <div>
                  La toolbox est un outil permettant l'éxecution à la demande
                  d'un processus.
                </div>
                <br />
                <div>Pour éxecuter un processus :</div>
                <ul>
                  <li>Sélectionner un processus depuis la liste déroulante,</li>
                  <li>
                    Renseigner l'ensemble des champs nécessaire à l'éxecution,
                  </li>
                  <li>Cliquer sur le bouton "Executer"</li>
                </ul>
              </v-alert>
            </div>
          </v-col>
          <v-col cols="12">
            <v-card color="basil">
              <v-card-title class="text-center justify-center py-6">
                {{ toolboxName }}
              </v-card-title>

              <v-tabs
                v-model="tab"
                background-color="transparent"
                color="basil"
                grow
              >
                <v-tab v-for="item in tabs" :key="item">
                  {{ item }}
                </v-tab>
              </v-tabs>

              <v-tabs-items class="d-flex justify-center" v-model="tab">
                <v-tab-item v-for="item in tabs" :key="item">
                  <v-row v-if="tab === 1">
                    <v-col cols="12" class="d-flex space-between align-start">
                      <v-col cols="6">
                        <v-select
                          v-model="toolboxName"
                          :items="workflows"
                          label="Sélectionner un processus"
                          solo
                        ></v-select>
                      </v-col>
                      <v-col cols="6">
                        <v-select
                          v-model="clientID"
                          :items="lstClient"
                          label="Sélectionner votre client"
                          solo
                        ></v-select>
                      </v-col>
                    </v-col>
                  </v-row>
                  <TlbxTNR
                    v-if="
                      toolboxName === 'Tests de Non Régression (TNR)' &&
                      tab === 1
                    "
                    :client="clientID"
                  />
                  <TlbxAnalyseData
                    v-if="toolboxName === 'Analyse de données' && tab === 1"
                  />
                  <TlbxAnalyseLogique
                    v-if="toolboxName === 'Analyse logique' && tab === 1"
                  />
                  <TlbxIntegration
                    v-if="toolboxName === 'Test d\'intégration' && tab === 1"
                  />
                  <TlbxPackagingDependances
                    v-if="
                      toolboxName === 'Packaging des dépendances' && tab === 1
                    "
                  />
                  <TlbxPreControl
                    v-if="toolboxName === 'Pré contrôle mdb' && tab === 1"
                  />
                  <TlbxTestsProcessusCritiques
                    v-if="
                      toolboxName === 'Tests des processus critiques' &&
                      tab === 1
                    "
                  />
                  <v-alert
                    v-if="toolboxName === ''"
                    icon="mdi-information-outline"
                    text
                    type="warning"
                    class="mx-4 mb-0"
                    border="left"
                    dark
                    dismissible
                  >
                    Il n'est pas encore possible d'éxecuter ce processus en mode
                    Toolbox
                  </v-alert>
                  <TlbxResultats v-if="tab === 0" />
                </v-tab-item>
              </v-tabs-items>
            </v-card>
          </v-col>
        </v-row>
      </v-flex>
    </v-container>
  </v-layout>
</template>

<script>
import axios from 'axios'
import { mapGetters } from 'vuex'
import TlbxTNR from '../components/Toolbox/TlbxTNR'
import TlbxAnalyseData from '../components/Toolbox/TlbxAnalyseData'
import TlbxAnalyseLogique from '../components/Toolbox/TlbxAnalyseLogique'
import TlbxIntegration from '../components/Toolbox/TlbxIntegration'
import TlbxPackagingDependances from '../components/Toolbox/TlbxPackagingDependances'
import TlbxPreControl from '../components/Toolbox/TlbxPreControl'
import TlbxTestsProcessusCritiques from '../components/Toolbox/TlbxTestsProcessusCritiques'
import TlbxResultats from '../components/Toolbox/TlbxResultats'
export default {
  components: {
    TlbxTNR,
    TlbxAnalyseData,
    TlbxAnalyseLogique,
    TlbxIntegration,
    TlbxPackagingDependances,
    TlbxPreControl,
    TlbxTestsProcessusCritiques,
    TlbxResultats
  },
  data: () => ({
    title: 'Toolbox',
    subTitle: 'Executer un processus',
    items: [
      'Pré contrôle mdb',
      'Packaging des dépendances',
      "Test d'intégration",
      'Tests de Non Régression (TNR)',
      'Analyse de données',
      'Analyse logique',
      'Tests des processus critiques'
    ],
    tabs: ['Résultats', 'Execution'],
    toolboxName: '',
    alert: true,
    lstClient: [],
    showResultats: true,
    showExecution: false,
    tab: null,
    clientID: '',
    workflows: []
  }),
  computed: {
    ...mapGetters({
      clients: 'modules/auth/clients'
    })
  },
  created() {
    this.toolboxName = 'Tests de Non Régressions (TNR)'
    this.createLstClient()
    this.getListWorkflow()
  },
  methods: {
    createLstClient() {
      this.clients.forEach((client) => {
        this.lstClient.push({
          value: client.ID_CLIENT,
          text: client.CLIENT_NAME
        })
      })
    },
    getTabs(e) {
      if (e === 0) {
        this.showResultats = !this.showResultats
      } else {
        this.showExecution = !showExecution
      }
    },
    async getListWorkflow() {
      try {
        const response = await axios.get(`${process.env.WEB_SERVICE_WCF}/toolbox/workflow`, {
          params: {
            toolbox : true
          }
        })
        if (response.status === 200) {
          response.data.forEach(element => {
            this.workflows.push(element.WORKFLOW_LABEL)
          })
        } else {
          console.log(response)
        }
      } catch (error) {
        console.log(response)
      }
    }
  }
}
</script>
