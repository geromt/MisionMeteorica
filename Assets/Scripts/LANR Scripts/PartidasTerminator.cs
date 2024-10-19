using UnityEngine;


public class PartidasTerminator : MonoBehaviour
{
    //Si se detectan partidas que no se han subido a CITAN indicarle al usuario con un panel que se van a subir las partidas
    //Este panel debe tener un boton que diga continuar que mandará llamar a la función Guardar de este script
    public GameObject avisoPartidasPendientesPanel;

    private PlayerData _bitacoraDelJugador;
    private Partida _nuevaPartida;
    private ManejadorXMLs _miXML;

    private void OnEnable()
    {
        AdminController.OnDataUpload += IntentaSubirLaPartidaA_CITAN;
        AduanaCITAN.partidaSubidaConExito += MarcaLaPartidaComoSubidaYArchivala;
        AduanaCITAN.partidaNoSePudoSubir += MarcaLaPartidaComoNOSubidaYArchivala;
    }

    private void OnDisable()
    {
        AdminController.OnDataUpload -= IntentaSubirLaPartidaA_CITAN;
        AduanaCITAN.partidaSubidaConExito -= MarcaLaPartidaComoSubidaYArchivala;
        AduanaCITAN.partidaNoSePudoSubir -= MarcaLaPartidaComoNOSubidaYArchivala;
    }

    /* En la clase Partida tambien se tienen arreglos que almacenan No de aciertos, fallos y falsos aciertos, sin embargo, primero 
     * se guardaran dichos datos de manera "local" y al final del juego se guardaran formalmente en el archivo. En una relacion 
     * 1 a 1 con los elementos de la clase Partida
	 */
    private void Awake()
    {
        _bitacoraDelJugador = new PlayerData();
        _bitacoraDelJugador.id = GameMaster.IdPaciente;
        _bitacoraDelJugador.nombre = GameMaster.NombrePaciente;
        _nuevaPartida = new Partida();
        _miXML = new ManejadorXMLs();
    }

    private void IntentaSubirLaPartidaA_CITAN()
    {
        _nuevaPartida = CreaContenedorDeLaPartidaJugada();
    }

    private Partida CreaContenedorDeLaPartidaJugada()
    {
        Partida partidaRecienJugada = GameMaster.Partida;

        StartCoroutine(AduanaCITAN.SubePartidasA_CITAN(partidaRecienJugada, GameMaster.IdPaciente.ToString()));
        return partidaRecienJugada;
    }

    private void MarcaLaPartidaComoSubidaYArchivala()
    {
        Debug.Log("Solo debo entrar a este evento una sola vez, que es cuando subo a la BD la partida que acabo de jugar. Pero puede volver a lanzarse si es que hay partidas sin guardar, pero evitamos eso con una bandera.");
        _nuevaPartida.guardado = 1;

        _bitacoraDelJugador.HistorialPartidas.Add(_nuevaPartida);
        _miXML.CreaHistorialPartidasXML(_bitacoraDelJugador, GameMaster.RutaHistorialPartidasPaciente, true);
        Debug.Log("En caso de que hubiera iniciado sin Internet y durante la partida se haya restablecido la conexión voy a ver si hay partidas sin guardar en la BD. Quien se encargará de cambiar el estado del ModoSinConexion es Paciente.cs o SeleccionDispo.cs");
        Debug.Log("Todas las partidas estan en la BD avanzando a a escena de Resultados");
    }

    void MarcaLaPartidaComoNOSubidaYArchivala()
    {
        _nuevaPartida.guardado = 0;
        _bitacoraDelJugador.HistorialPartidas.Add(_nuevaPartida);
        _miXML.CreaHistorialPartidasXML(_bitacoraDelJugador, GameMaster.RutaHistorialPartidasPaciente, true);
        Debug.Log("Nunca tuve Internet de todas formas. No mostramos ningun panel. Continuamos con la escena de Resultados.");
    }
}