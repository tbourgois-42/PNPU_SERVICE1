<template>
  <v-form ref="form" class="ma-6">
    <v-container class="fill-height" fluid>
      <v-data-table
        :headers="headers"
        :items="items"
        :items-per-page="6"
        class="elevation-1 cursor mb-6"
        @click:row="getReport($event)"
        ><template v-slot:item.ID_STATUT="{ item }">
          <v-chip :color="getColor(item.ID_STATUT)" dark>{{
            item.ID_STATUT
          }}</v-chip>
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
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Pré contrôle des .mdb'
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
          reportName === 'Gestion des dépendances'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportAnalyseData
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Analyse d\'impact sur les données'
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
          reportName === 'Analyse d\'impact logique'
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
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Intégration'
        "
        :idPROCESS="currentID_PROCESS"
        :reportJsonData="JSON_TEMPLATE"
        :idInstanceWF="idInstanceWF"
        :workflowID="workflowID"
        :currentID_STATUT="currentID_STATUT"
      />
      <ReportTNR
        v-if="
          Object.entries(JSON_TEMPLATE).length > 0 &&
          reportName === 'Tests de Non Régression (TNR)'
        "
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
          value: 'WORKFLOW_ID'
        },
        {
          text: 'Instance ID',
          value: 'ID_H_WORKFLOW'
        },
        {
          text: 'Nom du client',
          value: 'CLIENT_NAME'
        },
        {
          text: 'Client ID',
          value: 'CLIENT_ID'
        },
        { text: "Nom de l'instance", value: 'INSTANCE_NAME' },
        { text: 'Nom du processus', value: 'PROCESS_LABEL' },
        { text: 'Process ID', value: 'ID_PROCESS' },
        { text: 'Lancé le', value: 'LAUNCHING_DATE' },
        { text: 'Statut', value: 'ID_STATUT' }
      ],
      items: [],
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
    /**
     * Get habilitation from vuex
     */
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
     * Load informations for data table
     */
    async initialize() {
      try {
        const response = await axios.get(
          `${process.env.WEB_SERVICE_WCF}/toolbox/Dashboard/`,
          {
            params: {
              user: this.user,
              habilitation: this.profil
            }
          }
        )
        if (response.status === 200) {
          this.items = response.data.GetInfoLaunchToolBoxResult
        }
      } catch (error) {
        this.showSnackbar('error', `${error} !`)
      }
    },

    /**
     * Get color according to workflow statut
     * @param {string} - workflow statut
     */
    getColor(statut) {
      if (statut === 'IN PROGRESS') return 'grey lighten-1'
      else if (statut === 'ERROR') return 'error'
      else if (statut === 'WARNING') return 'warning'
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
            row.WORKFLOW_ID +
            '/' +
            row.ID_H_WORKFLOW +
            '/' +
            row.ID_PROCESS +
            '/' +
            row.CLIENT_ID
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
            if (this.reportName === 'Livraison') {
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
