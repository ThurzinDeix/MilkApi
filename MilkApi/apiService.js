// apiService.js

const API_BASE_URL = "http://localhost:7092"; // ajuste para a porta da sua API
const userId = localStorage.getItem("userId");

if (!userId) {
    console.warn("⚠ Nenhum userId encontrado no localStorage. Verifique o login.");
}

async function fetchData(endpoint) {
    try {
        const response = await fetch(`${API_BASE_URL}/${endpoint}`);
        if (!response.ok) {
            throw new Error(`Erro ao buscar ${endpoint}: ${response.statusText}`);
        }
        return await response.json();
    } catch (error) {
        console.error("Erro na API:", error);
        return null;
    }
}

export const ApiService = {
    // Usuário
    getUsuario: () => fetchData(`Usuario/${userId}`),

    // Telefones
    getTelefones: () => fetchData(`Telefone/${userId}`),

    // Vacinas
    getVacinas: () => fetchData(`Vacina/${userId}`),
    getHistoricoVacinas: () => fetchData(`HistoricoVacina/${userId}`),
    getTiposVacina: () => fetchData("TipoVacina"), // esse geralmente não depende do usuário

    // Manejo
    getManejoGeral: () => fetchData(`ManejoGeral/${userId}`),

    // Reprodução
    getReproducao: () => fetchData(`Reproducao/${userId}`),

    // Leite
    getLeite: () => fetchData(`Leite/${userId}`),

    // Qualidade do leite (se o controller for por lote)
    getQualidadeLeite: (loteId) => fetchData(`QualidadeLeite/${loteId}`),

    // Gado
    getGado: () => fetchData(`Gado/${userId}`),

    // Fazenda
    getFazenda: () => fetchData(`Fazenda/${userId}`),

    // Alertas
    getAlertas: () => fetchData(`Alertas/${userId}`)
};
