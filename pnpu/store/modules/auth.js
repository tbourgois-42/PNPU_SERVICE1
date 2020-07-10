import axios from 'axios'
export default {
  namespaced: true,
  state: () => ({
    token: null,
    user: null,
    profil: null,
    clients: [],
    dashboardInfo: [],
    nbLocalisation: null
  }),
  getters: {
    authenticated(state) {
      return state.token && state.user
    },

    user(state) {
      return state.user
    },

    profil(state) {
      return state.profil
    },

    clients(state) {
      return state.clients
    },

    dashboardInfo(state) {
      return state.dashboardInfo
    },

    nbLocalisation(state) {
      return state.nbLocalisation
    }
  },
  mutations: {
    SET_TOKEN(state, token) {
      state.token = token
    },
    SET_USER(state, data) {
      state.user = data
    },
    SET_HABILITATION(state, data) {
      state.profil = data
    },
    SET_CLIENTS(state, data) {
      state.clients = data
    },
    SET_DASHBOARD_INFO(state, data) {
      state.dashboardInfo = data
    },
    SET_NB_LOCALISATION(state, data) {
      state.nbLocalisation = data
    }
  },
  actions: {
    async signIn({ dispatch }, credentials) {
      const fd = new FormData()
      fd.append('user', credentials.user)
      fd.append('password', credentials.password)
      const response = await axios.post(
        `${process.env.WEB_SERVICE_WCF}/auth/signin`,
        fd
      )
      return dispatch('attempt', response.data)
    },

    async attempt({ commit, state, dispatch }, token) {
      if (token) {
        commit('SET_TOKEN', token)
      }
      if (!state.token) {
        return
      }
      const fd = new FormData()
      fd.append('token', token)
      try {
        const response = await axios.post(
          `${process.env.WEB_SERVICE_WCF}/auth/me`,
          fd
        )
        if (response.data === '') {
          commit('SET_USER', null)
        } else {
          commit('SET_USER', response.data)
        }
        localStorage.setItem('token', token)
        dispatch('getHabilitation', response.data)
      } catch (error) {
        commit('SET_TOKEN', null)
        commit('SET_USER', null)
      }
    },

    async signOut({ commit, state }) {
      const fd = new FormData()
      fd.append('user', state.user)
      fd.append('token', state.token)
      const response = await axios.post(
        `${process.env.WEB_SERVICE_WCF}/auth/signout`,
        fd
      )
      if (response.status === 200) {
        commit('SET_TOKEN', null)
        commit('SET_USER', null)
        localStorage.removeItem('token')
      }
    },

    async getHabilitation({ commit, state, dispatch }) {
      const response = await axios.get(
        `${process.env.WEB_SERVICE_WCF}/auth/habilitation`,
        {
          params: {
            user: state.user,
            token: state.token
          }
        }
      )
      if (response.status === 200) {
        commit('SET_HABILITATION', response.data)
        dispatch('getListClient', response.data)
      }
    },

    async getListClient({ commit, state, dispatch }) {
      const response = await axios.get(
        `${process.env.WEB_SERVICE_WCF}/auth/habilitation/clients`,
        {
          params: {
            user: state.user,
            habilitation: state.profil
          }
        }
      )
      if (response.status === 200) {
        commit('SET_CLIENTS', response.data)
        dispatch('getInfoDashboard', response.data)
      }
    },

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
    },

    async getNbLocalisation({ commit, state }) {
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
        commit('SET_NB_LOCALISATION', response.data)
      }
    },

    async getHistoricWorkflow({ commit }) {
      const response = await axios.get(
        `${process.env.WEB_SERVICE_WCF}/workflow/historic`
      )
      if (response.status === 200) {
        commit('SET_H_WORKFLOW', response.data)
        commit(
          'SET_WORKFLOW_ID',
          response.data.GetHWorkflowResult[
            this.workflows.length - 1
          ].WORKFLOW_ID.toString()
        )
        commit(
          'SET_WORKFLOW_NAME',
          response.data.GetHWorkflowResult[this.workflows.length - 1]
            .WORKFLOW_LABEL
        )
        commit(
          'SET_WORKFLOW_STATUS',
          response.data.GetHWorkflowResult[this.workflows.length - 1]
            .STATUT_GLOBAL
        )
        commit(
          'SET_INSTANCE_NAME_WF',
          response.data.GetHWorkflowResult[this.workflows.length - 1]
            .INSTANCE_NAME
        )
        commit(
          'SET_ID_INSTANCE_WF',
          response.data.GetHWorkflowResult[this.workflows.length - 1]
            .ID_H_WORKFLOW
        )
      }
    }
  }
}
