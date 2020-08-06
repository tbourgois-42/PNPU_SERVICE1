<template>
  <v-form ref="form" class="ma-6">
    <v-container class="fill-height" fluid>
      <v-data-table
        :headers="headers"
        :items="items"
        :items-per-page="6"
        class="elevation-1 cursor mb-6"
        @click:row="getReport($event)"
        ><template v-slot:item.statut="{ item }">
          <v-chip :color="getColor(item.statut)" dark>{{ item.statut }}</v-chip>
        </template></v-data-table
      >
      <ReportLivraison
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 && reportName === 'Livraison'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :nbAvailablePack="nbAvailablePack"
        :currentID_STATUT="currentID_STATUT"
        :clientID="clientId"
        :clientName="clientName"
      />
      <ReportPreControle
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 && reportName === 'Pré contrôle des .mdb'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportInitialisation
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Initialisation'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportPackagingDependances
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Packaging des dépendances'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportAnalyseData
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 && reportName === 'Analyse de données'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportAnalyseLogique
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Analyse logique'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportProcessusCritiques
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Tests des processus critiques'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportIntegration
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 && reportName === 'Intégration'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportTNR
        v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportName === 'Tests de Non Régression (TNR)'"
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <v-snackbar v-model="snackbar" :color="colorsnackbar" :timeout="6000" top>
        {{ snackbarMessage }}
        <v-btn dark text @click="snackbar = false">
          Close
        </v-btn>
      </v-snackbar>
    </v-container>
  </v-form>
