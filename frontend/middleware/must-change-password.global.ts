export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()
  
  // Sprawdź czy użytkownik jest zalogowany
  if (!authStore.isAuthenticated) {
    return
  }
  
  // Sprawdź czy użytkownik musi zmienić hasło
  const mustChangePassword = authStore.user?.mustChangePassword === true
  
  // Jeśli użytkownik musi zmienić hasło i nie jest na stronie zmiany hasła
  if (mustChangePassword && to.path !== '/auth/change-password' && to.path !== '/auth/logout') {
    return navigateTo('/auth/change-password')
  }
  
  // Jeśli użytkownik nie musi zmieniać hasła i jest na stronie zmiany hasła (próbuje wejść ręcznie)
  // pozwól mu tam wejść (może chce zmienić hasło dobrowolnie)
})

