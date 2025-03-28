using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        // Configuração inicial do calendário
        int ano = ObterInteiroDoUsuario("Digite o ANO (ex: 2023):", 1900, 2100);
        int mes = ObterInteiroDoUsuario("Digite o MÊS (1-12):", 1, 12);
        string primeiroDiaSemana = ObterDiaSemanaValido("Digite o primeiro dia da semana do mês (ex: Segunda):");
        int ultimoDiaMes = DateTime.DaysInMonth(ano, mes);

        // Dicionário para armazenar horários por data (chave: dia do mês)
        Dictionary<int, Dictionary<string, int>> horariosPorData = new Dictionary<int, Dictionary<string, int>>();

        // Configurar horários padrão para cada dia da semana
        Console.WriteLine("\nConfigurar horários padrão para dias da semana:");
        var diasSemana = new List<string> { "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo" };
        Dictionary<string, Dictionary<string, int>> horariosPadraoPorDia = new Dictionary<string, Dictionary<string, int>>();

        foreach (var dia in diasSemana)
        {
            horariosPadraoPorDia[dia] = ObterHorariosParaDia(dia);
        }

        // Perguntar se deseja adicionar datas específicas
        if (PerguntarSimNao("\nDeseja incluir alguma data específica?"))
        {
            AdicionarDatasEspecificas(horariosPorData, ultimoDiaMes);
        }

        // Processar e exibir o calendário
        ProcessarCalendario(mes, ano, primeiroDiaSemana, ultimoDiaMes, horariosPadraoPorDia, horariosPorData);
    }

    static void AdicionarDatasEspecificas(Dictionary<int, Dictionary<string, int>> horariosPorData, int ultimoDiaMes)
    {
        while (true)
        {
            int dia = ObterInteiroDoUsuario($"\nDigite o dia do mês (1-{ultimoDiaMes}) ou 0 para sair:", 0, ultimoDiaMes);
            if (dia == 0) break;

            Console.WriteLine($"\nConfigurando horários para o dia {dia}:");
            var horariosDia = new Dictionary<string, int>();

            while (true)
            {
                Console.Write("Digite um horário (formato HH:mm) ou 'sair' para encerrar: ");
                string horario = Console.ReadLine();

                if (horario.ToLower() == "sair")
                    break;

                if (!ValidarFormatoHorario(horario))
                {
                    Console.WriteLine("Formato inválido. Use HH:mm (ex: 08:30 ou 19:45)");
                    continue;
                }

                int quantidade = ObterInteiroDoUsuario($"Digite a quantidade de vagas para {horario}:", 1, 100);
                horariosDia[horario] = quantidade;
            }

            if (horariosDia.Count > 0)
            {
                horariosPorData[dia] = horariosDia;
            }
            else
            {
                Console.WriteLine("Nenhum horário foi adicionado para esta data.");
            }
        }
    }

    static bool PerguntarSimNao(string mensagem)
    {
        while (true)
        {
            Console.WriteLine($"{mensagem} (S/N)");
            string resposta = Console.ReadLine().ToUpper();
            if (resposta == "S") return true;
            if (resposta == "N") return false;
            Console.WriteLine("Por favor, digite S para Sim ou N para Não");
        }
    }

    static Dictionary<string, int> ObterHorariosParaDia(string dia)
    {
        Dictionary<string, int> horarios = new Dictionary<string, int>();
        Console.WriteLine($"\nConfigurando horários para {dia}:");

        while (true)
        {
            Console.Write($"Digite um horário para {dia} (formato HH:mm) ou 'sair' para encerrar: ");
            string horario = Console.ReadLine();

            if (horario.ToLower() == "sair")
                break;

            if (!ValidarFormatoHorario(horario))
            {
                Console.WriteLine("Formato inválido. Use HH:mm (ex: 08:30 ou 19:45)");
                continue;
            }

            int quantidade = ObterInteiroDoUsuario($"Digite a quantidade de vagas para {dia} às {horario}:", 1, 100);
            horarios[horario] = quantidade;
        }

        return horarios;
    }

    static bool ValidarFormatoHorario(string horario)
    {
        if (horario.Length != 5 || horario[2] != ':')
            return false;

        if (!int.TryParse(horario.Substring(0, 2), out int horas) || horas < 0 || horas > 23)
            return false;

        if (!int.TryParse(horario.Substring(3, 2), out int minutos) || minutos < 0 || minutos > 59)
            return false;

        return true;
    }

    static int ObterInteiroDoUsuario(string mensagem, int min, int max)
    {
        while (true)
        {
            Console.WriteLine(mensagem);
            string input = Console.ReadLine();
            
            if (int.TryParse(input, out int resultado) && resultado >= min && resultado <= max)
            {
                return resultado;
            }
            Console.WriteLine($"Valor inválido. Digite um número entre {min} e {max}.");
        }
    }

    static string ObterDiaSemanaValido(string mensagem)
    {
        var diasValidos = new List<string> { "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo" };
        
        while (true)
        {
            Console.WriteLine(mensagem);
            string input = Console.ReadLine();
            
            if (diasValidos.Contains(input))
            {
                return input;
            }
            Console.WriteLine("Dia inválido. Os dias válidos são: " + string.Join(", ", diasValidos));
        }
    }

    static void ProcessarCalendario(int mes, int ano, string primeiroDiaSemana, int ultimoDiaMes, 
                                  Dictionary<string, Dictionary<string, int>> horariosPadraoPorDia,
                                  Dictionary<int, Dictionary<string, int>> horariosPorData)
    {
        var diasSemana = new List<string> { "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo" };

        // Ajusta a fila para começar no dia correto
        int indexInicio = diasSemana.IndexOf(primeiroDiaSemana);
        var filaDiasSemana = new Queue<string>();

        // Preenche a fila com os dias da semana na ordem correta
        for (int i = indexInicio; i < diasSemana.Count; i++)
        {
            filaDiasSemana.Enqueue(diasSemana[i]);
        }
        for (int i = 0; i < indexInicio; i++)
        {
            filaDiasSemana.Enqueue(diasSemana[i]);
        }

        // Preenche a fila com os dias do mês
        var filaDiasMes = new Queue<int>();
        for (int i = 1; i <= ultimoDiaMes; i++)
        {
            filaDiasMes.Enqueue(i);
        }

        // Exibe o cabeçalho
        Console.WriteLine($"\nCalendário do Mês: {mes}/{ano}");
        Console.WriteLine($"Primeiro dia: {primeiroDiaSemana}");
        Console.WriteLine($"Último dia: {ultimoDiaMes}\n");

        // Processa cada dia do mês
        while (filaDiasMes.Count > 0)
        {
            string diaSemana = filaDiasSemana.Dequeue();
            int diaMes = filaDiasMes.Dequeue();

            // Verifica se existem horários para esta data (específicos ou padrão)
            bool temHorariosEspecificos = horariosPorData.ContainsKey(diaMes) && horariosPorData[diaMes].Count > 0;
            bool temHorariosPadrao = horariosPadraoPorDia[diaSemana].Count > 0;

            // Mostra apenas dias com horários
            if (temHorariosEspecificos || temHorariosPadrao)
            {
                Console.WriteLine($" {diaSemana}, dia {diaMes}");

                // Prioriza horários específicos se existirem
                if (temHorariosEspecificos)
                {
                    foreach (var horario in horariosPorData[diaMes])
                    {
                        Console.WriteLine($" {horario.Key} - {horario.Value} vagas");
                    }
                }
                else
                {
                    foreach (var horario in horariosPadraoPorDia[diaSemana])
                    {
                        Console.WriteLine($" {horario.Key} - {horario.Value} vagas");
                    }
                }

                Console.WriteLine("----------------------");
            }

            filaDiasSemana.Enqueue(diaSemana);

            CreateTestMessage3();
        }
    }

    // envio de emails

    public static void CreateTestMessage3()
    {
        MailAddress to = new MailAddress("jorge.guedert@weipa.com.br");
        MailAddress from = new MailAddress("joca12855@gmail.com"); // Seu e-mail do Gmail
        MailMessage message = new MailMessage(from, to);
        message.Subject = "Using the new SMTP client.";
        message.Body = @"Using this new feature, you can send an email message from an application very easily.";

        // Configuração específica para Gmail
        SmtpClient client = new SmtpClient("smtp.gmail.com", 587); // Servidor e porta do Gmail
        client.EnableSsl = true; // SSL obrigatório
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential("joca12855@gmail.com", "smrp oylc sobv dmwz"); // Seu e-mail e senha

        Console.WriteLine("Sending an email message to {0} at {1} by using the SMTP host={2}.",
            to.User, to.Host, client.Host);

        try
        {
            client.Send(message);
            Console.WriteLine("E-mail enviado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
        }
        finally
        {
            message.Dispose();
        }
    }
}