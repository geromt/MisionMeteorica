//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class Resultados : MonoBehaviour
//{
//    [Tooltip("Info Aciertos")]
//    [SerializeField]
//    private Text aciertos_text;

//    [Tooltip("Info Errores")]
//    [SerializeField]
//    private Text errores_text;

//    [Tooltip("Info Falsos Aciertos")]
//    [SerializeField]
//    private Text falsosAciertos_text;

//    [Tooltip("Info Mano Utilizada")]
//    [SerializeField]
//    private Text manoUtilizada_text;

//    [Tooltip("Info Tipo Movimiento")]
//    [SerializeField]
//    private Text tipoMovimiento_text;

//    [Tooltip("Info Modalidad")]
//    [SerializeField]
//    private Text modalidad_text;

//    [Tooltip("Info Nivel")]
//    [SerializeField]
//    private Text nivel_text;

//    [Tooltip("Info Angulo Máximo")]
//    [SerializeField]
//    private Text anguloMaximo_text;

//    [Tooltip("Info Tiempo Mantenido")]
//    [SerializeField]
//    private Text tiempoMantenido_text;

//    [Tooltip("Info Duracion Total")]
//    [SerializeField]
//    private Text duracionTotal;

//    private int falsosAciertos;
//    private int errores;
//    private int aciertos;
//    private int numRep;
//    private List<float> tiemposMantenidos;
//    private float tiempo;
//    private List<float> promedioTiemposMantenidos;

//    private Configuracion.TipoMov tipoMov;

//    private void Awake()
//    {
//        promedioTiemposMantenidos = new List<float>();
//        tipoMov = GameMaster.TipoMov;
//        falsosAciertos = GameMaster.FalsosAciertos;
//        errores = GameMaster.Errores;
//        numRep = GameMaster.NumRepeticiones;
//        tiemposMantenidos = GameMaster.TiemposMantenidos;

//        aciertos = numRep * 10 - falsosAciertos;
//        Debug.Log(tiemposMantenidos);
//        Debug.Log(GameMaster.AciertosPorSerie);
//        if (GameMaster.Modalidad == Configuracion.Modalidad.Mantener)
//        {
//            for (int i = 0; i < numRep; i++)
//            {
//                tiempo = 0f;
//                for (int j = i * 10; j < (tiemposMantenidos.Count / numRep) * (i + 1); j++)
//                {
//                    tiempo += tiemposMantenidos[j];
//                }
//                if (GameMaster.AciertosPorSerie[i] == 0)
//                {
//                    promedioTiemposMantenidos.Add(0);
//                }
//                else
//                {
//                    promedioTiemposMantenidos.Add(tiempo / GameMaster.AciertosPorSerie[i]);
//                }
//            }
//        }
//    }

//    void Start()
//    {
//        aciertos_text.text = "Aciertos: " + aciertos;
//        errores_text.text = "Errores: " + errores;
//        if (GameMaster.Modalidad == Configuracion.Modalidad.Mantener)
//        {
//            falsosAciertos_text.text = "Falsos Aciertos: " + falsosAciertos;
//        }
//        else
//        {
//            falsosAciertos_text.text = "Falsos Aciertos: No Aplica";
//        }
//        manoUtilizada_text.text += GameMaster.ManoAUtilizar.ToString();
//        tipoMovimiento_text.text += GameMaster.TipoMov_texto;
//        modalidad_text.text += GameMaster.Modalidad_texto;
//        nivel_text.text += GameMaster.Click_texto;
//        anguloMaximo_text.text = "Rango de movilidad vertical: (" + GameMaster.AnguloMaxSuperior + ", " + GameMaster.AnguloMaxInferior + ")";
//        anguloMaximo_text.text += "\nRango de movilidad horizontal: (" + GameMaster.AnguloMaxIzquierda + ", " + GameMaster.AnguloMaxDerecha + ")";
//        if (GameMaster.Modalidad == Configuracion.Modalidad.Mantener)
//        {
//            for (int i = 1; i <= numRep; i++)
//            {
//                tiempoMantenido_text.text += "\n\t\tSerie " + i + ": " + promedioTiemposMantenidos[i - 1] + " segundos";
//            }
//        }
//        else
//        {
//            tiempoMantenido_text.text += "\n\tNo Aplica";
//        }
//        duracionTotal.text += "\n" + GameMaster.DuracionTotalPartida;
//    }

//    public void RegresarAlMenu()
//    {
//        SceneManager.LoadScene("LoginOC");
//    }
//}