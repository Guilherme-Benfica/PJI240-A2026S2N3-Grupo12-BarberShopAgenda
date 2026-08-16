const AUTH_STORAGE_KEY = "barbershop_auth";

function getAuth() {
  const bruto = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!bruto) return null;

  try {
    const auth = JSON.parse(bruto);
    if (!auth?.token || !auth?.expiraEm) return null;
    if (new Date(auth.expiraEm) <= new Date()) {
      localStorage.removeItem(AUTH_STORAGE_KEY);
      return null;
    }
    return auth;
  } catch {
    return null;
  }
}

function salvarAuth(auth) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
}

function logout() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
  window.location.href = "login.html";
}

function isAuthenticated() {
  return getAuth() !== null;
}

/** Página inicial de cada papel — usada tanto após o login quanto quando um usuário autenticado
 * tenta acessar uma página que não é dele, evitando redirecionar de volta para login.html
 * (o que criaria um loop infinito: página nega acesso → login → login vê sessão válida → página). */
function paginaInicialPara(papel) {
  if (papel === "Barbeiro") return "agendamentos.html";
  if (papel === "Cliente") return "minha-conta.html";
  return "index.html";
}

/** Garante que o usuário está autenticado e tem um dos papéis permitidos; senão, redireciona
 * para o login (se não autenticado) ou para a página inicial do próprio papel (se autenticado
 * mas sem permissão aqui). */
function requireAuth(papeisPermitidos) {
  const auth = getAuth();
  if (!auth) {
    window.location.href = "login.html";
    return null;
  }
  if (!papeisPermitidos.includes(auth.papel)) {
    window.location.href = paginaInicialPara(auth.papel);
    return null;
  }
  renderizarBadgeUsuario(auth);
  esconderLinksNaoPermitidos(auth.papel);
  return auth;
}

/** Esconde da navegação os links que o papel atual não tem permissão de acessar
 * (ex.: Barbeiro não pode entrar em Dashboard/Clientes/Barbeiros/Serviços — são só do Admin),
 * pra evitar que o usuário clique num link que só vai jogá-lo de volta. */
function esconderLinksNaoPermitidos(papel) {
  if (papel === "Admin") return;

  const paginasSoDeAdmin = ["index.html", "clientes.html", "barbeiros.html", "servicos.html"];
  document.querySelectorAll(".navegacao a").forEach((link) => {
    const pagina = link.getAttribute("href");
    if (paginasSoDeAdmin.includes(pagina)) {
      link.remove();
    }
  });
}

function renderizarBadgeUsuario(auth) {
  const cabecalho = document.querySelector(".cabecalho");
  if (!cabecalho) return;

  const rotulosPapel = { Admin: "Administrador", Barbeiro: "Barbeiro", Cliente: "Cliente" };

  const badge = document.createElement("div");
  badge.className = "badge-usuario";
  badge.innerHTML = `
    <span>${escaparHtmlAuth(auth.nome)} <span class="papel-usuario">${rotulosPapel[auth.papel] || auth.papel}</span></span>
    <a href="trocar-senha.html" class="botao-secundario botao-pequeno">Trocar senha</a>
    <button type="button" class="botao-secundario botao-pequeno botao-sair" aria-label="Sair da conta">Sair</button>
  `;
  cabecalho.appendChild(badge);
  badge.querySelector(".botao-sair").addEventListener("click", logout);
}

function escaparHtmlAuth(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}
