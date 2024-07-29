# LerCsvNubank
README - LerCsvNubank

Descrição
O projeto LerCsvNubank tem como objetivo ler múltiplos arquivos CSV da Nubank a partir de um diretório especificado, consolidar todos os dados de transações em um único arquivo CSV, e gerar um arquivo final para melhor visualização e análise.

Estrutura do Projeto
O projeto contém as seguintes classes:
CsvNubank
Transaction
TransactionMap

Como Usar
Utilize o método Start da classe CsvNubank, passando o caminho da pasta contendo os arquivos CSV da Nubank e o caminho completo do arquivo CSV final que será gerado.
using LerCsvNubank;

class Program
{

    static void Main(string[] args)
    {
        string caminhoDaPastaOrigemCsv = @"C:\Caminho\Para\Os\ArquivosCSV";
        string caminhoArquivoFinalCsv = @"C:\Caminho\Para\Salvar\NubankTotal.csv";
        CsvNubank.Start(caminhoDaPastaOrigemCsv, caminhoArquivoFinalCsv);
    }
}


sta classe contém o método Start, responsável por ler os arquivos CSV da Nubank, consolidar os dados e gerar o arquivo CSV final.

Método Start:
Define a cultura para pt-BR (Português do Brasil).
Lê todos os arquivos CSV do diretório especificado.
Para cada arquivo, lê linha por linha e extrai os dados de transação.
Cria uma lista de transações (Transaction).
Gera um novo arquivo CSV com todas as transações consolidadas.
Classe Transaction
Representa uma transação financeira.

Propriedades:

Data: Data da transação.
Valor: Valor da transação.
Categoria: Categoria da transação (Despesa ou Receita).
Descricao: Descrição da transação.

Construtor:

Define a categoria da transação com base no valor (positivo para receita, negativo para despesa).
Classe TransactionMap
Mapeamento da classe Transaction para o CSV.

Configuração:
Define o formato da data como "dd/MM/yyyy".
Mapeia as propriedades Data, Valor, Categoria e Descricao.
Observações
Certifique-se de que os arquivos CSV da Nubank estejam no formato correto, com as colunas esperadas.
Verifique as permissões de leitura/escrita nos diretórios especificados.
Em caso de erro, o método Start exibirá a mensagem de exceção no console.
Contato
Para dúvidas ou sugestões, entre em contato pelo luccastrindadeguimaraes@gmail.com.
