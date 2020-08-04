import Vuex from 'vuex'
import auth from './modules/auth'

export const store = new Vuex.Store({
  namespaced: true,

  state: () => ({}),
  mutations: {
    increment(state) {
      state.counter++
    }
  },
  actions: {},
  modules: {
    auth
  }
})