</template>
<script>
import { mapGetters } from 'vuex'
import axios from 'axios'
import ReportPreControle from '../../components/ReportPreControle'
import ReportTNR from '../../components/ReportTNR'
import ReportLivraison from '../../components/ReportLivraison'
import ReportInitialisation from '../../components/ReportInitialisation'
import ReportPackagingDependances from '../../components/ReportPackagingDependances'
import ReportAnalyseData from '../../components/ReportAnalyseData'
import ReportAnalyseLogique from '../../components/ReportAnalyseLogique'
import ReportProcessusCritiques from '../../components/ReportProcessusCritiques'
import ReportIntegration from '../../components/ReportIntegration'
export default {
  components: {
    ReportTNR,
    ReportLivraison,
    ReportPreControle,
    ReportInitialisation,
    ReportPackagingDependances,
    ReportAnalyseData,
    ReportAnalyseLogique,
    ReportProcessusCritiques,
    ReportIntegration
  },
  data() {
    return {
      headers: [
        {
          text: 'Workflow ID',
          align: 'start',
          value: 'workflowID'
        },
        {
          text: 'Instance ID',
          value: 'idInstanceWF'
        },
        {
          text: 'Nom du client',
          value: 'clientName'
        },
        {
          text: 'Client ID',
          value: 'clientID'
        },
        { text: "Nom de l'instance", value: 'instanceName' },
        { text: 'Nom du processus', value: 'processName' },
        { text: 'Process ID', value: 'processID' },
        { text: "Lancé le", value: 'dtLaunchDate' },
        { text: 'Statut', value: 'statut' },
      ],
      items: [
        {
          workflowID: '20',
          idInstanceWF: '27',
          clientName: 'OCP Répartition',
          clientID: '110',
          instanceName: 'test',
          processName: 'Initialisation',
          processID: '2',
          dtLaunchDate: '29/07/2020 13:34:19',
          statut: 'Terminé'
        },
        {
          workflowID: '30',
          idInstanceWF: '181',
          clientName: 'CAMAIEU',
          clientID: '12',
          processus: 'Tests de Non Régression (TNR)',
          idPROCESS: '7',
          statut: 'En erreur'
        },
        {
         workflowID: '28',
          idInstanceWF: '129',
          clientName: 'Allez & Cie',
          clientID: '111',
          processus: 'Intégration',
          idPROCESS: '5',
          statut: 'En erreur'
        },
        {
          workflowID: '29',
          idInstanceWF: '168',
          clientName: 'Allez & Cie',
          clientID: '101',
          processus: 'Tests des processus critiques',
          idPROCESS: '6',
          statut: 'Terminé'
        },
        {
          workflowID: '28',
          idInstanceWF: '115',
          clientName: 'Allez & Cie',
          clientID: '111',
          processus: 'Pré contrôle mdb',
          idPROCESS: '1',
          statut: 'Terminé'
        },
        {
          workflowID: '26',
          idInstanceWF: '47',
          clientName: 'Radio France',
          clientID: '101',
          processus: 'Livraison',
          idPROCESS: '8',
          statut: 'Terminé'
        }
      ],
      JSON_TEMPLATE: {},
      idPROCESS: '',
      idInstanceWF: '',
      workflowID: '',
      currentID_STATUT: '',
      reportName: '',
      currentID_PROCESS: '',
      nbAvailablePack: 0,
      clientName: '',
      clientID: '',
      snackbar: false,
      colorsnackbar: '',
      snackbarMessage: ''
    }
  },
  computed: {
    computedDateFormatted() {
      return this.formatDate(this.date)
    },
    ...mapGetters({
        user: 'modules/auth/user',
        profil: 'modules/auth/profil'
    })
    },

  created() {
    this.initialize()
  },

  methods: {
      /**
      * Chargement des informations pour les cartes.
      */
      initialize() {
        const vm = this
        vm.loadingData = true
        axios
          .get(`${process.env.WEB_SERVICE_WCF}/toolbox/Dashboard/`, {
            params: {
              user: this.user,
              habilitation: this.profil
            }
          })
          .then(function (response) {
            debugger
          })
          .catch(function (error) {
            vm.showSnackbar(
              'error',
              `${error} ! Impossible de récupérer l'historique des steps`
            )
          })
      },
    getColor(statut) {
      if (statut === 'En cours') return 'grey lighten-1'
      else if (statut === 'En erreur') return 'error'
      else if (statut === 'Manuel') return 'warning'
      else return 'success'
    },

    /**
     * Get report from database
     * @param {object} - Row selected
     */
    async getReport(row) {
      try {
        const response = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/report/` +
            row.workflowID +
            '/' +
            row.idInstanceWF +
            '/' +
            row.processID +
            '/' +
            row.clientID
        )
        if (response.status === 200) {
          if (response.data.GetReportResult.length > 0) {
            this.JSON_TEMPLATE = JSON.parse(
              response.data.GetReportResult[0].JSON_TEMPLATE
            )
            this.reportName = this.JSON_TEMPLATE[0].name
            this.idInstanceWF = row.idInstanceWF
            this.workflowID = row.workflowID
            this.currentID_PROCESS = row.idPROCESS
            this.currentID_STATUT = this.JSON_TEMPLATE[0].result
            if (this.result === '') {
              this.GetNbAvailablePack()
              this.clientID = row.clientID
              this.clientName = row.clientName
            }
          } else {
            this.JSON_TEMPLATE = {}
          }
        } else {
          this.showSnackbar('error', `${response} !`)
        }
      } catch (error) {
        this.showSnackbar('error', `${error} !`)
      }
    },

    /***
     * Get available packages from database
     */
    GetNbAvailablePack() {
      const vm = this
      axios
        .get(
          `${process.env.WEB_SERVICE_WCF}/livraison/availablePack/` +
            vm.workflowID +
            `/` +
            vm.idInstanceWF +
            `/` +
            vm.clientId
        )
        .then(function (response) {
          if (response.status === 200) {
            vm.nbAvailablePack = response.data
          }
        })
        .catch(function (error) {
          vm.showSnackbar('error', `${error} !`)
        })
    },

    /**
     * Gére l'affichage du snackbar.
     * @param {string} color - Couleur de la snackbar.
     * @param {string} message - Message affiché dans la snackbar.
     */
    showSnackbar(color, message) {
      this.snackbar = true
      this.colorsnackbar = color
      this.snackbarMessage = message
    }
  }
}
</script>
<style lang="css">
.cursor {
  cursor: pointer;
}
.v-treeview-node__root {
  cursor: pointer !important;
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s;
}
.fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
  opacity: 0;
}
</style>
