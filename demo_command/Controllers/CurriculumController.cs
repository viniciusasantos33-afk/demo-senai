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
        public IActionResult Enviar(IFormFile arquivo)
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine("[SISTEMA] --- Início do Processamento de Upload ---");
            logBuilder.AppendLine($"[SISTEMA] Horário: {DateTime.Now.ToString("HH:mm:ss")}");

            // 1. Verifica se o framework barrou o arquivo ou se veio vazio
            if (arquivo == null || arquivo.Length == 0)
            {
                logBuilder.AppendLine("[ERRO] Arquivo nulo ou vazio detectado pelo validador do .NET.");
                ViewBag.DevDebugLog = logBuilder.ToString();
                ViewBag.Erro = "Por favor, selecione um arquivo válido.";
                return View("Index");
            }

            // Para garantir que o .NET não limpou o comando, pegamos o nome bruto enviado
            string nomeArquivo = arquivo.FileName;
            logBuilder.AppendLine($"[SISTEMA] Arquivo recebido: '{nomeArquivo}'");
            logBuilder.AppendLine("[SISTEMA] Executando rotina de verificação de integridade no terminal...");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();

                if (OperatingSystem.IsWindows())
                {
                    psi.FileName = "cmd.exe";
                    // Forçamos o echo para garantir que algo escreva no console
                    psi.Arguments = $"/c echo [CMD] Executando checagem... && dir {nomeArquivo}";
                }
                else
                {
                    psi.FileName = "/bin/bash";
                    psi.Arguments = $"-c \"echo '[BASH] Executando checagem...' && ls -la '{nomeArquivo}'\"";
                }

                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                logBuilder.AppendLine($"[DEBUG] Comando enviado ao OS: {psi.FileName} {psi.Arguments}");

                using (Process processo = Process.Start(psi))
                {
                    // Lendo as saídas para evitar deadlocks
                    string output = processo.StandardOutput.ReadToEnd();
                    string error = processo.StandardError.ReadToEnd();
                    processo.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        logBuilder.AppendLine("\n--- [SAÍDA DO TERMINAL] ---");
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
                logBuilder.AppendLine($"\n[EXCEÇÃO CRÍTICA] Falha ao iniciar processo: {ex.Message}");
            }

            logBuilder.AppendLine("[SISTEMA] --- Fim do Processamento ---");

            // Alimenta a View com o relatório completo
            ViewBag.Mensagem = "Sua candidatura foi recebida com sucesso. Obrigado!";
            ViewBag.DevDebugLog = logBuilder.ToString();

            return View("Index");
        }
    }
}