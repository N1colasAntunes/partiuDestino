using MySql.Data.MySqlClient;

namespace projectPartiuDestino.Data
{
    public class Conexao
    {
        //altera aqui pra sua senha e o seu root, o meu ta diferente do padrão
        private string dadosConexao =
            "Server=localhost;Database=bdpartiudestino;Uid=root;Pwd=nicolas123;";

        public MySqlConnection ObterConexao()
        {
            return new MySqlConnection(dadosConexao);
        }
    }
}
