function checkAuthentication() {
    // Verifique se o usuário está autenticado comparando com o valor no localStorage
    const isAuthenticated = localStorage.getItem('authenticated');

    // Se isAuthenticated for verdadeiro, o usuário está autenticado
    return isAuthenticated === 'true';
}

// Verificar a autenticação antes de permitir o acesso à dashboard
window.addEventListener('DOMContentLoaded', () => {
    if (!checkAuthentication()) {
        // Se o usuário não estiver autenticado, redirecione para a página de login
        window.location.href = 'login.html';
    }
});