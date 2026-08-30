document.addEventListener("DOMContentLoaded", () => {
  const feedback = document.getElementById("feedback");
  const secaoFormulario = document.getElementById("secao-formulario");
  const secaoSucesso = document.getElementById("secao-sucesso");
  const form = document.getElementById("form-redefinir-senha");
  const campoSenha = document.getElementById("redefinir-senha");
  const campoConfirmarSenha = document.getElementById("redefinir-confirmar-senha");

  const parametros = new URLSearchParams(window.location.search);
  const token = parametros.get("token");

  if (!token) {
    mostrarFeedback(feedback, "Link inválido — falta o token de redefinição.", "erro");
    form.querySelector("button[type=submit]").disabled = true;
    return;
  }

  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (campoSenha.value !== campoConfirmarSenha.value) {
      mostrarFeedback(feedback, "As senhas não conferem.", "erro");
      return;
    }

    const botaoRedefinir = form.querySelector("button[type=submit]");
    botaoRedefinir.disabled = true;

    try {
      await api.auth.redefinirSenha(token, campoSenha.value);
      secaoFormulario.hidden = true;
      secaoSucesso.hidden = false;
    } catch (erro) {
      mostrarFeedback(feedback, erro.message || "Não foi possível redefinir a senha. O link pode ter expirado.", "erro");
    } finally {
      botaoRedefinir.disabled = false;
    }
  });
});
