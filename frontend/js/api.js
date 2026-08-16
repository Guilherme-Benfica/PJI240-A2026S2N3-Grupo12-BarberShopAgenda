// Configuração base da API. Ajuste conforme o endereço do backend.
const API_BASE_URL = window.BARBERSHOP_API_URL || "https://localhost:7001/api";

class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.status = status;
  }
}

async function apiRequest(caminho, opcoes = {}) {
  const auth = typeof getAuth === "function" ? getAuth() : null;
  const cabecalhos = { "Content-Type": "application/json", ...(opcoes.headers || {}) };
  if (auth?.token) {
    cabecalhos["Authorization"] = `Bearer ${auth.token}`;
  }

  const resposta = await fetch(`${API_BASE_URL}${caminho}`, {
    ...opcoes,
    headers: cabecalhos,
  });

  if (resposta.status === 401 && auth && typeof logout === "function") {
    logout();
    return null;
  }

  if (resposta.status === 204) {
    return null;
  }

  let corpo = null;
  const texto = await resposta.text();
  if (texto) {
    try {
      corpo = JSON.parse(texto);
    } catch {
      corpo = null;
    }
  }

  if (!resposta.ok) {
    const mensagem = corpo?.mensagem || corpo?.title || `Erro na requisição (${resposta.status}).`;
    throw new ApiError(mensagem, resposta.status);
  }

  return corpo;
}

const api = {
  clientes: {
    listar: () => apiRequest("/clientes"),
    buscar: (id) => apiRequest(`/clientes/${id}`),
    criar: (dados) => apiRequest("/clientes", { method: "POST", body: JSON.stringify(dados) }),
    atualizar: (id, dados) => apiRequest(`/clientes/${id}`, { method: "PUT", body: JSON.stringify(dados) }),
    remover: (id) => apiRequest(`/clientes/${id}`, { method: "DELETE" }),
  },
  barbeiros: {
    listar: () => apiRequest("/barbeiros"),
    listarTodos: () => apiRequest("/barbeiros/todos"),
    buscar: (id) => apiRequest(`/barbeiros/${id}`),
    criar: (dados) => apiRequest("/barbeiros", { method: "POST", body: JSON.stringify(dados) }),
    atualizar: (id, dados) => apiRequest(`/barbeiros/${id}`, { method: "PUT", body: JSON.stringify(dados) }),
    ativarConta: (id) => apiRequest(`/barbeiros/${id}/conta/ativar`, { method: "PUT" }),
    inativarConta: (id) => apiRequest(`/barbeiros/${id}/conta/inativar`, { method: "PUT" }),
  },
  servicos: {
    listar: () => apiRequest("/servicos"),
    criar: (dados) => apiRequest("/servicos", { method: "POST", body: JSON.stringify(dados) }),
    atualizar: (id, dados) => apiRequest(`/servicos/${id}`, { method: "PUT", body: JSON.stringify(dados) }),
  },
  agendamentos: {
    listar: () => apiRequest("/agendamentos"),
    buscar: (id) => apiRequest(`/agendamentos/${id}`),
    porBarbeiro: (barbeiroId) => apiRequest(`/agendamentos/barbeiro/${barbeiroId}`),
    porCliente: (telefone, codigo) =>
      apiRequest(`/agendamentos/cliente?telefone=${encodeURIComponent(telefone)}&codigo=${encodeURIComponent(codigo)}`),
    minhaConta: () => apiRequest("/agendamentos/me"),
    porData: (data) => apiRequest(`/agendamentos/data/${data}`),
    criar: (dados) => apiRequest("/agendamentos", { method: "POST", body: JSON.stringify(dados) }),
    confirmar: (id) => apiRequest(`/agendamentos/${id}/confirmar`, { method: "PUT" }),
    cancelar: (id) => apiRequest(`/agendamentos/${id}/cancelar`, { method: "PUT" }),
    concluir: (id) => apiRequest(`/agendamentos/${id}/concluir`, { method: "PUT" }),
  },
  dashboard: {
    hoje: () => apiRequest("/dashboard/hoje"),
  },
  auth: {
    login: (dados) => apiRequest("/auth/login", { method: "POST", body: JSON.stringify(dados) }),
    registrar: (dados) => apiRequest("/auth/registrar", { method: "POST", body: JSON.stringify(dados) }),
    confirmarEmail: (token) => apiRequest("/auth/confirmar-email", { method: "POST", body: JSON.stringify({ token }) }),
    esqueciSenha: (email) => apiRequest("/auth/esqueci-senha", { method: "POST", body: JSON.stringify({ email }) }),
    redefinirSenha: (token, novaSenha) =>
      apiRequest("/auth/redefinir-senha", { method: "POST", body: JSON.stringify({ token, novaSenha }) }),
    alterarSenha: (senhaAtual, novaSenha) =>
      apiRequest("/auth/senha", { method: "PUT", body: JSON.stringify({ senhaAtual, novaSenha }) }),
  },
  horarios: {
    disponiveis: (barbeiroId, data, servicoId) =>
      apiRequest(`/horarios/disponiveis?barbeiroId=${barbeiroId}&data=${data}&servicoId=${servicoId}`),
  },
};

function mostrarFeedback(elemento, mensagem, tipo = "sucesso") {
  elemento.textContent = mensagem;
  elemento.className = `feedback visivel ${tipo}`;
  elemento.setAttribute("role", tipo === "erro" ? "alert" : "status");
  if (tipo === "sucesso") {
    setTimeout(() => {
      elemento.className = "feedback";
    }, 4000);
  }
}

function formatarMoeda(valor) {
  return new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(valor);
}

function formatarDataHora(dataHoraISO) {
  const data = new Date(dataHoraISO);
  return data.toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" });
}

const STATUS_LABEL = {
  Pendente: "Pendente",
  Confirmado: "Confirmado",
  Cancelado: "Cancelado",
  Concluido: "Concluído",
};

function badgeStatus(status) {
  const classe = `badge-${status.toLowerCase()}`;
  return `<span class="badge ${classe}">${STATUS_LABEL[status] || status}</span>`;
}
