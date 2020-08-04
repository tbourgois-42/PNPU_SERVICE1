import axios from 'axios'
export default {
  namespaced: true,
  state: () => ({
    dashboardInfo: []
  }),
  getters: {
    dashboardInfo(state) {
      return state.token && state.user
    }
  },
  mutations: {
    SET_DASHBOARD_INFO(state, token) {
      state.token = token
    }
  },
  actions: {
    async getInfoDashboard({ commit, state }) {
      const response = await axios.get(
        `${process.env.WEB_SERVICE_WCF}/clients/dashboard`,
        {
          params: {
            workflowID: state.workflowID,
            idInstanceWF: state.idInstanceWF,
            user: state.user,
            habilitation: state.profil
          }
        }
      )
      if (response.status === 200) {
        commit('SET_DASHBOARD_INFO', response.data)
      }
    }
  }
}
