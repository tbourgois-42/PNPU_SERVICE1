import axios from 'axios'
export default {
  namespaced: true,
  state: () => ({
    token: null,
    user: null,
    habilitationName: null,
    clients: []
  }),
  getters: {
    authenticated(state) {
      return state.token && state.user
    },

    user(state) {
      return state.user
    },

    habilitationName(state) {
      return state.habilitationName
    },

    clients(state) {
      return state.clients
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
      state.habilitationName = data
    },
    SET_CLIENTS(state, data) {
      state.clients = data
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
        dispatch('getListClient', response.data)
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

    async getHabilitation({ commit, state }) {
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
      }
    },

    async getListClient({ commit, state }) {
      const response = await axios.get(
        `${process.env.WEB_SERVICE_WCF}/auth/habilitation/clients`,
        {
          params: {
            user: state.user,
            habilitation: state.habilitationName
          }
        }
      )
      if (response.status === 200) {
        commit('SET_CLIENTS', response.data)
      }
    }
  }
}
