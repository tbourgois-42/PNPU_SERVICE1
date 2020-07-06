export default ({ store }) => {
  store.app.router.beforeEach((to, from, next) => {
    if (!store.getters['modules/auth/authenticated'] && to.name !== 'SignIn') {
      next({
        name: 'SignIn'
      })
    } else {
      store.dispatch('modules/auth/attempt', localStorage.getItem('token'))
      next()
    }
  })
}
