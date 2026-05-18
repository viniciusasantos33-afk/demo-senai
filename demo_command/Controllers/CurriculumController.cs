using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace demo_command.Controllers
{
    public class CurriculumController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Permite testar via HTTPie/cURL sem o token de segurança CSRF
        public IActionResult Enviar(IFormFile arquivo)
        {
            // AQUI DEFINIMOS O LOGBUILDER! Ele registra tudo o que acontece no método
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine("[SISTEMA] --- Inicio do Processamento de Upload ---");
            logBuilder.AppendLine($"[SISTEMA] Horario: {DateTime.Now.ToString("HH:mm:ss")}");

            // 1. Verifica se o arquivo veio nulo ou vazio
            if (arquivo == null || arquivo.Length == 0)
            {
                logBuilder.AppendLine("[ERRO] Arquivo nulo ou vazio detectado pelo validador.");
                ViewBag.DevDebugLog = logBuilder.ToString();
                ViewBag.Erro = "Por favor, selecione um arquivo valido.";
                return View("Index");
            }

            // Captura o nome do arquivo enviado pelo usuário (vetor de injeção)
            string nomeArquivo = arquivo.FileName;
            logBuilder.AppendLine($"[SISTEMA] Arquivo recebido: '{nomeArquivo}'");
            logBuilder.AppendLine("[SISTEMA] Executando rotina de verificacao no terminal...");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();

                if (OperatingSystem.IsWindows())
                {
                    psi.FileName = "cmd.exe";
                    // Concatenação vulnerável: junta o nome do arquivo direto no comando
                    psi.Arguments = $"/c echo [CMD] Executando checagem... && dir {nomeArquivo}";
                }
                else
                {
                    psi.FileName = "/bin/bash";
                    psi.Arguments = $"-c \"echo '[BASH] Executando...' && ls -la '{nomeArquivo}'\"";
                }

                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                logBuilder.AppendLine($"[DEBUG] Comando enviado ao OS: {psi.FileName} {psi.Arguments}");

                using (Process processo = Process.Start(psi))
                {
                    string output = processo.StandardOutput.ReadToEnd();
                    string error = processo.StandardError.ReadToEnd();
                    processo.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        logBuilder.AppendLine("\n--- [SAIDA DO TERMINAL] ---");
                        logBuilder.AppendLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        logBuilder.AppendLine("\n--- [ERRO DO TERMINAL] ---");
                        logBuilder.AppendLine(error);
                    }
                }
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"\n[EXCECAO CRITICA] Falha ao iniciar processo: {ex.Message}");
            }

            logBuilder.AppendLine("[SISTEMA] --- Fim do Processamento ---");

            // =======================================================================
            // TRATAMENTO DO CABEÇALHO PARA EVITAR O ERRO DE ACENTUAÇÃO (KESTREL)
            // =======================================================================

            // 1. Converte o logBuilder para string e limpa quebras de linha/tabs
            string resultadoTexto = logBuilder.ToString();
            string resultadoSanitizadoParaHeader = resultadoTexto
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("   ", " ");

            // 2. CORREÇÃO EXCLUSIVA: Remove acentos usando Normalização Nativa do .NET
            // Separa os caracteres dos acentos (Ex: 'ã' vira 'a' + '~')
            string stringNormalizada = resultadoSanitizadoParaHeader.Normalize(NormalizationForm.FormD);
            
            StringBuilder sbSemAcentos = new StringBuilder();
            foreach (char c in stringNormalizada)
            {
                // Verifica na tabela Unicode se o caractere NÃO é um acento/marca gráfica
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sbSemAcentos.Append(c);
                }
            }

            // Reconstroi o texto totalmente limpo de acentos
            string headerSemAcentos = sbSemAcentos.ToString();

            // 3. Mantém apenas caracteres visíveis padrão da tabela ASCII (32 a 126)
            headerSemAcentos = new string(headerSemAcentos.Where(c => c >= 32 && c <= 126).ToArray());

            // 4. Injeta com segurança no cabeçalho de resposta HTTP
            Response.Headers.Add("X-Terminal-Output", headerSemAcentos);

            // =======================================================================

            // Alimenta a View com as informações (o navegador aceita acentos normalmente)
            ViewBag.Mensagem = "Sua candidatura foi recebida com sucesso. Obrigado!";
            ViewBag.DevDebugLog = resultadoTexto;

            return View("Index");
        }
    }
}