document.addEventListener("DOMContentLoaded", async () => {
  const mensagem = document.getElementById("mensagem-confirmacao");
  const linkLogin = document.getElementById("link-login");

  const parametros = new URLSearchParams(window.location.search);
  const token = parametros.get("token");

  if (!token) {
    mensagem.textContent = "Link inválido — falta o token de confirmação.";
    return;
  }

  try {
    await api.auth.confirmarEmail(token);
    mensagem.textContent = "E-mail confirmado com sucesso! Você já pode entrar.";
    linkLogin.hidden = false;
  } catch (erro) {
    mensagem.textContent = erro.message || "Não foi possível confirmar o e-mail. O link pode ter expirado.";
  }
});
