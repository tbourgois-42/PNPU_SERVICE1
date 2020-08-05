<template>
  <v-form ref="form" @submit.prevent="launch" class="ma-6">
    <v-container class="fill-height" fluid>
      <v-data-table
        :headers="headers"
        :items="items"
        :items-per-page="6"
        class="elevation-1 cursor"
        @click:row="getReport($event)"
        ><template v-slot:item.statut="{ item }">
          <v-chip :color="getColor(item.statut)" dark>{{ item.statut }}</v-chip>
        </template></v-data-table
      >
      <ReportTNR v-if="Object.entries(JSON_TEMPLATE).length > 0 && reportName === 'Tests de Non Régression (TNR)'"
          :idPROCESS="currentID_PROCESS"
          :reportJsonData="JSON_TEMPLATE"
          :idInstanceWF="idInstanceWF"
          :workflowID="workflowID"
          :currentID_STATUT="currentID_STATUT"
        />
    </v-container>
  </v-form>
</template>
<script>
import axios from 'axios'
import aes from 'aes-js'
import ReportTNR from '../../components/ReportTNR'
export default {
  components: { ReportTNR },
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
        { text: 'Processus', value: 'processus' },
        { text: 'Process ID', value: 'idPROCESSS' },
        { text: 'Statut', value: 'statut' }
      ],
      items: [
        {
          workflowID: 1,
          idInstanceWF: '1',
          client: 'SANEF',
          processus: 'Tests de Non Régression (TNR)',
          statut: 'En cours'
        },
        {
          workflowID: '30',
          idInstanceWF: '181',
          clientName: 'CAMAIEU',
          clientID: '12',
          processus: 'Tests de Non Régression (TNR)',
          idPROCESS: '7',
          statut: 'Terminé'
        },
        {
          workflowID: '1',
          idInstanceWF: '1',
          client: 'GLS',
          processus: 'Tests de Non Régression (TNR)',
          statut: 'En erreur'
        },
        {
          workflowID: '1',
          idInstanceWF: '1',
          client: 'GLS',
          processus: 'Tests des processus critiques',
          statut: 'En erreur'
        },
        {
          workflowID: '1',
          idInstanceWF: '1',
          client: 'SANEF',
          processus: 'Pré contrôle mdb',
          statut: 'Terminé'
        },
        {
          workflowID: '1',
          idInstanceWF: '1',
          client: 'SANEF',
          processus: 'Analyse logique',
          statut: 'Manuel'
        }
      ],
      JSON_TEMPLATE: {},
      idPROCESS: '',
      idInstanceWF: '',
      workflowID: '',
      currentID_STATUT: '',
      reportName: '',
      currentID_PROCESS: ''
    }
  },
  computed: {
    computedDateFormatted() {
      return this.formatDate(this.date)
    },
    formIsValid() {
      return (
        this.form.serverBefore &&
        this.form.serverAfter &&
        this.form.databaseBefore &&
        this.form.databaseAfter &&
        this.form.passwordBefore &&
        this.form.passwordAfter &&
        this.computedDateFormatted
      )
    }
  },

  watch: {
    loader() {
      const l = this.loader
      this[l] = !this[l]

      setTimeout(() => (this[l] = false), 3000)

      this.loader = null
    },
    date(val) {
      this.dateFormatted = this.formatDate(this.date)
    }
  },
  methods: {
    formatDate(date) {
      if (!date) return null

      const [year, month, day] = date.split('-')
      return `${month}/${day}/${year}`
    },
    parseDate(date) {
      if (!date) return null

      const [month, day, year] = date.split('/')
      return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`
    },
    async launch() {
      const fd = new FormData()
      fd.append('serverBefore', this.form.serverBefore)
      fd.append('databaseBefore', this.form.databaseBefore)
      fd.append(
        'passwordBefore',
        aes.utils.utf8.toBytes(this.form.passwordBefore)
      )
      fd.append('serverAfter', this.form.serverAfter)
      fd.append('databaseAfter', this.form.databaseAfter)
      fd.append(
        'passwordAfter',
        aes.utils.utf8.toBytes(this.form.passwordAfter)
      )
      fd.append('dtPaie', this.computedDateFormatted)
      try {
        await axios.post(`${process.env.WEB_SERVICE_WCF}/toolbox/TNR`, fd)
      } catch (error) {
        console.log(error)
      }
    },
    getColor(statut) {
      if (statut === 'En cours') return 'grey lighten-1'
      else if (statut === 'En erreur') return 'error'
      else if (statut === 'Manuel') return 'warning'
      else return 'success'
    },
    getReport(row) {
      debugger
      const vm = this
      axios
        .get(`${process.env.WEB_SERVICE_WCF}/report/` +
            row.workflowID +
            '/' +
            row.idInstanceWF +
            '/' +
            row.idPROCESS +
            '/' +
            row.clientID
        )
        .then(function(response) {
          if (response.data.getReportResult.length > 0) {
            vm.JSON_TEMPLATE = JSON.parse(
              response.data.getReportResult[0].JSON_TEMPLATE
            )
            debugger
            vm.reportName = vm.JSON_TEMPLATE[0].name
            vm.idInstanceWF = row.idInstanceWF
            vm.workflowID = row.workflowID
            vm.currentID_PROCESS = row.idPROCESS
            vm.currentID_STATUT = vm.JSON_TEMPLATE[0].result
          } else {
            vm.JSON_TEMPLATE = {}
          }
        })
        .catch(function(error) {
          vm.showSnackbar('error', `${error} !`)
        })
    },
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