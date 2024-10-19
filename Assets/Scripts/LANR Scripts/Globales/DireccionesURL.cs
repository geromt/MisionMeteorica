public class DireccionesURL
{
    private static string ligaParaProbarSiHayInternet = "https://lanr.ifc.unam.mx/unity";
    public static string LigaParaProbarSiHayInternet
    {
        get
        {
            return ligaParaProbarSiHayInternet;
        }
    }

    //private static string id_NombrePaciente =  "http://132.248.16.11:8000/api/index/"; 	//direccion que recibe el Id del paciente y devuelve su nombre y apellidos
    private static string id_NombrePaciente = "https://lanr.ifc.unam.mx/citan/public/api/index/";   //direccion que recibe el Id del paciente y devuelve su nombre y apellidos
    public static string Id_NombrePaciente
    {
        get
        {
            return id_NombrePaciente;
        }
    }

    private static string id_ContraseniaPaciente = "https://lanr.ifc.unam.mx/unity/validaUsuario.php";  //direccion que recibe el Id del paciente y devuelve su contraseña
    public static string Id_ContraseniaPaciente
    {
        get
        {
            return id_ContraseniaPaciente;
        }
    }

    private static string idTerapeuta_ListaPacientes = "https://lanr.ifc.unam.mx/citan/public/api/checkUser/";
    public static string IdTerapeuta_ListaPacientes
    {
        get
        {
            return idTerapeuta_ListaPacientes;
        }
    }

    //private static string email_NombreTerapeutaID = "http://132.248.16.11:8000/api/checkDoctor/";
    private static string email_NombreTerapeutaID = "https://lanr.ifc.unam.mx/citan/public/api/checkDoctor/";
    public static string Email_NombreTerapeutaID
    {
        get
        {
            return email_NombreTerapeutaID;
        }
    }

    //private static string emailPassword_TrueFalse = "http://132.248.16.11:8000/authGame/";
    private static string emailPassword_TrueFalse = "https://lanr.ifc.unam.mx/citan/public/authGame/";
    public static string EmailPassword_TrueFalse
    {
        get
        {
            return emailPassword_TrueFalse;
        }
    }

    private static string obtenResultadosJuego = "https://lanr.ifc.unam.mx/unity/obtenResultadosMision.php";
    public static string ObtenResultadosJuego
    {
        get
        {
            return obtenResultadosJuego;
        }
    }


    private static string insertaPartidasEnBD = "https://lanr.ifc.unam.mx/unity/insertaMision.php";
    public static string InsertaPartidasEnBD
    {
        get
        {
            return insertaPartidasEnBD;
        }
    }

    private static string directorioRutinasJuego = "https://lanr.ifc.unam.mx/unity/RutinasMisionMeteorica/";
    public static string DirectorioRutinasJuego
    {
        get
        {
            return directorioRutinasJuego;
        }
    }

    private static string idPacienteIdJuego_RevisaSiTieneRutina = "https://lanr.ifc.unam.mx/unity/rutinaAsignada.php"; //este php nos devuelve el nombre de la rutina que el paciente tenga asignada, en caso de no tener devuelve "Ninguna"
    public static string IdPacienteIdJuego_RevisaSiTieneRutina
    {
        get
        {
            return idPacienteIdJuego_RevisaSiTieneRutina;
        }
    }

    private static string idPacienteIdJuego_ActualizaRutina = "https://lanr.ifc.unam.mx/unity/actualizaRutina.php"; //este php nos devuelve el nombre de la rutina que el paciente tenga asignada, en caso de no tener devuelve "Ninguna"
    public static string IdPacienteIdJuego_ActualizaRutina
    {
        get
        {
            return idPacienteIdJuego_ActualizaRutina;
        }
    }

    //private string _urlAsignaRutina="https://lanr.ifc.unam.mx/unity/actualizaRutinaConSets.php"; //este script hace un update a la tabla medical_histories en el campo nombre rutina
    private static string idRutinaName_AsignaRutina = "https://lanr.ifc.unam.mx/unity/actualizaRutinaConSets.php"; //este script hace un update a la tabla routines, recibe el id del paciente y el nombre de la rutina
    public static string IdRutinaName_AsignaRuti
    {
        get
        {
            return idRutinaName_AsignaRutina;
        }
    }

    private static string idTerapeutaNombreJuego_CreaZipRutinas = "https://lanr.ifc.unam.mx/unity/createZIP.php?id=";//crea (si existen) un ZIP de todas las rutinas creadas por el terapeuta 
    public static string IdTerapeutaNombreJuego_CreaZipRutinas
    {
        get
        {
            return idTerapeutaNombreJuego_CreaZipRutinas;
        }
    }

    private static string idTerapeuta_BorraZip = "https://lanr.ifc.unam.mx/unity/deleteZIP.php?id="; //borra un archivo ZIP del servidor, se tiene que enviar el id del terapeuta
    public static string IdTerapeuta_BorraZip
    {
        get
        {
            return idTerapeuta_BorraZip;
        }
    }

    private static string subeRutinaACITAN = "https://lanr.ifc.unam.mx/unity/uploadXML.php";
    public static string SubeRutinaACITAN
    {
        get
        {
            return subeRutinaACITAN;
        }
    }
}
