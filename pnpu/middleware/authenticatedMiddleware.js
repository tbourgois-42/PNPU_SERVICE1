export default function({ store, redirect }) {
  // If the user is not authenticated
  if (!store.getters['modules/auth/authenticated']) {
    return redirect('/signin')
  }
}
